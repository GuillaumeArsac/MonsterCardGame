using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using MonsterCardGame.Core;
using MonsterCardGame.Gameplay.Combat;
using MonsterCardGame.Gameplay.Combat.AI;
using MonsterCardGame.Gameplay.Content;
using MonsterCardGame.Gameplay.Monsters;

namespace MonsterCardGame.Editor
{
    /// <summary>
    /// Simule toutes les équipes de 5 possibles parmi les monstres de départ, contre les combats du contenu,
    /// avec l'IA des deux côtés. Le rapport est écrit dans la console et dans Logs/Simulations.
    /// Les assets sont lus tels quels : relancer après chaque réglage pour en voir l'effet.
    /// </summary>
    public static class BalanceSimulator
    {
        private const int Combats = 1000;
        private const int Seed = 12345;
        private const string ReportFolder = "Logs/Simulations";

        [MenuItem("MonsterCardGame/Équilibrage/Simuler les équipes de départ")]
        public static void SimulateStarterTeams()
        {
            var content = Resources.Load<ContentDatabase>(ContentDatabase.ResourcePath);
            if (content == null)
            {
                Debug.LogError("[BalanceSimulator] ContentDatabase introuvable : générer d'abord le contenu");
                return;
            }

            var starters = content.StarterMonsters.ToList();
            var opponents = new[] { "plaines-patrouille-centaure", "plaines-avant-garde-de-thrak", "test-horde-de-thrak" }
                            .Select(content.FindEncounter).Where(e => e != null).ToList();
            var teams = Combinations(starters, GameRules.TeamSize).ToList();

            var report = new StringBuilder();
            report.AppendLine("# Simulation des équipes de départ");
            report.AppendLine();
            report.AppendLine($"{Combats} combats par équipe, IA contre IA, le joueur commence. Graine {Seed}.");

            try
            {
                int done = 0, total = teams.Count * opponents.Count;
                foreach (var encounter in opponents)
                {
                    var enemy = encounter.Monsters.ToList();
                    var results = new List<(List<MonsterCardData> team, CombatSimulation.Report result)>();
                    foreach (var team in teams)
                    {
                        if (EditorUtility.DisplayCancelableProgressBar("Simulation", $"{encounter.DisplayName} : {Names(team)}", (float)done++ / total))
                            return;
                        results.Add((team, CombatSimulation.Run(() => team.Select(CombatMonster.From),
                                                                 () => enemy.Select(CombatMonster.From), Combats, Seed)));
                    }
                    AppendEncounter(report, encounter.DisplayName, starters, results);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            Directory.CreateDirectory(ReportFolder);
            var path = Path.Combine(ReportFolder, "equipes-de-depart.md");
            File.WriteAllText(path, report.ToString(), Encoding.UTF8);
            Debug.Log($"[BalanceSimulator] Rapport écrit dans {path}\n{report}");
        }

        private static void AppendEncounter(StringBuilder report, string title, List<MonsterCardData> starters,
                                            List<(List<MonsterCardData> team, CombatSimulation.Report result)> results)
        {
            report.AppendLine();
            report.AppendLine($"## Contre {title}");
            report.AppendLine();
            report.AppendLine("| Équipe | Victoires | Durée moyenne | Fatigue | Premier vaincu |");
            report.AppendLine("|---|---|---|---|---|");
            foreach (var (team, r) in results.OrderByDescending(x => x.result.PlayerWinRate))
                report.AppendLine($"| {Names(team)} | {Percent(r.PlayerWinRate)} | {r.AverageTurns:F1} tours | {Percent(r.ShareFatigue)} | tour {r.AverageFirstDefeatTurn:F1} |");

            report.AppendLine();
            report.AppendLine("Victoires moyennes des équipes avec ou sans chaque monstre :");
            report.AppendLine();
            report.AppendLine("| Monstre | Avec | Sans |");
            report.AppendLine("|---|---|---|");
            foreach (var monster in starters)
            {
                var with    = results.Where(x => x.team.Contains(monster)).Select(x => x.result.PlayerWinRate).DefaultIfEmpty().Average();
                var without = results.Where(x => !x.team.Contains(monster)).Select(x => x.result.PlayerWinRate).DefaultIfEmpty().Average();
                report.AppendLine($"| {monster.MonsterName} | {Percent(with)} | {Percent(without)} |");
            }
        }

        private static string Percent(double share) => $"{Math.Round(share * 100):0} %";

        private static string Names(IEnumerable<MonsterCardData> team)
            => string.Join(", ", team.Select(m => m.MonsterName.Replace("Essence ", "")));

        private static IEnumerable<List<T>> Combinations<T>(IReadOnlyList<T> items, int size, int start = 0)
        {
            if (size == 0) { yield return new List<T>(); yield break; }
            for (int i = start; i <= items.Count - size; i++)
                foreach (var rest in Combinations(items, size - 1, i + 1))
                {
                    rest.Insert(0, items[i]);
                    yield return rest;
                }
        }
    }
}
