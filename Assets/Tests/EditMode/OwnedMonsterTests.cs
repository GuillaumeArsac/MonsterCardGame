using System;
using NUnit.Framework;
using MonsterCardGame.Gameplay.Monsters;
using MonsterCardGame.Gameplay.Skills;

namespace MonsterCardGame.Tests.EditMode
{
    /// <summary>Règles du dépeçage appliquées à un monstre du joueur. Référence : docs/progression.md.</summary>
    public class OwnedMonsterTests
    {
        private TestCatalog _catalog;

        [SetUp]    public void SetUp()    => _catalog = new TestCatalog();
        [TearDown] public void TearDown() => _catalog.Dispose();

        private OwnedMonster OursBrun()
            => new(_catalog.Monster("Ours brun", MonsterRole.Carry, 200, 0, _catalog.Griffe));

        [Test]
        public void NouveauMonstre_ReprendLesCompetencesDeSaCarte_SansBoost()
        {
            var ours = OursBrun();

            Assert.That(ours.Skills, Is.EqualTo(new[] { _catalog.Griffe }));
            Assert.That(ours.Boost.IsEmpty, Is.True);
            Assert.That(ours.MaxHp, Is.EqualTo(200));
            Assert.That(ours.LockedSkillIndex, Is.EqualTo(OwnedMonster.NoLockedSkill));
        }

        [Test]
        public void AjouterCompetence_ModifiePasLaCarteDOrigine()
        {
            var ours = OursBrun();

            ours.AddSkill(_catalog.Carapace);

            Assert.That(ours.Skills.Count, Is.EqualTo(2));
            Assert.That(ours.Origin.Skills.Count, Is.EqualTo(1));
        }

        [Test]
        public void AjouterCompetenceDejaPossedee_LesDeuxExemplairesSontGardes()
        {
            var ours = OursBrun();

            ours.AddSkill(_catalog.Griffe);

            Assert.That(ours.Skills, Is.EqualTo(new[] { _catalog.Griffe, _catalog.Griffe }));
        }

        [Test]
        public void AjouterQuatriemeCompetence_EstRefuse()
        {
            var ours = OursBrun();
            ours.AddSkill(_catalog.Griffe);
            ours.AddSkill(_catalog.Carapace);

            Assert.That(ours.HasFreeSkillSlot, Is.False);
            Assert.Throws<InvalidOperationException>(() => ours.AddSkill(_catalog.Soin));
        }

        [Test]
        public void RemplacerCompetence_LaCompetenceRemplaceeEstPerdue()
        {
            var ours = OursBrun();
            ours.AddSkill(_catalog.Griffe);
            ours.AddSkill(_catalog.Carapace);

            ours.ReplaceSkill(1, _catalog.Soin);

            Assert.That(ours.Skills, Is.EqualTo(new[] { _catalog.Griffe, _catalog.Soin, _catalog.Carapace }));
        }

        [Test]
        public void CompetenceUnique_NePeutPasEtreRemplacee()
        {
            var card = _catalog.QuestMonster("Monstre de quête", MonsterRole.Support, 180, _catalog.Repli, _catalog.Griffe);
            var monstre = new OwnedMonster(card);

            Assert.That(monstre.LockedSkillIndex, Is.EqualTo(0));
            Assert.Throws<InvalidOperationException>(() => monstre.ReplaceSkill(0, _catalog.Soin));

            monstre.ReplaceSkill(1, _catalog.Soin);
            Assert.That(monstre.Skills, Is.EqualTo(new[] { _catalog.Repli, _catalog.Soin }));
        }

        [Test]
        public void Boost_AugmenteLesPvDeDepart()
        {
            var ours = OursBrun();

            ours.SetBoost(new StatBoost(40));

            Assert.That(ours.MaxHp, Is.EqualTo(240));
        }

        [Test]
        public void NouveauBoost_RemplaceLePrecedent()
        {
            var ours = OursBrun();
            ours.SetBoost(new StatBoost(80));

            ours.SetBoost(new StatBoost(20));

            Assert.That(ours.Boost.Hp, Is.EqualTo(20));
            Assert.That(ours.MaxHp, Is.EqualTo(220));
        }

        [Test]
        public void MonstreRecharge_AvecTropDeCompetences_EstRefuse()
        {
            var card = _catalog.Monster("Ours brun", MonsterRole.Carry, 200, 0, _catalog.Griffe);
            var skills = new[] { _catalog.Griffe, _catalog.Griffe, _catalog.Griffe, _catalog.Griffe };

            Assert.Throws<ArgumentException>(() => new OwnedMonster(card, skills, StatBoost.None, OwnedMonster.NoLockedSkill));
        }
    }
}
