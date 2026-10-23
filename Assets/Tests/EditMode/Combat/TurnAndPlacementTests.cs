using System;
using System.Linq;
using NUnit.Framework;
using MonsterCardGame.Gameplay.Combat;

namespace MonsterCardGame.Tests.EditMode.Combat
{
    /// <summary>Mise en place, tours et pose. Référence : docs/combat.md (Le terrain, Déroulement).</summary>
    public class TurnAndPlacementTests : CombatTestBase
    {
        [Test]
        public void MiseEnPlace_LesDeuxCampsPosentEnMemeTemps_PuisLeJoueurCommence()
        {
            var p1 = Mon("Joueur 1", 200);
            var p2 = Mon("Joueur 2", 200);
            var e1 = Mon("Adversaire", 200);
            int placedEvents = 0;

            Engine = new CombatEngine(new[] { p1, p2 }, new[] { e1 }, new Random(1));
            Engine.MonsterPlaced += e => { if (e.DuringSetup) placedEvents++; };
            Engine.Setup(new[] { (p1, 0), (p2, 2) }, new[] { (e1, 1) });

            Assert.That(placedEvents, Is.EqualTo(3));
            Assert.That(Engine.GetSlot(Side.Player, 0), Is.SameAs(p1));
            Assert.That(Engine.GetSlot(Side.Player, 2), Is.SameAs(p2));
            Assert.That(Engine.GetSlot(Side.Enemy, 1), Is.SameAs(e1));
            Assert.That(Engine.GetHand(Side.Player), Is.Empty);
            Assert.That(Engine.Turn, Is.EqualTo(1));
            Assert.That(Engine.CurrentSide, Is.EqualTo(Side.Player));
        }

        [Test]
        public void MiseEnPlace_DeuxMonstresSurLeMemeEmplacement_EstRefusee()
        {
            var p1 = Mon("Joueur 1", 200);
            var p2 = Mon("Joueur 2", 200);
            Engine = new CombatEngine(new[] { p1, p2 }, new[] { Mon("Adversaire", 200) });

            Assert.Throws<InvalidOperationException>(() => Engine.Setup(new[] { (p1, 0), (p2, 0) }, null));
        }

        [Test]
        public void UnTour_EstLaPhaseDuJoueurPuisCelleDeLAdversaire()
        {
            StartDuel(Mon("Joueur", 200), Mon("Adversaire", 200));

            Engine.EndPhase();
            Assert.That((Engine.Turn, Engine.CurrentSide), Is.EqualTo((1, Side.Enemy)));

            Engine.EndPhase();
            Assert.That((Engine.Turn, Engine.CurrentSide), Is.EqualTo((2, Side.Player)));
        }

        [Test]
        public void Pose_SurUnEmplacementLibre_PendantSaPhase()
        {
            var posé = Mon("Posé", 200);
            var enMain = Mon("En main", 200);
            var enemy = Mon("Adversaire", 200);
            Start(new[] { posé, enMain }, new[] { enemy }, new[] { (posé, 0) }, new[] { (enemy, 0) });

            Assert.That(Engine.CanPlace(enMain, 0), Is.False, "emplacement occupé");
            Engine.Place(enMain, 2);

            Assert.That(enMain.Zone, Is.EqualTo(MonsterZone.Field));
            Assert.That(enMain.Slot, Is.EqualTo(2));
        }

        [Test]
        public void Pose_PendantLaPhaseAdverse_EstRefusee()
        {
            var enMain = Mon("En main", 200);
            var posé = Mon("Posé", 200);
            var enemy = Mon("Adversaire", 200);
            Start(new[] { posé, enMain }, new[] { enemy }, new[] { (posé, 0) }, new[] { (enemy, 0) });

            Engine.EndPhase();

            Assert.That(Engine.CanPlace(enMain, 1), Is.False);
            Assert.Throws<InvalidOperationException>(() => Engine.Place(enMain, 1));
        }

        [Test]
        public void Pose_ApresUneCompetence_EstRefusee()
        {
            var griffon = Mon("Griffon", 200, Catalog.Griffe);
            var enMain = Mon("En main", 200);
            var enemy = Mon("Adversaire", 200);
            Start(new[] { griffon, enMain }, new[] { enemy }, new[] { (griffon, 0) }, new[] { (enemy, 0) });

            Use(griffon, Catalog.Griffe, enemy);

            Assert.That(Engine.PlacementLocked, Is.True);
            Assert.That(Engine.CanPlace(enMain, 1), Is.False);
        }

        [Test]
        public void MonstrePoseDansLaPhase_PeutAgirTout_De_Suite()
        {
            var tank = Mon("Tank", 300);
            var loup = Mon("Loup", 150, Catalog.Griffe);
            var enemy = Mon("Adversaire", 200);
            Start(new[] { tank, loup }, new[] { enemy }, new[] { (tank, 0) }, new[] { (enemy, 0) });

            Engine.Place(loup, 1);
            Use(loup, Catalog.Griffe, enemy);

            Assert.That(enemy.Hp, Is.EqualTo(170));
        }

        [Test]
        public void MonstreEnMain_NePeutPasEtreCible()
        {
            var loup = Mon("Loup", 150, Catalog.Griffe);
            var enemyField = Mon("Adversaire posé", 200);
            var enemyHand = Mon("Adversaire en main", 200);
            Start(new[] { loup }, new[] { enemyField, enemyHand }, new[] { (loup, 0) }, new[] { (enemyField, 0) });

            var targets = Engine.GetValidTargets(loup, IndexOf(loup, Catalog.Griffe));

            Assert.That(targets, Has.No.Member(enemyHand));
            Assert.Throws<InvalidOperationException>(() => Use(loup, Catalog.Griffe, enemyHand));
        }

        [Test]
        public void MonstreSansActive_NAgitPas()
        {
            var tank = Mon("Tank", 300, Catalog.Provocation, Catalog.Carapace);
            StartDuel(tank, Mon("Adversaire", 200));

            Assert.That(Enumerable.Range(0, tank.Skills.Count).Any(i => Engine.CanUseSkill(tank, i)), Is.False);
        }
    }
}
