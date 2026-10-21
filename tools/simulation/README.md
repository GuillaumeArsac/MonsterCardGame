# Simulateur de combat

Script Python qui joue des milliers de combats automatiquement, pour mesurer la durée des combats et l'équilibre entre équipes.
Il applique les règles de [docs/combat.md](../../docs/combat.md) et [docs/competences.md](../../docs/competences.md).

Il sert à deux choses :
- **tester une idée de design** (un monstre, une compétence, une règle) avant de l'implémenter dans Unity ;
- **servir de référence** pour le moteur de combat C# : à règles égales, les deux doivent donner des résultats proches.

## Utilisation

Python 3, sans dépendance externe. Depuis ce dossier :

```
python3 run_sims.py                          # rapport de référence (toutes les équipes de test)
python3 run_sims.py A B                      # équipe A (joueur) contre équipe B (adversaire)
python3 run_sims.py Départ Patrouille --details   # avec les statistiques par monstre
python3 run_sims.py A B --n 5000             # nombre de combats (2000 par défaut)
```

Le joueur joue toujours en premier, comme dans le jeu.

**Ce que le rapport affiche :**
- le pourcentage de victoires du joueur ;
- la durée des combats en tours (moyenne, 10 % les plus courts, médiane, 10 % les plus longs) ;
- la part des combats entre 7 et 15 tours (la cible de design) et la part qui atteint la fatigue ;
- le tour moyen du premier monstre vaincu ;
- avec `--details` : par monstre, le nombre de poses, les dégâts, les monstres vaincus, les morts, les soins et l'usage de chaque compétence.

## Modifier le contenu

Tout se trouve en haut de `combat_sim.py` :

| Quoi | Où |
|---|---|
| Temps de recharge des compétences actives | `ACTIVE` |
| Compétences passives | `PASSIVE` |
| Durée de Bénédiction, tour de début de la fatigue | `CFG` |
| Monstres (rôle, PV, compétences) | `MONSTERS` |
| Équipes de 5 monstres | `TEAMS` |

Une nouvelle compétence demande aussi de coder son effet dans `Game.act` et sa valeur pour l'IA dans `Game.score`.

## Équipes disponibles

| Clé | Équipe | Source |
|---|---|---|
| `A` | Équipe de test du joueur | docs/monstres.md |
| `B` | La horde de Thrâk | docs/monstres.md |
| `Départ` | Meilleure équipe de 5 de la collection de départ | docs/monstres.md |
| `Patrouille` | Combat 1, la patrouille centaure | docs/combats-scenario.md |

## Limites

- **L'IA est simple.** Elle choisit le meilleur coup immédiat sans anticiper, pose toujours de quoi remplir le terrain, et utilise mal Repli et Bouclier. Un joueur humain jouera mieux, surtout avec les supports.
- **L'IA ne vise que des ennemis avec ses attaques et que des alliés avec ses soins**, alors que les règles autorisent n'importe quelle cible.
- **Les chiffres donnent des tendances**, pas un équilibrage final.
