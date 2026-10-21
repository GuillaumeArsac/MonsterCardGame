using NUnit.Framework;
using MonsterCardGame.Gameplay.Skills;

namespace MonsterCardGame.Tests.EditMode
{
    /// <summary>Texte des compétences affiché au joueur. Référence : docs/competences.md.</summary>
    public class SkillDescriberTests
    {
        private TestCatalog _catalog;

        [SetUp]    public void SetUp()    => _catalog = new TestCatalog();
        [TearDown] public void TearDown() => _catalog.Dispose();

        [Test]
        public void ToutesLesCompetences_OntUneDescription()
        {
            foreach (var skill in _catalog.AllSkills)
                Assert.That(SkillDescriber.Effect(skill), Is.Not.EqualTo("Aucun effet."), skill.SkillName);
        }

        [Test]
        public void GrandBalayage_DegatsEtRalentissement_IgnoreLaProvocation()
        {
            var skill = _catalog.GrandBalayage;

            Assert.That(SkillDescriber.Summary(skill), Does.StartWith("Active · recharge 3"));
            Assert.That(SkillDescriber.Effect(skill), Does.StartWith("Inflige 1 dégât et applique Ralentissement"));
            Assert.That(SkillDescriber.TargetingNote(skill), Does.Contain("ignore la Provocation"));
        }

        [Test]
        public void MorsurePerforante_IgnoreLesReductions()
        {
            Assert.That(SkillDescriber.Effect(_catalog.MorsurePerforante), Does.Contain("2 dégâts, qui ignorent les réductions"));
        }

        [Test]
        public void Benediction_DureeEnTours_Carapace_Permanente()
        {
            Assert.That(SkillDescriber.Effect(_catalog.Benediction), Does.Contain("augmentent de 1 pendant 6 tours"));
            Assert.That(SkillDescriber.Effect(_catalog.Carapace), Does.Contain("en permanence"));
        }

        [Test]
        public void Passive_IndiqueSonDeclencheur()
        {
            Assert.That(SkillDescriber.Summary(_catalog.AuraApaisante), Is.EqualTo("Passive · au début de chaque phase de ce monstre"));
            Assert.That(SkillDescriber.Summary(_catalog.ChasseurDeTrophees), Is.EqualTo("Passive · au début du combat"));
            Assert.That(SkillDescriber.TargetingNote(_catalog.Provocation), Is.Empty);
        }

        [Test]
        public void TexteComplet_ContientLaCible()
        {
            var text = SkillDescriber.FullText(_catalog.Griffe);

            Assert.That(text, Does.Contain("Cible : un monstre sur le terrain, allié ou ennemi"));
            Assert.That(text, Does.Contain("Inflige 3 dégâts."));
            Assert.That(text, Does.Contain("soumise à la Provocation"));
        }
    }
}
