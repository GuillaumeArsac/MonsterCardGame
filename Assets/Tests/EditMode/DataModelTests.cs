using System.Linq;
using NUnit.Framework;
using MonsterCardGame.Core;
using MonsterCardGame.Gameplay.Encounters;
using MonsterCardGame.Gameplay.Skills;
using MonsterCardGame.Gameplay.Skills.Effects;

namespace MonsterCardGame.Tests.EditMode
{
    /// <summary>
    /// Le modèle de données sait décrire tout le catalogue des docs.
    /// Références : docs/competences.md, docs/combats-scenario.md.
    /// </summary>
    public class DataModelTests
    {
        private TestCatalog _catalog;

        [SetUp]    public void SetUp()    => _catalog = new TestCatalog();
        [TearDown] public void TearDown() => _catalog.Dispose();

        [Test]
        public void Catalogue_Contient13Competences_ToutesAvecDesEffets()
        {
            var skills = _catalog.AllSkills.ToList();

            Assert.That(skills.Count, Is.EqualTo(13));
            Assert.That(skills.Select(s => s.SkillName).Distinct().Count(), Is.EqualTo(13));
            Assert.That(skills, Has.All.Matches<SkillData>(s => s.Effects.Count > 0));
        }

        [Test]
        public void MorsurePerforante_IgnoreLesReductions()
        {
            var damage = _catalog.MorsurePerforante.GetEffect<DamageEffect>();

            Assert.That(damage.Amount, Is.EqualTo(2));
            Assert.That(damage.IgnoresReductions, Is.True);
            Assert.That(_catalog.Griffe.GetEffect<DamageEffect>().IgnoresReductions, Is.False);
        }

        [Test]
        public void GrandBalayage_InfligeDesDegatsPuisRalentit()
        {
            var skill = _catalog.GrandBalayage;

            Assert.That(skill.Target, Is.EqualTo(SkillTarget.AllEnemies));
            Assert.That(skill.Effects[0], Is.TypeOf<DamageEffect>());
            Assert.That(skill.Effects[1], Is.TypeOf<SlowEffect>());
        }

        [Test]
        public void Carapace_EstUneReductionPermanente_Bouclier_DureDeuxTours()
        {
            var carapace = _catalog.Carapace.GetEffect<DamageReductionEffect>();
            var bouclier = _catalog.Bouclier.GetEffect<DamageReductionEffect>();

            Assert.That(carapace.IsPermanent, Is.True);
            Assert.That(bouclier.IsPermanent, Is.False);
            Assert.That(bouclier.DurationTurns, Is.EqualTo(2));
        }

        [Test]
        public void Passives_OntLeurDeclencheur()
        {
            Assert.That(_catalog.Provocation.IsPassive, Is.True);
            Assert.That(_catalog.Provocation.Trigger, Is.EqualTo(PassiveTrigger.Always));
            Assert.That(_catalog.AuraApaisante.Trigger, Is.EqualTo(PassiveTrigger.StartOfOwnPhase));
            Assert.That(_catalog.ChasseurDeTrophees.Trigger, Is.EqualTo(PassiveTrigger.StartOfCombat));
        }

        [Test]
        public void Effets_SurviventALaSerialisationUnity()
        {
            var copy = UnityEngine.Object.Instantiate(_catalog.MorsurePerforante);
            try
            {
                var damage = copy.GetEffect<DamageEffect>();
                Assert.That(damage, Is.Not.Null);
                Assert.That(damage, Is.Not.SameAs(_catalog.MorsurePerforante.GetEffect<DamageEffect>()));
                Assert.That(damage.Amount, Is.EqualTo(2));
                Assert.That(damage.IgnoresReductions, Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(copy);
            }
        }

        [Test]
        public void PatrouilleCentaure_EstUnCombatDuScenarioDe5Monstres()
        {
            var encounter = _catalog.PatrouilleCentaure();

            Assert.That(encounter.Type, Is.EqualTo(EncounterType.Scenario));
            Assert.That(encounter.IsRepeatable, Is.False);
            Assert.That(encounter.Monsters.Count, Is.EqualTo(GameRules.TeamSize));
            Assert.That(encounter.Monsters.Select(m => m.StatBoost.Hp), Has.All.EqualTo(2));
            Assert.That(encounter.Monsters.Max(m => m.Skills.Count), Is.LessThanOrEqualTo(GameRules.MaxSkillsPerMonster));
        }
    }
}
