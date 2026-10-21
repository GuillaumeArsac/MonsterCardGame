using System;
using System.Collections.Generic;
using System.Linq;
using MonsterCardGame.Core;

namespace MonsterCardGame.Gameplay.Combat.AI
{
    /// <summary>
    /// Joue des combats IA contre IA pour mesurer l'équilibre, comme tools/simulation/run_sims.py.
    /// Le joueur joue toujours en premier.
    /// </summary>
    public static class CombatSimulation
    {
        /// <summary>Sécurité : la fatigue termine les combats bien avant.</summary>
        public const int MaxTurns = 60;

        public class Report
        {
            public int    Combats;
            public int    PlayerWins;
            public double AverageTurns;
            /// <summary>Part des combats qui durent entre 7 et 15 tours (la cible de design).</summary>
            public double ShareTurns7To15;
            /// <summary>Part des combats qui atteignent la fatigue.</summary>
            public double ShareFatigue;
            /// <summary>Tour moyen du premier monstre vaincu.</summary>
            public double AverageFirstDefeatTurn;

            public double PlayerWinRate => Combats == 0 ? 0 : (double)PlayerWins / Combats;

            public override string ToString()
                => $"{Combats} combats : joueur {PlayerWinRate:P0}, durée moyenne {AverageTurns:F1} tours, "
                 + $"7 à 15 tours {ShareTurns7To15:P0}, fatigue {ShareFatigue:P0}, premier vaincu au tour {AverageFirstDefeatTurn:F1}";
        }

        /// <param name="playerTeam">Crée une équipe neuve pour chaque combat.</param>
        public static Report Run(Func<IEnumerable<CombatMonster>> playerTeam, Func<IEnumerable<CombatMonster>> enemyTeam,
                                 int combats, int seed)
        {
            var random = new Random(seed);
            var turns  = new List<int>();
            var firstDefeats = new List<int>();
            int playerWins = 0;

            for (int i = 0; i < combats; i++)
            {
                var (winner, turn, firstDefeat) = PlayOne(playerTeam(), enemyTeam(), random.Next());
                if (winner == Side.Player) playerWins++;
                turns.Add(turn);
                if (firstDefeat > 0) firstDefeats.Add(firstDefeat);
            }

            return new Report
            {
                Combats                = combats,
                PlayerWins             = playerWins,
                AverageTurns           = turns.Average(),
                ShareTurns7To15        = turns.Count(t => t >= 7 && t <= 15) / (double)combats,
                ShareFatigue           = turns.Count(t => t >= GameRules.FatigueStartTurn) / (double)combats,
                AverageFirstDefeatTurn = firstDefeats.Count > 0 ? firstDefeats.Average() : 0
            };
        }

        /// <summary>Joue un combat complet. Retourne le vainqueur, le dernier tour et le tour du premier monstre vaincu (0 si aucun).</summary>
        public static (Side winner, int turns, int firstDefeatTurn) PlayOne(IEnumerable<CombatMonster> playerTeam,
                                                                             IEnumerable<CombatMonster> enemyTeam, int seed)
        {
            var random = new Random(seed);
            var engine = new CombatEngine(playerTeam, enemyTeam, new Random(random.Next()));
            var playerAI = new CombatAI(new Random(random.Next()));
            var enemyAI  = new CombatAI(new Random(random.Next()));

            int firstDefeat = 0;
            engine.MonsterDefeated += _ => { if (firstDefeat == 0) firstDefeat = engine.Turn; };

            engine.Setup(playerAI.ChooseSetup(engine, Side.Player), enemyAI.ChooseSetup(engine, Side.Enemy));

            while (engine.State == CombatState.Phase && engine.Turn <= MaxTurns)
            {
                (engine.CurrentSide == Side.Player ? playerAI : enemyAI).PlayPhase(engine);
                if (engine.State == CombatState.Phase) engine.EndPhase();
            }

            // Comme le simulateur : au-delà de la limite, l'adversaire gagne
            var winner = engine.Winner ?? Side.Enemy;
            return (winner, engine.Turn, firstDefeat);
        }
    }
}
