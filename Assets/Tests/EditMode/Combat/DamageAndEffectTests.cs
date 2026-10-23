using NUnit.Framework;
using MonsterCardGame.Gameplay.Combat;
using MonsterCardGame.Gameplay.Monsters;
using MonsterCardGame.Gameplay.Skills;
using MonsterCardGame.Gameplay.Skills.Effects;

namespace MonsterCardGame.Tests.EditMode.Combat
{
    /// <summary>Calcul des dégâts, soin, durées, Provocation et compétences du catalogue. Référence : docs/competences.md.</summary>
    public class DamageAndEffectTests : CombatTestBase
    {
        // ── Dégâts ────────────────────────────────────────────────────────

        [Test]
        public void Carapace_ReduitGriffeA20_MorsurePerforanteLIgnore()
        {
            var loup = Mon("Loup", 150, Catalog.Griffe, Catalog.MorsurePerforante);
            var taureau = Mon("Taureau", 300, Catalog.Carapace);
            StartDuel(loup, taureau);

            Use(loup, Catalog.Griffe, taureau);
            Assert.That(taureau.Hp, Is.EqualTo(280));

            Use(loup, Catalog.MorsurePerforante, taureau);
            Assert.That(taureau.Hp, Is.EqualTo(260));
        }

        [Test]
        public void Degats_BonusPuisReductions()
        {
            var bonus2 = Active("Bonus +2", 0, new DamageBonusEffect(20, 3));
            var loup = Mon("Loup", 150, bonus2, Catalog.Griffe);
            var taureau = Mon("Taureau", 300, Catalog.Carapace);
            StartDuel(loup, taureau);

            Use(loup, bonus2, loup);
            Use(loup, Catalog.Griffe, taureau);

            Assert.That(taureau.Hp, Is.EqualTo(300 - (30 + 20 - 10)));
        }

        [Test]
        public void MorsurePerforante_ProfiteDesBonus()
        {
            var renard = Mon("Renard", 140, Catalog.Benediction, Catalog.MorsurePerforante);
            var taureau = Mon("Taureau", 300, Catalog.Carapace);
            StartDuel(renard, taureau);
            AdvanceTo(4, Side.Player);

            Use(renard, Catalog.Benediction);
            Use(renard, Catalog.MorsurePerforante, taureau);

            Assert.That(taureau.Hp, Is.EqualTo(270));
        }

        [Test]
        public void Reduction_NeFaitPasDescendreLesDegatsSous0()
        {
            var chiquenaude = Active("Chiquenaude", 0, new DamageEffect(10));
            var protection = Active("Protection", 0, new DamageReductionEffect(30, 2));
            var loup = Mon("Loup", 150, chiquenaude);
            var enemy = Mon("Adversaire", 200, protection);
            StartDuel(loup, enemy);
            Engine.EndPhase();
            Use(enemy, protection, enemy);
            Engine.EndPhase();

            Use(loup, chiquenaude, enemy);

            Assert.That(enemy.Hp, Is.EqualTo(200));
        }

        [Test]
        public void EffetsIdentiques_SeCumulent()
        {
            var monstre = Mon("Deux carapaces", 300, Catalog.Carapace, Catalog.Carapace);
            Assert.That(monstre.DamageReduction, Is.EqualTo(20));
        }

        // ── Soin ──────────────────────────────────────────────────────────

        [Test]
        public void Soin_NeDepassePasLesPvDeDepart_BoostCompris()
        {
            var card = Catalog.Monster("Chouette", MonsterRole.Support, 160, 0, Catalog.Soin, Catalog.Griffe);
            var owned = new OwnedMonster(card);
            owned.SetBoost(new StatBoost(40));
            var chouette = CombatMonster.From(owned);
            var enemy = Mon("Adversaire", 400, Catalog.Griffe);
            StartDuel(chouette, enemy);
            Assert.That(chouette.MaxHp, Is.EqualTo(200));

            Engine.EndPhase();
            Use(enemy, Catalog.Griffe, chouette);
            AdvanceTo(3, Side.Player);
            Use(chouette, Catalog.Soin, chouette);

            Assert.That(chouette.Hp, Is.EqualTo(200));
        }

        [Test]
        public void AuraApaisante_SoigneTousLesAlliesAuDebutDeSaPhase()
        {
            var porte = Mon("Porte-étendard", 140, Catalog.AuraApaisante);
            var allie = Mon("Allié", 200);
            var loup = Mon("Loup adverse", 150, Catalog.Griffe, Catalog.Griffe);
            Start(new[] { loup }, new[] { porte, allie }, new[] { (loup, 0) }, new[] { (porte, 0), (allie, 1) });

            Engine.UseSkill(loup, 0, porte);
            Engine.UseSkill(loup, 1, allie);
            Engine.EndPhase();

            Assert.That(porte.Hp, Is.EqualTo(120), "y compris ce monstre");
            Assert.That(allie.Hp, Is.EqualTo(180));

            Engine.EndPhase();
            Assert.That(porte.Hp, Is.EqualTo(120), "pas pendant la phase du joueur");
        }

        // ── Durées ────────────────────────────────────────────────────────

        [Test]
        public void ExempleDeLaDoc_EffetDe2ToursDuJoueur()
        {
            var svathurim = Mon("Svathurim", 200, Catalog.Bouclier);
            StartDuel(svathurim, Mon("Adversaire", 200));
            AdvanceTo(4, Side.Player);

            Use(svathurim, Catalog.Bouclier, svathurim);
            Assert.That(svathurim.DamageReduction, Is.EqualTo(20), "pendant la fin de sa phase");

            AdvanceTo(4, Side.Enemy);
            Assert.That(svathurim.DamageReduction, Is.EqualTo(20), "1re phase de l'autre camp");
            AdvanceTo(5, Side.Player);
            Assert.That(svathurim.DamageReduction, Is.EqualTo(20));
            AdvanceTo(5, Side.Enemy);
            Assert.That(svathurim.DamageReduction, Is.EqualTo(20), "2e phase de l'autre camp");

            AdvanceTo(6, Side.Player);
            Assert.That(svathurim.DamageReduction, Is.EqualTo(0), "terminé au début de la phase suivante du joueur");
        }

        [Test]
        public void ExempleDeLaDoc_EffetDe1TourDeLAdversaire()
        {
            var protection = Active("Protection courte", 0, new DamageReductionEffect(20, 1));
            var enemy = Mon("Adversaire", 200, protection);
            StartDuel(Mon("Joueur", 200), enemy);
            AdvanceTo(1, Side.Enemy);

            Use(enemy, protection, enemy);

            AdvanceTo(2, Side.Player);
            Assert.That(enemy.DamageReduction, Is.EqualTo(20), "toute la phase suivante du joueur");
            AdvanceTo(2, Side.Enemy);
            Assert.That(enemy.DamageReduction, Is.EqualTo(0), "terminé au début de la phase suivante de l'adversaire");
        }

        [Test]
        public void Benediction_SeulsLesAlliesSurLeTerrainRecoiventLeBonus_Pendant6Tours()
        {
            var luciole = Mon("Luciole", 160, Catalog.Benediction);
            var posé = Mon("Posé", 200);
            var enMain = Mon("En main", 200);
            var enemy = Mon("Adversaire", 400);
            Start(new[] { luciole, posé, enMain }, new[] { enemy }, new[] { (luciole, 0), (posé, 1) }, new[] { (enemy, 0) });
            AdvanceTo(4, Side.Player);

            Use(luciole, Catalog.Benediction);

            Assert.That(luciole.DamageBonus, Is.EqualTo(10));
            Assert.That(posé.DamageBonus, Is.EqualTo(10));
            Assert.That(enMain.DamageBonus, Is.EqualTo(0));

            AdvanceTo(9, Side.Enemy);
            Assert.That(posé.DamageBonus, Is.EqualTo(10), "6e phase de l'adversaire");
            AdvanceTo(10, Side.Player);
            Assert.That(posé.DamageBonus, Is.EqualTo(0));
        }

        // ── Provocation ───────────────────────────────────────────────────

        [Test]
        public void Provocation_LesCompetencesQuiVisentUnEnnemiDoiventLeCibler()
        {
            var loup = Mon("Loup", 150, Catalog.Griffe);
            var allie = Mon("Allié", 200);
            var chien = Mon("Chien de garde", 260, Catalog.Provocation);
            var carry = Mon("Carry adverse", 140);
            Start(new[] { loup, allie }, new[] { chien, carry }, new[] { (loup, 0), (allie, 1) }, new[] { (chien, 0), (carry, 1) });

            var targets = Engine.GetValidTargets(loup, 0);

            Assert.That(targets, Has.Member(chien));
            Assert.That(targets, Has.No.Member(carry));
            Assert.That(targets, Has.Member(allie), "viser un allié reste permis");
        }

        [Test]
        public void Provocation_PlusieursMonstres_AuChoix()
        {
            var loup = Mon("Loup", 150, Catalog.Griffe);
            var chien1 = Mon("Chien 1", 260, Catalog.Provocation);
            var chien2 = Mon("Chien 2", 260, Catalog.Provocation);
            var carry = Mon("Carry adverse", 140);
            Start(new[] { loup }, new[] { chien1, chien2, carry }, new[] { (loup, 0) }, new[] { (chien1, 0), (chien2, 1), (carry, 2) });

            var targets = Engine.GetValidTargets(loup, 0);

            Assert.That(targets, Is.EquivalentTo(new[] { loup, chien1, chien2 }));
        }

        [Test]
        public void GrandBalayage_IgnoreLaProvocation_EtRalentitTousLesEnnemis()
        {
            var thrak = Mon("Thrâk", 260, Catalog.GrandBalayage);
            var chien = Mon("Chien de garde", 260, Catalog.Provocation);
            var loup = Mon("Loup", 150, Catalog.Griffe);
            Start(new[] { chien, loup }, new[] { thrak }, new[] { (chien, 0), (loup, 1) }, new[] { (thrak, 0) });
            AdvanceTo(4, Side.Enemy);

            Use(thrak, Catalog.GrandBalayage);

            Assert.That(chien.Hp, Is.EqualTo(250));
            Assert.That(loup.Hp, Is.EqualTo(140));
            Assert.That(loup.GetCooldown(0), Is.EqualTo(1));
        }

        // ── Exécution et Repli ────────────────────────────────────────────

        [Test]
        public void Execution_CibleA80PvOuMoins_EstVaincue()
        {
            var griffon = Mon("Griffon", 160, Catalog.Execution);
            var enemy = Mon("Adversaire", 80);
            var reserve = Mon("Réserve", 200);
            Start(new[] { griffon }, new[] { enemy, reserve }, new[] { (griffon, 0) }, new[] { (enemy, 0) });
            AdvanceTo(5, Side.Player);

            Use(griffon, Catalog.Execution, enemy);

            Assert.That(enemy.Zone, Is.EqualTo(MonsterZone.Defeated));
            Assert.That(Engine.GetPoints(Side.Player), Is.EqualTo(1));
        }

        [Test]
        public void Execution_CibleAuDessusDuSeuil_Subit10Degats()
        {
            var griffon = Mon("Griffon", 160, Catalog.Execution);
            var enemy = Mon("Adversaire", 90);
            StartDuel(griffon, enemy);
            AdvanceTo(5, Side.Player);

            Use(griffon, Catalog.Execution, enemy);

            Assert.That(enemy.Zone, Is.EqualTo(MonsterZone.Field));
            Assert.That(enemy.Hp, Is.EqualTo(80));
        }

        [Test]
        public void Repli_RenvoieUnAutreAllieEnMain_AvecSesPv_SansSesEffets()
        {
            var svathurim = Mon("Svathurim", 200, Catalog.Repli, Catalog.Bouclier);
            var loup = Mon("Loup", 150, Catalog.Griffe);
            var enemy = Mon("Adversaire", 400, Catalog.Griffe);
            Start(new[] { svathurim, loup }, new[] { enemy }, new[] { (svathurim, 0), (loup, 1) }, new[] { (enemy, 0) });
            AdvanceTo(4, Side.Enemy);
            Use(enemy, Catalog.Griffe, loup);
            AdvanceTo(5, Side.Player);
            Use(svathurim, Catalog.Bouclier, loup);

            Assert.That(Engine.GetValidTargets(svathurim, IndexOf(svathurim, Catalog.Repli)), Has.No.Member(svathurim));
            Use(svathurim, Catalog.Repli, loup);

            Assert.That(loup.Zone, Is.EqualTo(MonsterZone.Hand));
            Assert.That(Engine.GetHand(Side.Player), Has.Member(loup));
            Assert.That(loup.Hp, Is.EqualTo(120));
            Assert.That(loup.Modifiers, Is.Empty);
            Assert.That(Engine.GetPoints(Side.Enemy), Is.EqualTo(0), "un monstre renvoyé n'est pas vaincu");
        }

        [Test]
        public void MonstreReposeApresRepli_SesRechargesRepartentAuMaximum()
        {
            var svathurim = Mon("Svathurim", 200, Catalog.Repli);
            var chouette = Mon("Chouette", 160, Catalog.Soin);
            var enemy = Mon("Adversaire", 400);
            Start(new[] { svathurim, chouette }, new[] { enemy }, new[] { (svathurim, 0), (chouette, 1) }, new[] { (enemy, 0) });
            AdvanceTo(5, Side.Player);
            Use(svathurim, Catalog.Repli, chouette);

            AdvanceTo(6, Side.Player);
            Engine.Place(chouette, 1);

            Assert.That(chouette.GetCooldown(0), Is.EqualTo(2));
        }
    }
}
