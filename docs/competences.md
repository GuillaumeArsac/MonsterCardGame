# Compétences

Ce fichier contient les règles communes à toutes les compétences, puis le catalogue des compétences.
Les règles du combat sont dans [combat.md](combat.md).

## Règles communes

**Soin**
- Un soin ne peut pas faire dépasser à un monstre ses PV de départ.

**Cumul**
- Les effets identiques se cumulent. Par exemple, deux bonus de +1 dégât donnent +2 dégâts.

**Ordre de calcul des dégâts**
1. On applique d'abord les **bonus** de dégâts.
2. On applique ensuite les **réductions** de dégâts.

**Temps de recharge**
- Toutes les compétences actives ont un temps de recharge.
- **Temps de recharge X :** après utilisation, la compétence ne peut pas être réutilisée pendant les X phases suivantes de ce monstre.
- Avec 0, elle est utilisable à chaque phase ; avec 1, une phase sur deux ; avec 2, une phase sur trois, etc.

**Ralentissement**
- Un monstre ralenti voit la recharge en cours de **toutes** ses compétences actives augmentée de 1 : chacune sera disponible une phase plus tard que prévu.
- Une compétence déjà disponible devient indisponible pendant la prochaine phase du monstre.
- Le ralentissement n'a pas de durée : il ne touche que la recharge en cours. Après la prochaine utilisation de la compétence, son temps de recharge normal s'applique.
- Le ralentissement est indépendant des dégâts : il s'applique même si les dégâts de la compétence sont réduits à 0.

**Durées**
- Quand une compétence applique un effet pendant **X tours**, l'effet dure jusqu'à la fin de la X-ième phase **de l'autre camp**.
- Cette règle vaut pour tous les effets à durée : bonus, réductions, etc.
- Exemple : le joueur applique un effet de 2 tours pendant sa phase.
  - l'effet dure pendant la fin de sa phase, puis toute la phase de l'adversaire (1re phase de l'autre camp) ;
  - puis toute la phase suivante du joueur, et toute la phase suivante de l'adversaire (2e phase de l'autre camp) ;
  - au début de la phase suivante du joueur, l'effet est terminé.
- Exemple : l'adversaire applique un effet de 1 tour pendant sa phase.
  - l'effet dure pendant la fin de sa phase, puis toute la phase suivante du joueur ;
  - au début de la phase suivante de l'adversaire, l'effet est terminé.
- Ainsi, un même effet a la même valeur, qu'il soit utilisé par le joueur ou par l'adversaire.

## Format d'une fiche

| Champ | Contenu |
|---|---|
| **Nom** | Court et évocateur |
| **Type** | Active ou passive |
| **Rôle visé** | Carry, support ou tank. Pour le design seulement, ce n'est pas affiché sur la carte |
| **Cible** | Un ennemi, un allié, ce monstre, tous les ennemis, tous les alliés, ou aucune |
| **Effet** | Ce qui se passe, avec des chiffres |
| **Temps de recharge** | Actives seulement |
| **Durée** | Instantané, X tours, ou permanent |
| **Déclencheur** | Passives seulement : quand l'effet s'applique |

Une fiche doit être assez précise pour que n'importe qui obtienne le même résultat sans avoir à deviner. Les chiffres peuvent être provisoires.

## Catalogue

### Grand balayage

| Champ | Valeur |
|---|---|
| **Type** | Active |
| **Rôle visé** | Tank |
| **Cible** | Tous les ennemis sur le terrain |
| **Effet** | Inflige 1 dégât à chaque ennemi sur le terrain et lui applique Ralentissement |
| **Temps de recharge** | 4 |
| **Durée** | Instantané |

Ne choisit pas de cible : ignore la Provocation.

### Griffe

| Champ | Valeur |
|---|---|
| **Type** | Active |
| **Rôle visé** | Carry |
| **Cible** | N'importe quel monstre sur le terrain, allié ou ennemi |
| **Effet** | Inflige 3 dégâts |
| **Temps de recharge** | 0 |
| **Durée** | Instantané |

Si elle vise un ennemi, elle est soumise à la Provocation.

### Soin

| Champ | Valeur |
|---|---|
| **Type** | Active |
| **Rôle visé** | Support |
| **Cible** | N'importe quel monstre sur le terrain, allié ou ennemi |
| **Effet** | Rend 4 PV |
| **Temps de recharge** | 1 |
| **Durée** | Instantané |

Si elle vise un ennemi, elle est soumise à la Provocation.

### Bénédiction

| Champ | Valeur |
|---|---|
| **Type** | Active |
| **Rôle visé** | Support |
| **Cible** | Tous les alliés sur le terrain |
| **Effet** | Les dégâts infligés par chaque allié ciblé augmentent de 1 |
| **Temps de recharge** | 2 |
| **Durée** | 6 tours |

Seuls les alliés présents sur le terrain au moment de l'utilisation reçoivent le bonus.
