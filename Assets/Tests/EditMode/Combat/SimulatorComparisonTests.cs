using System.Linq;
using NUnit.Framework;
using UnityEngine;
using MonsterCardGame.Gameplay.Combat;
using MonsterCardGame.Gameplay.Combat.AI;
using MonsterCardGame.Gameplay.Monsters;

namespace MonsterCardGame.Tests.EditMode.Combat
{
    /// <summary>
    /// Le moteur et l'IA C# donnent les mêmes tendances que le simulateur Python.
    /// Références : résultats notés dans docs/monstres.md et docs/combats-scenario.md (2 000 combats chacun).
    /// </summary>
    public class SimulatorComparisonTests
    {
        /// <summary>1 000 combats par configuration : assez pour les tendances, et la suite reste rapide. Graine fixe : résultat stable.</summary>
        private const int Combats = 1000;
        private TestCatalog _catalog;

        [SetUp]    public void SetUp()    => _catalog = new TestCatalog();
        [TearDown] public void TearDown() => _catalog.Dispose();

        private static CombatSimulation.Report Run(string label, MonsterCardData[] player, MonsterCardData[] enemy)
        {
            var report = CombatSimulation.Run(() => player.Select(CombatMonster.From), () => enemy.Select(CombatMonster.From), Combats, seed: 12345);
            Debug.Log($"[Simulation] {label} — {report}");
            return report;
        }

        private MonsterCardData[] Patrouille() => _catalog.PatrouilleCentaure().Monsters.ToArray();

        private MonsterCardData[] Depart(params string[] names)
        {
            var all = _catalog.TeamStart().ToList();
            all.Add(_catalog.Monster("Luciole sacrée", Gameplay.Skills.MonsterRole.Support, 160, 0, _catalog.Benediction));
            all.Add(_catalog.Monster("Sanglier",       Gameplay.Skills.MonsterRole.Tank,    240, 0, _catalog.CoupDeBouclier));
            return names.Select(n => all.First(m => m.MonsterName.StartsWith(n))).ToArray();
        }

        /// <summary>Écart toléré sur la part de victoires, en points.</summary>
        private const double WinRateTolerance = 0.06;
        /// <summary>Écart toléré sur la durée moyenne, en tours.</summary>
        private const double TurnsTolerance = 1.5;

        private static void AssertClose(CombatSimulation.Report report, double expectedWinRate, double expectedTurns)
        {
            Assert.That(report.PlayerWinRate, Is.EqualTo(expectedWinRate).Within(WinRateTolerance), report.ToString());
            Assert.That(report.AverageTurns,  Is.EqualTo(expectedTurns).Within(TurnsTolerance),     report.ToString());
        }

        [Test]
        public void EquipeA_ContreB()
        {
            var report = Run("A contre B (doc : ~35 %, ~8 tours)", _catalog.TeamA(), _catalog.TeamB());

            AssertClose(report, 0.35, 8);
            Assert.That(report.ShareTurns7To15, Is.GreaterThan(0.9), "plus de 90 % des combats entre 7 et 15 tours");
            Assert.That(report.AverageFirstDefeatTurn, Is.EqualTo(5).Within(1), "premier monstre vaincu vers le tour 5");
        }

        [TestCase(new[] { "Loup", "Renard", "Ours", "Luciole", "Chien" },     0.78, 10)]
        [TestCase(new[] { "Loup", "Renard", "Ours", "Chouette", "Chien" },    0.83, 10)]
        [TestCase(new[] { "Loup", "Ours", "Chouette", "Luciole", "Chien" },   0.30, 12)]
        [TestCase(new[] { "Loup", "Renard", "Ours", "Chouette", "Sanglier" }, 0.01, 10)]
        public void Depart_ContrePatrouille(string[] team, double expectedWinRate, double expectedTurns)
        {
            var report = Run($"{string.Join(", ", team)} contre la patrouille (doc : {expectedWinRate:P0}, {expectedTurns} tours)",
                             Depart(team), Patrouille());

            AssertClose(report, expectedWinRate, expectedTurns);
        }
    }
}
