# Compétences

Ce fichier contient les règles communes à toutes les compétences, puis le catalogue des compétences.
Les règles du combat sont dans [combat.md](combat.md).

## Règles communes

**Soin**
- Un soin ne peut pas faire dépasser à un monstre ses PV de départ.
- Sur un monstre qui **saigne**, un soin retire tous ses saignements **au lieu de** rendre des PV. Cela vaut pour toutes les compétences qui soignent (Soin, Aura apaisante, Souffle de vie).

**Cumul**
- Les effets identiques se cumulent. Par exemple, deux bonus de +10 dégâts donnent +20 dégâts.

**Ordre de calcul des dégâts**
1. On applique d'abord les **bonus** de dégâts.
2. On applique ensuite les **réductions** de dégâts.

**Temps de recharge**
- Chaque compétence active a un **temps de recharge** (sur sa fiche) et un **compteur de recharge** (pendant le combat).
- Une compétence est **disponible** quand son compteur est à 0.
- Quand une compétence est utilisée, son compteur prend la valeur de son temps de recharge.
- À la **fin de la phase d'un camp**, les compteurs supérieurs à 0 des monstres de **ce camp** baissent de 1.
  - Fin de la phase du joueur : les compteurs des monstres du joueur baissent de 1.
  - Fin de la phase de l'adversaire : les compteurs des monstres de l'adversaire baissent de 1.

| Temps de recharge | Utilisable |
|---|---|
| 0 | à chaque phase |
| 1 | à chaque phase, sauf celle où le monstre est posé |
| 2 | une phase sur deux |
| 3 | une phase sur trois |

**Entrée sur le terrain**
- Un monstre qui arrive sur le terrain a tous ses compteurs au maximum, c'est-à-dire égaux à son temps de recharge.
- Exemple : un monstre posé au tour 1 avec une compétence à 0 et une compétence à 1.
  - La compétence à 0 est utilisable immédiatement, dès le tour 1.
  - À la fin de la phase du joueur au tour 1, le compteur de la seconde passe de 1 à 0 : elle est disponible au tour 2.
- Cette règle s'applique aussi aux monstres posés lors de la mise en place.

**Ralentissement**
- Un monstre ralenti voit le compteur de **toutes** ses compétences actives augmenter de 1, y compris celles qui sont disponibles.
- Comme les compteurs ne baissent qu'à la fin de la phase du camp du monstre, une compétence disponible qui est ralentie est bloquée pendant la prochaine phase de ce monstre, quel que soit le camp qui applique le ralentissement.
- Le ralentissement n'a pas de durée : il ne touche que le compteur en cours. Après la prochaine utilisation de la compétence, son temps de recharge normal s'applique.
- Le ralentissement est indépendant des dégâts : il s'applique même si les dégâts de la compétence sont réduits à 0.

**Saignement**
- Au début de chaque phase de son camp, un monstre qui saigne perd **5 % de ses PV max par saignement**, arrondi à l'entier inférieur, avec 1 dégât minimum par saignement.
- Les saignements se cumulent et ne s'arrêtent pas seuls : seul un soin les retire (voir Soin). Un retour en main ou la défaite les retire aussi.
- Ces dégâts ignorent les bonus et les réductions de dégâts.
- Un monstre vaincu par le saignement rapporte ses points à l'adversaire, comme tout monstre vaincu.
- Le saignement frappe avant les passives de début de phase : une Aura apaisante le retire, mais après qu'il a frappé une fois.

**Inciblable**
- Un monstre inciblable ne peut pas être choisi comme cible par les compétences adverses, même s'il a Provocation.
- Les compétences qui ne choisissent pas de cible (Grand balayage, Marée des abysses) le touchent quand même.

**Effets sur ce monstre ou sur les alliés**
- Certaines compétences ont des effets qui ne touchent pas leur cible : « ce monstre » (Frappe de l'ombre, Défi de pierre) ou « tous les alliés sur le terrain » (Souffle de vie, Faveur céleste).
- Ces effets s'appliquent après les dégâts sur la cible, dans l'ordre de la fiche.

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
| **Effet** | Inflige 10 dégâts à chaque ennemi sur le terrain et lui applique Ralentissement |
| **Temps de recharge** | 3 |
| **Durée** | Instantané |

Ne choisit pas de cible : ignore la Provocation.

### Griffe

| Champ | Valeur |
|---|---|
| **Type** | Active |
| **Rôle visé** | Carry |
| **Cible** | N'importe quel monstre sur le terrain, allié ou ennemi |
| **Effet** | Inflige 30 dégâts |
| **Temps de recharge** | 0 |
| **Durée** | Instantané |

Si elle vise un ennemi, elle est soumise à la Provocation.

### Soin

| Champ | Valeur |
|---|---|
| **Type** | Active |
| **Rôle visé** | Support |
| **Cible** | N'importe quel monstre sur le terrain, allié ou ennemi |
| **Effet** | Rend 40 PV |
| **Temps de recharge** | 2 |
| **Durée** | Instantané |

Si elle vise un ennemi, elle est soumise à la Provocation.

### Bénédiction

| Champ | Valeur |
|---|---|
| **Type** | Active |
| **Rôle visé** | Support |
| **Cible** | Tous les alliés sur le terrain |
| **Effet** | Les dégâts infligés par chaque allié ciblé augmentent de 10 |
| **Temps de recharge** | 3 |
| **Durée** | 6 tours |

Seuls les alliés présents sur le terrain au moment de l'utilisation reçoivent le bonus.

### Morsure perforante

| Champ | Valeur |
|---|---|
| **Type** | Active |
| **Rôle visé** | Carry |
| **Cible** | N'importe quel monstre sur le terrain, allié ou ennemi |
| **Effet** | Inflige 20 dégâts. Ces dégâts ignorent les réductions de dégâts |
| **Temps de recharge** | 0 |
| **Durée** | Instantané |

Si elle vise un ennemi, elle est soumise à la Provocation. Les bonus de dégâts s'appliquent normalement.

### Exécution

| Champ | Valeur |
|---|---|
| **Type** | Active |
| **Rôle visé** | Carry |
| **Cible** | N'importe quel monstre sur le terrain, allié ou ennemi |
| **Effet** | Si la cible a 80 PV ou moins, elle est vaincue. Sinon, inflige 10 dégâts |
| **Temps de recharge** | 4 |
| **Durée** | Instantané |

Si elle vise un ennemi, elle est soumise à la Provocation. Une cible vaincue rapporte normalement 1 point à l'adversaire de son camp.

### Chasseur de trophées

| Champ | Valeur |
|---|---|
| **Type** | Passive |
| **Rôle visé** | Carry |
| **Cible** | Un emplacement adverse, tiré au hasard |
| **Effet** | Un monstre adverse vaincu sur cet emplacement rapporte 1 point de plus |
| **Durée** | Tout le combat |
| **Déclencheur** | Au début du combat |

- Le tirage a lieu avant la mise en place, et l'emplacement marqué est visible des deux camps.
- Le point bonus s'applique quel que soit le monstre qui vainc la cible, même si le Chasseur de trophées est en main ou a été vaincu.
- Le point bonus compte aussi pour un monstre vaincu par la fatigue ou le saignement.

### Bouclier

| Champ | Valeur |
|---|---|
| **Type** | Active |
| **Rôle visé** | Support |
| **Cible** | N'importe quel monstre sur le terrain, allié ou ennemi |
| **Effet** | Les dégâts subis par la cible sont réduits de 20 |
| **Temps de recharge** | 3 |
| **Durée** | 2 tours |

Si elle vise un ennemi, elle est soumise à la Provocation.

### Aura apaisante

| Champ | Valeur |
|---|---|
| **Type** | Passive |
| **Rôle visé** | Support |
| **Cible** | Tous les alliés sur le terrain |
| **Effet** | Rend 10 PV à chaque allié sur le terrain, y compris ce monstre |
| **Durée** | Instantané, à chaque déclenchement |
| **Déclencheur** | Au début de chaque phase de ce monstre |

### Repli

| Champ | Valeur |
|---|---|
| **Type** | Active |
| **Rôle visé** | Support |
| **Cible** | Un autre allié sur le terrain |
| **Effet** | Renvoie la cible dans la main. Elle garde ses PV actuels et perd tous ses effets temporaires (bonus, réductions, ralentissement) |
| **Temps de recharge** | 4 |
| **Durée** | Instantané |

Un monstre renvoyé en main n'est pas vaincu : aucun point n'est marqué. S'il est reposé plus tard, ses recharges repartent au maximum, comme pour toute entrée sur le terrain.

### Provocation

| Champ | Valeur |
|---|---|
| **Type** | Passive |
| **Rôle visé** | Tank |
| **Cible** | Ce monstre |
| **Effet** | Ce monstre a Provocation (voir [combat.md](combat.md)) |
| **Durée** | Permanente |
| **Déclencheur** | Toujours |

### Carapace

| Champ | Valeur |
|---|---|
| **Type** | Passive |
| **Rôle visé** | Tank |
| **Cible** | Ce monstre |
| **Effet** | Les dégâts subis par ce monstre sont réduits de 10, sans descendre sous 0 |
| **Durée** | Permanente |
| **Déclencheur** | Quand ce monstre subit des dégâts |

### Coup de bouclier

| Champ | Valeur |
|---|---|
| **Type** | Active |
| **Rôle visé** | Tank |
| **Cible** | N'importe quel monstre sur le terrain, allié ou ennemi |
| **Effet** | Inflige 10 dégâts et applique Ralentissement |
| **Temps de recharge** | 3 |
| **Durée** | Instantané |

Si elle vise un ennemi, elle est soumise à la Provocation.

## Attaques des Essences

Les attaques de base des 7 Essences de départ (voir [monstres.md](monstres.md)). Elles ne visent que les ennemis.

### Entaille sanglante (Essence du sang)

| Champ | Valeur |
|---|---|
| **Type** | Active |
| **Rôle visé** | Carry |
| **Cible** | Un ennemi sur le terrain |
| **Effet** | Inflige 10 dégâts et applique 3 saignements |
| **Temps de recharge** | 2 |
| **Durée** | Les saignements durent jusqu'à un soin |

Soumise à la Provocation.

### Frappe de l'ombre (Essence d'ombre)

| Champ | Valeur |
|---|---|
| **Type** | Active |
| **Rôle visé** | Carry |
| **Cible** | Un ennemi sur le terrain |
| **Effet** | Inflige 40 dégâts. Ce monstre devient inciblable |
| **Temps de recharge** | 1 |
| **Durée** | Inciblable pendant 1 tour : jusqu'à la fin de la prochaine phase adverse |

Soumise à la Provocation. Avec la recharge de 1, elle frappe à chaque phase à partir de sa deuxième sur le terrain : elle reste alors inciblable pendant toutes les phases adverses, sauf si elle est ralentie ou renvoyée en main.

### Croissance sauvage (Essence de nature)

| Champ | Valeur |
|---|---|
| **Type** | Active |
| **Rôle visé** | Carry |
| **Cible** | Un ennemi sur le terrain |
| **Effet** | Inflige 20 dégâts. Ensuite, les dégâts de cette compétence augmentent de 5, et ce monstre gagne 10 PV (PV de départ compris) |
| **Temps de recharge** | 0 |
| **Durée** | Les gains durent tout le combat, même après un retour en main |

Soumise à la Provocation. Les coups infligent 20, puis 25, puis 30 dégâts, etc. Après 10 attaques, l'Essence a 280 PV de départ : une carry qui devient de plus en plus robuste.

### Marée des abysses (Essence des profondeurs)

| Champ | Valeur |
|---|---|
| **Type** | Active |
| **Rôle visé** | Support |
| **Cible** | Tous les ennemis sur le terrain |
| **Effet** | Applique Ralentissement à chaque ennemi sur le terrain, sans dégâts |
| **Temps de recharge** | 3 |
| **Durée** | Instantané |

Ne choisit pas de cible : ignore la Provocation et touche les monstres inciblables.

### Souffle de vie (Essence de vie)

| Champ | Valeur |
|---|---|
| **Type** | Active |
| **Rôle visé** | Support |
| **Cible** | Un ennemi sur le terrain |
| **Effet** | Inflige 15 dégâts. L'allié le plus blessé sur le terrain (y compris ce monstre) récupère autant de PV que la cible en a perdu |
| **Temps de recharge** | 0 |
| **Durée** | Instantané |

Soumise à la Provocation. Les réductions de dégâts de la cible réduisent aussi le vol de vie. C'est un soin : un allié qui saigne perd ses saignements au lieu d'être soigné.

### Faveur céleste (Essence des cieux)

| Champ | Valeur |
|---|---|
| **Type** | Active |
| **Rôle visé** | Support |
| **Cible** | Un ennemi sur le terrain |
| **Effet** | Inflige 10 dégâts. Les dégâts infligés par tous les alliés sur le terrain augmentent de 10 |
| **Temps de recharge** | 2 |
| **Durée** | Bonus pendant 3 tours |

Soumise à la Provocation. Avec la recharge de 2 et une durée de 3 tours, deux bonus se chevauchent un tour sur deux : les alliés ont alors +20 dégâts.

### Défi de pierre (Essence de pierre)

| Champ | Valeur |
|---|---|
| **Type** | Active |
| **Rôle visé** | Tank |
| **Cible** | Un ennemi sur le terrain |
| **Effet** | Inflige 10 dégâts. Ce monstre a Provocation |
| **Temps de recharge** | 0 |
| **Durée** | Provocation pendant 1 tour : jusqu'à la fin de la prochaine phase adverse |

Soumise à la Provocation. Ralentie ou renvoyée en main, l'Essence ne peut pas renouveler son Défi et perd sa Provocation.

## Compétences de Thrâk le Perpétuel

Le semi-boss des Plaines et ses clans (voir [combats-scenario.md](combats-scenario.md), combat 3).

### Esprit perpétuel

| Champ | Valeur |
|---|---|
| **Type** | Passive |
| **Rôle visé** | Tank |
| **Cible** | Ce monstre |
| **Effet** | Ce monstre est le chef des clans : il reçoit les bonus d'allégeance de ses alliés sur le terrain. Chaque fois que l'adversaire marque un point, il récupère tous ses PV et perd ses saignements |
| **Durée** | Permanente |
| **Déclencheur** | Toujours (allégeances) ; quand l'adversaire marque un point (régénération) |

- La régénération n'a lieu que si ce monstre est sur le terrain. En main, il ne se régénère pas.
- Ce n'est pas un soin : la règle du saignement ne s'applique pas, il récupère ses PV ET perd ses saignements.
- Elle se déclenche pour tout point marqué, y compris par la fatigue, mais pas quand ce monstre est lui-même vaincu.
- **Compétence spéciale :** le joueur peut l'obtenir en dépeçant Thrâk, mais un monstre ne porte qu'une compétence spéciale (voir [progression.md](progression.md), Dépeçage). Chez le joueur, elle marche de la même façon : son monstre se régénère quand l'adversaire marque.

### Allégeance du Guerrier, du Paladin, de l'Archer

| Champ | Valeur |
|---|---|
| **Type** | Passive |
| **Rôle visé** | Tank (Guerrier), Support (Paladin), Carry (Archer) |
| **Cible** | Le chef des clans allié (un monstre avec Esprit perpétuel) |
| **Effet** | Guerrier : le chef inflige 10 dégâts de plus. Paladin : le chef subit 10 dégâts de moins. Archer : les compétences du chef ignorent la Provocation |
| **Durée** | Tant que ce monstre et le chef sont sur le terrain |
| **Déclencheur** | Toujours |

- Les allégeances de plusieurs alliés se cumulent.
- Elles disparaissent dès que le monstre qui les donne quitte le terrain (vaincu ou renvoyé en main).
