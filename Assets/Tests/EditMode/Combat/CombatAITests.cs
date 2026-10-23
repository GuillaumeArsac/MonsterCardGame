using System;
using System.Linq;
using NUnit.Framework;
using MonsterCardGame.Gameplay.Combat;
using MonsterCardGame.Gameplay.Combat.AI;
using MonsterCardGame.Gameplay.Monsters;
using MonsterCardGame.Gameplay.Skills;

namespace MonsterCardGame.Tests.EditMode.Combat
{
    /// <summary>IA adverse, portée de tools/simulation/combat_sim.py.</summary>
    public class CombatAITests : CombatTestBase
    {
        [Test]
        public void PoidsDeMenace_IdentiquesAuSimulateur()
        {
            Assert.That(CombatAI.ThreatWeight(Catalog.Griffe),            Is.EqualTo(3));
            Assert.That(CombatAI.ThreatWeight(Catalog.MorsurePerforante), Is.EqualTo(2));
            Assert.That(CombatAI.ThreatWeight(Catalog.Execution),         Is.EqualTo(2));
            Assert.That(CombatAI.ThreatWeight(Catalog.GrandBalayage),     Is.EqualTo(2));
            Assert.That(CombatAI.ThreatWeight(Catalog.CoupDeBouclier),    Is.EqualTo(1.5));
            Assert.That(CombatAI.ThreatWeight(Catalog.Soin),              Is.EqualTo(2));
            Assert.That(CombatAI.ThreatWeight(Catalog.Benediction),       Is.EqualTo(2.5));
            Assert.That(CombatAI.ThreatWeight(Catalog.Bouclier),          Is.EqualTo(1));
            Assert.That(CombatAI.ThreatWeight(Catalog.Repli),             Is.EqualTo(0.5));
        }

        [Test]
        public void ToutesLesActivesDuCatalogue_SontReconnues()
        {
            foreach (var skill in Catalog.AllSkills.Where(s => s.IsActive))
                Assert.That(CombatAI.Classify(skill), Is.Not.EqualTo(CombatAI.SkillKind.Unknown), skill.SkillName);
        }

        [Test]
        public void MiseEnPlace_RemplitLeTerrain_AvecUnTank()
        {
            var team = Catalog.TeamA().Select(CombatMonster.From).ToList();
            var engine = new CombatEngine(team, Catalog.TeamB().Select(CombatMonster.From), new Random(1));
            var ai = new CombatAI(new Random(2));

            var setup = ai.ChooseSetup(engine, Side.Player);

            Assert.That(setup.Count, Is.EqualTo(3));
            Assert.That(setup.Select(p => p.slot).Distinct().Count(), Is.EqualTo(3));
            Assert.That(setup.Any(p => p.monster.Role == MonsterRole.Tank), Is.True);
        }

        [Test]
        public void Phase_VaincLaCibleQuandCEstPossible()
        {
            var loup = Mon("Loup", 150, Catalog.Griffe);
            var fragile = Mon("Fragile", 30);
            var solide = Mon("Solide", 300);
            Start(new[] { loup }, new[] { fragile, solide }, new[] { (loup, 0) }, new[] { (fragile, 0), (solide, 1) });

            new CombatAI(new Random(1)).PlayPhase(Engine);

            Assert.That(fragile.Zone, Is.EqualTo(MonsterZone.Defeated));
            Assert.That(solide.Hp, Is.EqualTo(300));
        }

        [Test]
        public void Phase_PoseDAbordPuisAgit()
        {
            var tank = Mon("Tank", 300);
            var loup = Mon("Loup", 150, Catalog.Griffe);
            var enemy = Mon("Adversaire", 400);
            Start(new[] { tank, loup }, new[] { enemy }, new[] { (tank, 0) }, new[] { (enemy, 0) });

            new CombatAI(new Random(1)).PlayPhase(Engine);

            Assert.That(loup.IsOnField, Is.True);
            Assert.That(enemy.Hp, Is.EqualTo(370));
        }

        [Test]
        public void Phase_NeSoignePasUnAllieEnBonneSante()
        {
            var chouette = Mon("Chouette", 160, Catalog.Soin);
            StartDuel(chouette, Mon("Adversaire", 400));
            AdvanceTo(3, Side.Player);

            new CombatAI(new Random(1)).PlayPhase(Engine);

            Assert.That(chouette.WasUsedThisPhase(0), Is.False);
        }

        [Test]
        public void CombatIAContreIA_SeTermineToujours()
        {
            for (int seed = 0; seed < 50; seed++)
            {
                var a = Catalog.TeamA().Select(CombatMonster.From);
                var b = Catalog.TeamB().Select(CombatMonster.From);
                var (_, turns, _) = CombatSimulation.PlayOne(a, b, seed);
                Assert.That(turns, Is.LessThan(CombatSimulation.MaxTurns), $"graine {seed}");
            }
        }
    }
}
