using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using MonsterCardGame.Gameplay.Combat;
using MonsterCardGame.Gameplay.Skills.Effects;

namespace MonsterCardGame.Tests.EditMode.Combat
{
    /// <summary>Points, emplacements marqués, fatigue et fin du combat. Référence : docs/combat.md.</summary>
    public class VictoryTests : CombatTestBase
    {
        [Test]
        public void MonstreVaincu_EstRetire_EtRapporte1PointALAdversaire()
        {
            var loup = Mon("Loup", 150, Catalog.Griffe);
            var enemy = Mon("Adversaire", 30);
            var reserve = Mon("Réserve", 200);
            Start(new[] { loup }, new[] { enemy, reserve }, new[] { (loup, 0) }, new[] { (enemy, 1) });

            Use(loup, Catalog.Griffe, enemy);

            Assert.That(enemy.Zone, Is.EqualTo(MonsterZone.Defeated));
            Assert.That(Engine.GetSlot(Side.Enemy, 1), Is.Null);
            Assert.That(Engine.GetPoints(Side.Player), Is.EqualTo(1));
            Assert.That(Engine.State, Is.EqualTo(CombatState.Phase));
        }

        [Test]
        public void VaincreSonPropreAllie_DonneLePointALAdversaire()
        {
            var loup = Mon("Loup", 150, Catalog.Griffe);
            var allie = Mon("Allié fragile", 30);
            var enemy = Mon("Adversaire", 200);
            Start(new[] { loup, allie }, new[] { enemy }, new[] { (loup, 0), (allie, 1) }, new[] { (enemy, 0) });

            Use(loup, Catalog.Griffe, allie);

            Assert.That(Engine.GetPoints(Side.Enemy), Is.EqualTo(1));
            Assert.That(Engine.GetPoints(Side.Player), Is.EqualTo(0));
        }

        [Test]
        public void TroisPoints_GagneLeCombat()
        {
            var hydre = Mon("Hydre", 200, Catalog.Griffe, Catalog.Griffe, Catalog.Griffe);
            var enemies = Enumerable.Range(0, 4).Select(i => Mon($"Fragile {i}", 3)).ToArray();
            CombatEndedEvent? ended = null;
            Engine = new CombatEngine(new[] { hydre }, enemies, new Random(1));
            Engine.CombatEnded += e => ended = e;
            Engine.Setup(new[] { (hydre, 0) }, new[] { (enemies[0], 0), (enemies[1], 1), (enemies[2], 2) });

            for (int i = 0; i < 3; i++)
                Engine.UseSkill(hydre, i, enemies[i]);

            Assert.That(Engine.State, Is.EqualTo(CombatState.Ended));
            Assert.That(Engine.Winner, Is.EqualTo(Side.Player));
            Assert.That(ended?.Reason, Is.EqualTo(CombatEndReason.Points));
            Assert.That(Engine.GetHand(Side.Enemy).Count, Is.EqualTo(1), "il restait un monstre adverse en main");
            Assert.Throws<InvalidOperationException>(() => Engine.EndPhase());
        }

        [Test]
        public void CampSansMonstre_PerdQuelQueSoitLeScore()
        {
            var loup = Mon("Loup", 150, Catalog.Griffe);
            var fragile = Mon("Fragile", 30);
            var enemy = Mon("Adversaire", 30, Catalog.Griffe);
            Start(new[] { loup, fragile }, new[] { enemy }, new[] { (loup, 0), (fragile, 1) }, new[] { (enemy, 0) });
            Use(loup, Catalog.Griffe, fragile);
            Assert.That(Engine.GetPoints(Side.Enemy), Is.EqualTo(1));

            Engine.EndPhase();
            Use(enemy, Catalog.Griffe, enemy);

            Assert.That(Engine.Winner, Is.EqualTo(Side.Player), "l'adversaire mène 1 à 1 mais n'a plus de monstre");
            Assert.That(Engine.EndReason, Is.EqualTo(CombatEndReason.NoMonstersLeft));
        }

        // ── Emplacements marqués ──────────────────────────────────────────

        [Test]
        public void ChasseurDeTrophees_MarqueUnEmplacementAdverseAvantLaMiseEnPlace()
        {
            var archer = Mon("Archer", 140, Catalog.ChasseurDeTrophees);
            Engine = new CombatEngine(new[] { archer }, new[] { Mon("Adversaire", 200) }, new Random(1));

            Assert.That(Engine.GetMarkedSlots(Side.Enemy).Count, Is.EqualTo(1));
            Assert.That(Engine.GetMarkedSlots(Side.Enemy)[0], Is.InRange(0, 2));
            Assert.That(Engine.GetMarkedSlots(Side.Player), Is.Empty);
        }

        [Test]
        public void EmplacementMarque_RapporteUnPointDePlus_MemeSiLeChasseurEstEnMain()
        {
            var archer = Mon("Archer adverse", 140, Catalog.ChasseurDeTrophees);
            var aigle = Mon("Aigle adverse", 180, Catalog.Griffe);
            var victime = Mon("Victime", 30);
            var reserve = Mon("Réserve", 200);
            Engine = new CombatEngine(new[] { victime, reserve }, new[] { aigle, archer }, new Random(7));
            int marked = Engine.GetMarkedSlots(Side.Player)[0];
            int defeatPoints = -1;
            Engine.MonsterDefeated += e => defeatPoints = e.PointsAwarded;
            Engine.Setup(new[] { (victime, marked) }, new[] { (aigle, 0) });

            Engine.EndPhase();
            Use(aigle, Catalog.Griffe, victime);

            Assert.That(defeatPoints, Is.EqualTo(2));
            Assert.That(Engine.GetPoints(Side.Enemy), Is.EqualTo(2));
        }

        // ── Fatigue ───────────────────────────────────────────────────────

        [Test]
        public void Fatigue_APartirDuTour16_DegatsCroissantsAuDebutDuTour()
        {
            var joueur = Mon("Joueur", 1000, Catalog.Carapace);
            var enemy = Mon("Adversaire", 1000);
            StartDuel(joueur, enemy);

            AdvanceTo(15, Side.Enemy);
            Assert.That(joueur.Hp, Is.EqualTo(1000), "aucune fatigue jusqu'au tour 15");

            AdvanceTo(16, Side.Player);
            Assert.That(joueur.Hp, Is.EqualTo(990), "10 dégâts au tour 16, sans réduction");
            Assert.That(enemy.Hp, Is.EqualTo(990), "les deux camps");

            AdvanceTo(17, Side.Player);
            Assert.That(joueur.Hp, Is.EqualTo(970));
            AdvanceTo(18, Side.Player);
            Assert.That(joueur.Hp, Is.EqualTo(940));
        }

        [Test]
        public void VaincuParLaFatigue_RapporteSesPoints_EmplacementMarqueCompris()
        {
            var archer = Mon("Archer adverse", 1000, Catalog.ChasseurDeTrophees);
            var victime = Mon("Victime", 10);
            var reserve = Mon("Réserve", 1000);
            Engine = new CombatEngine(new[] { victime, reserve }, new[] { archer }, new Random(3));
            int marked = Engine.GetMarkedSlots(Side.Player)[0];
            var defeats = new List<MonsterDefeatedEvent>();
            Engine.MonsterDefeated += defeats.Add;
            Engine.Setup(new[] { (victime, marked) }, new[] { (archer, 0) });

            AdvanceTo(16, Side.Player);

            Assert.That(defeats.Single().ByFatigue, Is.True);
            Assert.That(defeats.Single().PointsAwarded, Is.EqualTo(2));
            Assert.That(Engine.GetPoints(Side.Enemy), Is.EqualTo(2));
        }

        [Test]
        public void LesDeuxCampsPerdentLeurDernierMonstreEnMemeTemps_LeJoueurPerd()
        {
            StartDuel(Mon("Joueur", 10), Mon("Adversaire", 10));

            while (Engine.State != CombatState.Ended) Engine.EndPhase();

            Assert.That(Engine.Turn, Is.EqualTo(16));
            Assert.That(Engine.Winner, Is.EqualTo(Side.Enemy));
            Assert.That(Engine.EndReason, Is.EqualTo(CombatEndReason.BothSidesEmpty));
        }

        // ── Événements ────────────────────────────────────────────────────

        [Test]
        public void Evenements_DansLOrdre_PourUneAttaqueQuiFinitLeCombat()
        {
            var loup = Mon("Loup", 150, Catalog.Griffe);
            var enemy = Mon("Adversaire", 30);
            var log = new List<string>();
            Engine = new CombatEngine(new[] { loup }, new[] { enemy }, new Random(1));
            Engine.MonsterPlaced   += e => log.Add($"pose {e.Monster.Name}");
            Engine.PhaseStarted    += e => log.Add($"début {e.Side} {e.Turn}");
            Engine.SkillUsed       += e => log.Add($"compétence {e.Skill.SkillName}");
            Engine.Damaged         += e => log.Add($"dégâts {e.Amount}");
            Engine.MonsterDefeated += e => log.Add($"vaincu {e.Monster.Name}");
            Engine.CombatEnded     += e => log.Add($"fin {e.Winner}");
            Engine.Setup(new[] { (loup, 0) }, new[] { (enemy, 0) });

            Use(loup, Catalog.Griffe, enemy);

            Assert.That(log, Is.EqualTo(new[]
            {
                "pose Loup", "pose Adversaire", "début Player 1",
                "compétence Griffe", "dégâts 30", "vaincu Adversaire", "fin Player"
            }));
        }

        [Test]
        public void EffetInconnu_EstSignale()
        {
            var bizarre = Active("Bizarre", 0, new UnknownEffect());
            var monstre = Mon("Monstre", 200, bizarre);
            StartDuel(monstre, Mon("Adversaire", 200));

            Assert.Throws<NotSupportedException>(() => Use(monstre, bizarre, monstre));
        }

        [Serializable]
        private class UnknownEffect : SkillEffect { }
    }
}
