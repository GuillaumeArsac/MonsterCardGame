using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using MonsterCardGame.Gameplay.Content;
using MonsterCardGame.Gameplay.Encounters;
using MonsterCardGame.Gameplay.Monsters;
using MonsterCardGame.Gameplay.Skills;
using MonsterCardGame.Gameplay.World;

namespace MonsterCardGame.Tests.EditMode
{
    /// <summary>Collection, équipe, dépeçage et sauvegarde. Référence : docs/progression.md.</summary>
    public class ProgressionTests
    {
        private TestCatalog _catalog;
        private MonsterCardData[] _starters;

        [SetUp]
        public void SetUp()
        {
            _catalog  = new TestCatalog();
            _starters = _catalog.TeamStart().Concat(new[]
            {
                _catalog.Monster("Luciole sacrée", MonsterRole.Support, 160, 0, _catalog.Benediction),
                _catalog.Monster("Sanglier",       MonsterRole.Tank,    240, 0, _catalog.CoupDeBouclier),
            }).ToArray();
            foreach (var card in _starters) card.name = card.MonsterName;
            foreach (var skill in _catalog.AllSkills) skill.name = skill.SkillName;
        }

        [TearDown] public void TearDown() => _catalog.Dispose();

        private MonsterCollection NewGame()
        {
            var collection = new MonsterCollection();
            collection.StartNewGame(_starters);
            return collection;
        }

        // ── Collection et équipe ──────────────────────────────────────────

        [Test]
        public void NouvellePartie_SeptMonstres_EtUneEquipeDeCinq()
        {
            var collection = NewGame();

            Assert.That(collection.Monsters.Count, Is.EqualTo(7));
            Assert.That(collection.Team.Count, Is.EqualTo(5));
            Assert.That(collection.IsTeamComplete, Is.True);
        }

        [Test]
        public void Equipe_PasPlusDeCinqMonstres_SansDoublon()
        {
            var collection = NewGame();
            var reserve = collection.Monsters.First(m => !collection.IsInTeam(m));

            Assert.That(collection.AddToTeam(reserve), Is.False, "équipe complète");

            collection.RemoveFromTeam(collection.Team[0]);
            Assert.That(collection.IsTeamComplete, Is.False);
            Assert.That(collection.AddToTeam(reserve), Is.True);
            Assert.That(collection.AddToTeam(reserve), Is.False, "déjà dans l'équipe");
        }

        // ── Dépeçage ──────────────────────────────────────────────────────

        [Test]
        public void Depecage_TireUnMonstreDeLEquipeAdverse()
        {
            var encounter = _catalog.PatrouilleCentaure();
            var drawn = Enumerable.Range(0, 200).Select(seed => SkinningOffer.Draw(encounter, new System.Random(seed)).Monster).Distinct().ToList();

            Assert.That(drawn, Is.SubsetOf(encounter.Monsters));
            Assert.That(drawn.Count, Is.EqualTo(5), "tous les monstres peuvent être tirés, vaincus ou non");
        }

        [Test]
        public void Depecage_SemiBoss_ThrakToujoursDepece()
        {
            var encounter = _catalog.ThrakLePerpetuel();
            var drawn = Enumerable.Range(0, 50).Select(seed => SkinningOffer.Draw(encounter, new System.Random(seed)).Monster).Distinct().ToList();

            Assert.That(drawn, Is.EqualTo(new[] { encounter.Monsters[0] }));
            Assert.That(drawn[0].MonsterName, Is.EqualTo("Thrâk le Perpétuel"));
        }

        [Test]
        public void Depecage_CompetenceSpeciale_UneSeuleParMonstre()
        {
            var special = _catalog.EspritPerpetuel;
            var autre   = SkillData.CreatePassive("Autre spéciale", MonsterRole.Tank, SkillTarget.Self, PassiveTrigger.Always).SetSpecial();
            var porteur = new OwnedMonster(_catalog.Monster("Porteur", MonsterRole.Tank, 260, 0, _catalog.Griffe, special));
            var vierge  = new OwnedMonster(_catalog.Monster("Vierge", MonsterRole.Tank, 260, 0, _catalog.Griffe));

            Assert.That(SkinningOffer.NeedsReplacement(vierge, special), Is.False, "sans spéciale : ajout libre");
            Assert.That(SkinningOffer.NeedsReplacement(porteur, autre), Is.True, "un emplacement libre ne suffit pas");
            Assert.That(SkinningOffer.CanReplace(porteur, autre, 0), Is.False, "elle ne remplace pas une compétence ordinaire");
            Assert.That(SkinningOffer.CanReplace(porteur, autre, 1), Is.True, "elle remplace la spéciale");
            Assert.That(SkinningOffer.NeedsReplacement(porteur, _catalog.Griffe), Is.False, "une compétence ordinaire s'ajoute toujours");
            Assert.Throws<InvalidOperationException>(() => porteur.AddSkill(autre));

            new SkinningOffer(_catalog.Monster("Boss", MonsterRole.Tank, 300, 0, autre)).GiveSkill(autre, porteur, replacedIndex: 1);

            Assert.That(porteur.Skills, Is.EqualTo(new[] { _catalog.Griffe, autre }));
            UnityEngine.Object.DestroyImmediate(autre);
        }

        [Test]
        public void Depecage_CompetenceSpeciale_UniqueDeQuete_NePeutPasEtreRemplacee()
        {
            var unique  = SkillData.CreatePassive("Spéciale de quête", MonsterRole.Tank, SkillTarget.Self, PassiveTrigger.Always).SetSpecial();
            var quete   = new OwnedMonster(_catalog.QuestMonster("Héros", MonsterRole.Tank, 260, unique));

            Assert.That(SkinningOffer.CanReceive(quete, _catalog.EspritPerpetuel), Is.False);
            Assert.That(SkinningOffer.CanReceive(quete, _catalog.Griffe), Is.True);
            UnityEngine.Object.DestroyImmediate(unique);
        }

        [Test]
        public void Depecage_CompetenceAjouteeAUnMonstre()
        {
            var collection = NewGame();
            var ours = collection.Monsters.First(m => m.Name == "Ours brun");
            var offer = new SkinningOffer(_catalog.Monster("Cavalier", MonsterRole.Carry, 160, 20, _catalog.Griffe, _catalog.CoupDeBouclier));

            offer.GiveSkill(_catalog.Griffe, ours);

            Assert.That(ours.Skills, Is.EqualTo(new[] { _catalog.Griffe, _catalog.Griffe }), "un doublon est permis");
            Assert.That(offer.IsClaimed, Is.True);
            Assert.Throws<InvalidOperationException>(() => offer.GiveBoost(ours), "une seule récompense");
        }

        [Test]
        public void Depecage_MonstreATroisCompetences_RemplaceCelleChoisie()
        {
            var ours = new OwnedMonster(_catalog.Monster("Ours", MonsterRole.Carry, 200, 0, _catalog.Griffe, _catalog.Soin, _catalog.Repli));
            var offer = new SkinningOffer(_catalog.Monster("Jeune taureau", MonsterRole.Tank, 200, 20, _catalog.Provocation, _catalog.Carapace));

            Assert.That(SkinningOffer.NeedsReplacement(ours), Is.True);
            offer.GiveSkill(_catalog.Carapace, ours, replacedIndex: 2);

            Assert.That(ours.Skills, Is.EqualTo(new[] { _catalog.Griffe, _catalog.Soin, _catalog.Carapace }));
        }

        [Test]
        public void Depecage_Boost_RemplaceLeBoostPrecedent()
        {
            var ours = new OwnedMonster(_catalog.Monster("Ours", MonsterRole.Carry, 200, 0, _catalog.Griffe));
            ours.SetBoost(new StatBoost(80));
            var offer = new SkinningOffer(_catalog.Monster("Éclaireur", MonsterRole.Carry, 140, 20, _catalog.Griffe));

            offer.GiveBoost(ours);

            Assert.That(ours.MaxHp, Is.EqualTo(220));
        }

        [Test]
        public void Depecage_CompetencesProposees_SansDoublon_EtPasDeBoostVide()
        {
            var offer = new SkinningOffer(_catalog.Monster("Hydre", MonsterRole.Carry, 200, 0, _catalog.Griffe, _catalog.Griffe));

            Assert.That(offer.Skills, Is.EqualTo(new[] { _catalog.Griffe }));
            Assert.That(offer.HasBoost, Is.False);
        }

        [Test]
        public void SessionDeCombat_LaVictoireTireLeMonstreADepecerUneSeuleFois()
        {
            var session = new CombatSession();
            session.Begin(_catalog.PatrouilleCentaure(), "Team");

            session.Complete(true, new System.Random(1));
            var first = session.Offer;
            session.Complete(true, new System.Random(2));

            Assert.That(session.Victory, Is.True);
            Assert.That(session.Offer, Is.SameAs(first));
        }

        [Test]
        public void SessionDeCombat_DefaiteSansDepecage()
        {
            var session = new CombatSession();
            session.Begin(_catalog.PatrouilleCentaure(), "Team");

            session.Complete(false, new System.Random(1));

            Assert.That(session.Offer, Is.Null);
        }

        // ── Sauvegarde ────────────────────────────────────────────────────

        private ContentDatabase Database()
        {
            var database = _catalog.Track(ScriptableObject.CreateInstance<ContentDatabase>());
            database.Init(_catalog.AllSkills, _starters, _starters, Array.Empty<EncounterData>());
            return database;
        }

        [Test]
        public void Sauvegarde_GardeLesCompetencesLeBoostEtLEquipe()
        {
            var collection = NewGame();
            var ours = collection.Monsters.First(m => m.Name == "Ours brun");
            ours.AddSkill(_catalog.Carapace);
            ours.SetBoost(new StatBoost(40));
            var sanglier = collection.Monsters.First(m => m.Name == "Sanglier");
            collection.RemoveFromTeam(collection.Team[0]);
            collection.AddToTeam(sanglier);

            var data = new SaveData();
            CollectionSaveMapper.Write(collection, data);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            var reloaded = new MonsterCollection();
            var ignored = CollectionSaveMapper.Read(Newtonsoft.Json.JsonConvert.DeserializeObject<SaveData>(json), Database(), reloaded);

            Assert.That(ignored, Is.Empty);
            Assert.That(reloaded.Monsters.Select(m => m.Name), Is.EqualTo(collection.Monsters.Select(m => m.Name)));
            var oursReloaded = reloaded.Monsters.First(m => m.Name == "Ours brun");
            Assert.That(oursReloaded.Skills, Is.EqualTo(new[] { _catalog.Griffe, _catalog.Carapace }));
            Assert.That(oursReloaded.MaxHp, Is.EqualTo(240));
            Assert.That(reloaded.Team.Select(m => m.Name), Is.EqualTo(collection.Team.Select(m => m.Name)));
        }

        [Test]
        public void Sauvegarde_GardeLaCompetenceUniqueVerrouillee()
        {
            var quest = _catalog.QuestMonster("Monstre de quête", MonsterRole.Support, 180, _catalog.Repli, _catalog.Griffe);
            quest.name = "Monstre de quête";
            _starters = _starters.Append(quest).ToArray();
            var collection = NewGame();

            var data = new SaveData();
            CollectionSaveMapper.Write(collection, data);
            var reloaded = new MonsterCollection();
            CollectionSaveMapper.Read(data, Database(), reloaded);

            var monster = reloaded.Monsters.First(m => m.Name == "Monstre de quête");
            Assert.That(monster.IsLocked(0), Is.True);
        }

        [Test]
        public void Sauvegarde_ElementInconnu_EstIgnoreSansPerdreLeReste()
        {
            var data = new SaveData
            {
                Collection =
                {
                    new OwnedMonsterSave { Card = "Ours brun", Skills = { "Griffe", "Compétence disparue" } },
                    new OwnedMonsterSave { Card = "Monstre disparu", Skills = { "Griffe" } },
                    new OwnedMonsterSave { Card = "Loup des cendres", Skills = { "Griffe" } },
                },
                Team = { 0, 1, 2 }
            };
            var collection = new MonsterCollection();

            var ignored = CollectionSaveMapper.Read(data, Database(), collection);

            Assert.That(ignored.Count, Is.EqualTo(2));
            Assert.That(collection.Monsters.Select(m => m.Name), Is.EqualTo(new[] { "Ours brun", "Loup des cendres" }));
            Assert.That(collection.Team.Select(m => m.Name), Is.EqualTo(new[] { "Ours brun", "Loup des cendres" }));
        }
    }
}
