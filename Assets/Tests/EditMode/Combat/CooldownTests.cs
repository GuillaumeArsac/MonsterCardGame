using NUnit.Framework;
using MonsterCardGame.Gameplay.Combat;
using MonsterCardGame.Gameplay.Skills;
using MonsterCardGame.Gameplay.Skills.Effects;

namespace MonsterCardGame.Tests.EditMode.Combat
{
    /// <summary>Temps de recharge, entrée sur le terrain et ralentissement. Référence : docs/competences.md.</summary>
    public class CooldownTests : CombatTestBase
    {
        /// <summary>Comme Coup de bouclier, mais à recharge 0 : utilisable dès le premier tour.</summary>
        private SkillData _coupRapide;

        [SetUp] public void SetUp() => _coupRapide = Active("Coup rapide", 0, new DamageEffect(1), new SlowEffect());
        [Test]
        public void ExempleDeLaDoc_CompetenceA0Immediate_CompetenceA1AuTour2()
        {
            var recharge1 = Active("Recharge 1", 1, new DamageEffect(1));
            var monstre = Mon("Monstre", 20, Catalog.Griffe, recharge1);
            var enemy = Mon("Adversaire", 40);
            StartDuel(monstre, enemy);

            Assert.That(CanUse(monstre, Catalog.Griffe), Is.True, "recharge 0 : dès le tour 1");
            Assert.That(CanUse(monstre, recharge1), Is.False, "recharge 1 : pas pendant la phase de pose");
            Assert.That(monstre.GetCooldown(IndexOf(monstre, recharge1)), Is.EqualTo(1));

            Engine.EndPhase();
            Assert.That(monstre.GetCooldown(IndexOf(monstre, recharge1)), Is.EqualTo(0), "baisse à la fin de la phase du joueur");

            AdvanceTo(2, Side.Player);
            Assert.That(CanUse(monstre, recharge1), Is.True);
        }

        [Test]
        public void Recharge0_UneSeuleFoisParPhase()
        {
            var loup = Mon("Loup", 15, Catalog.Griffe);
            var enemy = Mon("Adversaire", 40);
            StartDuel(loup, enemy);

            Use(loup, Catalog.Griffe, enemy);

            Assert.That(CanUse(loup, Catalog.Griffe), Is.False);
            AdvanceTo(2, Side.Player);
            Assert.That(CanUse(loup, Catalog.Griffe), Is.True);
        }

        [Test]
        public void TroisGriffe_TroisUtilisationsParPhase()
        {
            var monstre = Mon("Trois griffes", 20, Catalog.Griffe, Catalog.Griffe, Catalog.Griffe);
            var enemy = Mon("Adversaire", 40);
            StartDuel(monstre, enemy);

            for (int i = 0; i < 3; i++)
                Engine.UseSkill(monstre, i, enemy);

            Assert.That(enemy.Hp, Is.EqualTo(40 - 9));
        }

        [Test]
        public void Recharge2_UnePhaseSurDeux()
        {
            var chouette = Mon("Chouette", 16, Catalog.Soin);
            StartDuel(chouette, Mon("Adversaire", 40));

            Assert.That(CanUse(chouette, Catalog.Soin), Is.False, "tour 1 : compteur à 2");
            AdvanceTo(2, Side.Player);
            Assert.That(CanUse(chouette, Catalog.Soin), Is.False, "tour 2 : compteur à 1");
            AdvanceTo(3, Side.Player);
            Assert.That(CanUse(chouette, Catalog.Soin), Is.True, "tour 3 : disponible");

            Use(chouette, Catalog.Soin, chouette);
            AdvanceTo(4, Side.Player);
            Assert.That(CanUse(chouette, Catalog.Soin), Is.False);
            AdvanceTo(5, Side.Player);
            Assert.That(CanUse(chouette, Catalog.Soin), Is.True);
        }

        [Test]
        public void Compteurs_NeBaissentQuALaFinDeLaPhaseDeLeurCamp()
        {
            var joueur = Mon("Joueur", 20);
            var chaman = Mon("Chaman", 16, Catalog.Soin);
            StartDuel(joueur, chaman);
            int soin = IndexOf(chaman, Catalog.Soin);

            Engine.EndPhase();
            Assert.That(chaman.GetCooldown(soin), Is.EqualTo(2), "fin de la phase du joueur : pas de changement");

            Engine.EndPhase();
            Assert.That(chaman.GetCooldown(soin), Is.EqualTo(1), "fin de la phase de l'adversaire");
        }

        [Test]
        public void Ralentissement_BloqueUneCompetenceDisponible_PendantLaProchainePhaseDuMonstre()
        {
            var sanglier = Mon("Sanglier", 24, _coupRapide);
            var loup = Mon("Loup adverse", 15, Catalog.Griffe);
            Start(new[] { sanglier }, new[] { loup }, new[] { (sanglier, 0) }, new[] { (loup, 0) });
            AdvanceTo(2, Side.Player);

            Use(sanglier, _coupRapide, loup);
            Assert.That(loup.GetCooldown(0), Is.EqualTo(1));

            Engine.EndPhase();
            Assert.That(CanUse(loup, Catalog.Griffe), Is.False, "ralentie : bloquée pendant la phase de l'adversaire");

            AdvanceTo(3, Side.Enemy);
            Assert.That(CanUse(loup, Catalog.Griffe), Is.True, "puis son temps de recharge normal s'applique");
        }

        [Test]
        public void Ralentissement_ParLAdversaire_BloqueLaProchainePhaseDuJoueur()
        {
            var loup = Mon("Loup", 15, Catalog.Griffe);
            var sanglier = Mon("Sanglier adverse", 24, _coupRapide);
            StartDuel(loup, sanglier);
            AdvanceTo(2, Side.Enemy);

            Use(sanglier, _coupRapide, loup);
            Engine.EndPhase();

            Assert.That(CanUse(loup, Catalog.Griffe), Is.False);
        }

        [Test]
        public void Ralentissement_SAppliqueMemeSiLesDegatsSontReduitsA0()
        {
            var sanglier = Mon("Sanglier", 24, _coupRapide);
            var taureau = Mon("Taureau", 30, Catalog.Carapace, Catalog.Griffe);
            Start(new[] { sanglier }, new[] { taureau }, new[] { (sanglier, 0) }, new[] { (taureau, 0) });
            AdvanceTo(2, Side.Player);

            Use(sanglier, _coupRapide, taureau);

            Assert.That(taureau.Hp, Is.EqualTo(30), "Carapace réduit 1 dégât à 0");
            Assert.That(taureau.GetCooldown(IndexOf(taureau, Catalog.Griffe)), Is.EqualTo(1));
        }

        [Test]
        public void Ralentissement_TouchePasLesPassives_EtNAPasDeDuree()
        {
            var sanglier = Mon("Sanglier", 24, _coupRapide);
            var loup = Mon("Loup adverse", 15, Catalog.Griffe, Catalog.Carapace);
            StartDuel(sanglier, loup);
            AdvanceTo(2, Side.Player);

            Use(sanglier, _coupRapide, loup);
            Assert.That(loup.GetCooldown(IndexOf(loup, Catalog.Carapace)), Is.EqualTo(0));

            AdvanceTo(3, Side.Enemy);
            Use(loup, Catalog.Griffe, sanglier);
            AdvanceTo(4, Side.Enemy);
            Assert.That(CanUse(loup, Catalog.Griffe), Is.True, "recharge 0 à nouveau après l'utilisation");
        }
    }
}
