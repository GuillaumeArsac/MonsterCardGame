using NUnit.Framework;
using UnityEngine;
using MonsterCardGame.Gameplay.Encounters;
using MonsterCardGame.Gameplay.Monsters;
using MonsterCardGame.Gameplay.Skills;
using MonsterCardGame.Gameplay.World;

namespace MonsterCardGame.Tests.EditMode
{
    /// <summary>Combats des zones et déblocage des Ruines. Référence : docs/progression.md (Combats), docs/scenario/zones.</summary>
    public class WorldTests
    {
        private TestCatalog _catalog;
        private EncounterData _patrol, _thrak, _vaelisse, _training;
        private ZoneData _plains, _ruins;
        private WorldProgressionService _world;

        [SetUp]
        public void SetUp()
        {
            _catalog  = new TestCatalog();
            _patrol   = Encounter("patrouille", EncounterType.Scenario);
            _thrak    = Encounter("thrak", EncounterType.Scenario);
            _vaelisse = Encounter("vaelisse", EncounterType.Scenario);
            _training = Encounter("entrainement", EncounterType.Free);

            _plains = Zone("Plaines", false);
            _plains.SetEncounters(new[] { _patrol, _thrak, _vaelisse }, new[] { _training }, _vaelisse);
            _ruins = Zone("Ruines", true);

            var world = _catalog.Track(ScriptableObject.CreateInstance<WorldData>());
            world.SetZones(new[] { _plains, _ruins });
            _world = new WorldProgressionService(world);
        }

        [TearDown] public void TearDown() => _catalog.Dispose();

        private EncounterData Encounter(string id, EncounterType type)
        {
            var encounter = _catalog.Track(ScriptableObject.CreateInstance<EncounterData>());
            var monster = _catalog.Monster(id, MonsterRole.Carry, 10, 0, _catalog.Griffe);
            encounter.Init(id, id, type, new[] { monster, monster, monster, monster, monster });
            return encounter;
        }

        private ZoneData Zone(string zoneName, bool requiresBosses)
        {
            var zone = _catalog.Track(ScriptableObject.CreateInstance<ZoneData>());
            zone.InitIdentity(zoneName, "", new Vector2(0.5f, 0.5f), requiresBosses);
            return zone;
        }

        [Test]
        public void CombatsDuScenario_DisponiblesDansLOrdre()
        {
            Assert.That(_world.GetEncounterState(_plains, _patrol), Is.EqualTo(EncounterState.Available));
            Assert.That(_world.GetEncounterState(_plains, _thrak), Is.EqualTo(EncounterState.Locked));
            Assert.That(_world.GetEncounterState(_plains, _vaelisse), Is.EqualTo(EncounterState.Locked));

            _world.RecordVictory("patrouille");

            Assert.That(_world.GetEncounterState(_plains, _patrol), Is.EqualTo(EncounterState.Completed), "unique : ne se rejoue pas");
            Assert.That(_world.GetEncounterState(_plains, _thrak), Is.EqualTo(EncounterState.Available));
            Assert.That(_world.GetEncounterState(_plains, _vaelisse), Is.EqualTo(EncounterState.Locked), "Vaelisse attend la défaite de Thrâk");
        }

        [Test]
        public void CombatLibre_ToujoursRejouable()
        {
            _world.RecordVictory("entrainement");

            Assert.That(_world.GetEncounterState(_plains, _training), Is.EqualTo(EncounterState.Available));
        }

        [Test]
        public void Ruines_VerrouilleesTantQueLesBossDesAutresZonesNeSontPasVaincus()
        {
            Assert.That(_world.IsZoneUnlocked(_plains), Is.True);
            Assert.That(_world.IsZoneUnlocked(_ruins), Is.False);

            _world.RecordVictory("patrouille");
            _world.RecordVictory("thrak");
            Assert.That(_world.IsZoneUnlocked(_ruins), Is.False);

            _world.RecordVictory("vaelisse");
            Assert.That(_world.IsBossDefeated(_plains), Is.True);
            Assert.That(_world.IsZoneUnlocked(_ruins), Is.True);
        }

        [Test]
        public void ZoneSansBossDefini_GardeLesRuinesVerrouillees()
        {
            _plains.SetEncounters(new[] { _patrol }, new EncounterData[0], null);
            _world.RecordVictory("patrouille");

            Assert.That(_world.IsBossDefeated(_plains), Is.False);
            Assert.That(_world.IsZoneUnlocked(_ruins), Is.False);
        }

        [Test]
        public void ZoneVerrouillee_SesCombatsLeSontAussi()
        {
            var sealedEntity = Encounter("entite", EncounterType.Scenario);
            _ruins.SetEncounters(new[] { sealedEntity }, new EncounterData[0], sealedEntity);

            Assert.That(_world.GetEncounterState(_ruins, sealedEntity), Is.EqualTo(EncounterState.Locked));
        }
    }
}
