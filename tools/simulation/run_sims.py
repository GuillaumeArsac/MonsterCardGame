"""Lance des séries de combats et affiche les statistiques.

Utilisation :
    python3 run_sims.py                      # rapport de référence
    python3 run_sims.py A B                  # A (joueur) contre B (adversaire)
    python3 run_sims.py A B --n 5000 --details
"""
import argparse
import random
import statistics
from collections import Counter, defaultdict

import combat_sim as cs


def run(player, enemy, n=2000, seed=1):
    """Joue n combats entre deux équipes de cs.TEAMS et renvoie les statistiques."""
    rng = random.Random(seed)
    stats = defaultdict(lambda: defaultdict(float))
    wins, turns, reasons, first_kills = 0, [], Counter(), []
    for _ in range(n):
        game = cs.Game(player, enemy, rng, stats)
        winner, turn, reason = game.play()
        wins += winner == 0
        turns.append(turn)
        reasons[reason] += 1
        if game.first_kill_turn:
            first_kills.append(game.first_kill_turn)
    q = sorted(turns)
    return dict(
        n=n, win=wins / n, mean=statistics.mean(turns),
        p10=q[n // 10], med=q[n // 2], p90=q[9 * n // 10],
        in_range=sum(7 <= t <= 15 for t in turns) / n,
        fatigue=sum(t >= cs.CFG["fatigue_start"] for t in turns) / n,
        first_kill=statistics.mean(first_kills) if first_kills else float("nan"),
        reasons=reasons, stats=stats,
    )


def show(title, r):
    print(f"\n=== {title}  ({r['n']} combats)")
    print(f"  victoire du joueur : {r['win'] * 100:.0f} %")
    print(f"  durée : moyenne {r['mean']:.1f} | p10 {r['p10']} | médiane {r['med']} | p90 {r['p90']}")
    print(f"  entre 7 et 15 tours : {r['in_range'] * 100:.0f} %   | fatigue atteinte : {r['fatigue'] * 100:.0f} %")
    print(f"  premier monstre vaincu au tour : {r['first_kill']:.1f}")
    print("  fins :", dict(r["reasons"]))


def show_monsters(r):
    n = r["n"]
    print(f"\n  {'monstre':22s} {'poses':>6s} {'dégâts':>7s} {'kills':>6s} {'morts':>6s} {'soins':>6s}  compétences utilisées par combat")
    for name, s in r["stats"].items():
        if name.startswith("_"):
            continue
        uses = {k[5:]: v / n for k, v in s.items() if k.startswith("uses:")}
        u = ", ".join(f"{k} {v:.1f}" for k, v in sorted(uses.items(), key=lambda kv: -kv[1]))
        print(f"  {name:22s} {s['poses'] / n:6.2f} {s['dégâts'] / n:7.1f} {s['kills'] / n:6.2f} "
              f"{s['morts'] / n:6.2f} {s['soins'] / n:6.1f}  {u}")


def reference_report(n):
    for player, enemy in [("A", "B"), ("B", "A"), ("A", "A"), ("B", "B"), ("Départ", "Patrouille")]:
        show(f"{player} (joueur) contre {enemy} (adversaire)", run(player, enemy, n))


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Simulations de combat MonsterCardGame")
    parser.add_argument("player", nargs="?", help="équipe du joueur (clé de TEAMS)")
    parser.add_argument("enemy", nargs="?", help="équipe adverse (clé de TEAMS)")
    parser.add_argument("--n", type=int, default=2000, help="nombre de combats (2000 par défaut)")
    parser.add_argument("--details", action="store_true", help="statistiques par monstre")
    args = parser.parse_args()

    if args.player and args.enemy:
        result = run(args.player, args.enemy, args.n)
        show(f"{args.player} (joueur) contre {args.enemy} (adversaire)", result)
        if args.details:
            show_monsters(result)
    else:
        reference_report(args.n)
