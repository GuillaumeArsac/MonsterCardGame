# Combat

Ce fichier ne contient que les règles déjà décidées. Il sera complété au fur et à mesure.

## Principe

Chaque carte est un **monstre unique** avec ses propres capacités.

## Les cartes

- Chaque joueur a **5 monstres**.
- Un monstre est soit **en main**, soit **sur le terrain**.
- Il n'y a ni deck, ni pioche, ni cimetière.
- Les 5 monstres sont **en main dès le début** du combat.

## Le terrain

- Chaque joueur a **3 places**, appelées emplacements : gauche, centre et droite.
- On ne peut poser un monstre que sur un emplacement libre, et le joueur choisit lequel.

## Déroulement

**Mise en place**
- Les deux joueurs choisissent **en secret** les monstres qu'ils posent, puis les révèlent en même temps.

**Tours**
- Un **tour** se compose de la phase du joueur, puis de la phase de l'adversaire.
- Un tour est donc terminé quand toutes les créatures en jeu des deux camps ont agi.
- Pendant sa phase, un joueur :
  1. pose autant de monstres qu'il veut, dans la limite des places libres ;
  2. fait agir **chacun** de ses monstres sur le terrain, y compris ceux qui viennent d'être posés, s'ils ont une compétence active disponible (voir les temps de recharge dans [competences.md](competences.md)).

## Les monstres sur le terrain

- Pendant sa phase, un monstre peut utiliser **toutes** ses compétences actives disponibles, c'est-à-dire celles qui ne sont pas en recharge.
- Chaque compétence ne peut être utilisée qu'**une fois par phase**, même si son temps de recharge est 0.
- Si un monstre possède plusieurs exemplaires d'une même compétence active, chaque exemplaire a son propre compteur et peut être utilisé une fois par phase. Par exemple, un monstre avec trois Griffe peut utiliser Griffe trois fois.
- Ses compétences passives sont toujours actives.
- Un monstre ne peut revenir du terrain dans la main que grâce à une compétence qui le permet.

## La carte monstre

Une carte monstre affiche uniquement :
- son **nom** et son **illustration** ;
- ses **PV** ;
- ses **compétences**.

**Compétences**
- Un monstre a **jusqu'à 3 compétences**, actives et passives comprises (par exemple 1 active et 2 passives).
- Un monstre peut n'avoir que des compétences passives. Il n'agit alors pas pendant sa phase.

**Ciblage**
- Le joueur **choisit la cible** des compétences de ses monstres.
- Une compétence peut viser n'importe quel monstre **sur le terrain**, allié ou ennemi, selon ce qu'elle indique.
- Un monstre en main ne peut jamais être ciblé.

**Provocation**
- Tant qu'un monstre avec Provocation est sur le terrain, les compétences adverses qui visent un ennemi doivent le cibler.
- Si plusieurs monstres ont Provocation, l'adversaire choisit lequel cibler parmi eux.

## Monstre vaincu et victoire

- Un monstre vaincu est **retiré du combat**.
- Chaque monstre vaincu rapporte **1 point** à l'adversaire. C'est aussi vrai pour un monstre vaincu par la fatigue ou le saignement.
- Le premier joueur à **3 points** gagne.
- Un camp qui n'a plus aucun monstre, ni en main ni sur le terrain, **perd**, quel que soit le score.

## Fatigue

- Après le tour 15, tous les monstres sur le terrain, des deux camps, subissent des dégâts de **fatigue** au début de chaque tour.
- Ces dégâts augmentent de 10 à chaque tour : 10 dégâts au tour 16, 20 au tour 17, 30 au tour 18, etc.
- La fatigue garantit qu'un combat finit toujours, sans limiter les stratégies défensives avant le tour 15.
- Un monstre vaincu par la fatigue **rapporte ses points**, comme tout monstre vaincu.
- Si les deux camps atteignent 3 points en même temps, **le joueur perd** et l'adversaire gagne.
- Si les deux camps perdent leur dernier monstre en même temps sans qu'aucun n'ait atteint 3 points, **le joueur perd** et l'adversaire gagne.
  Le jeu étant uniquement en PvE, cela empêche le joueur de gagner en se soignant en boucle et en laissant la fatigue faire le travail.

## Pourquoi garder des monstres en main

Il n'y a pas de règle spéciale pour cela. Deux risques suffisent à limiter l'envie de tout poser :
- un monstre en main ne peut pas être vaincu, alors qu'un monstre sur le terrain y est exposé ;
- garder des monstres en main permet de s'adapter aux choix de l'adversaire.
