# Progression

Ce fichier contient les règles de progression du joueur entre les combats. Il ne contient que les règles déjà décidées.
Les règles du combat sont dans [combat.md](combat.md), les compétences dans [competences.md](competences.md) et les monstres dans [monstres.md](monstres.md).

## Principe

Le jeu est une aventure solo, avec un scénario écrit.
Le joueur commence avec quelques monstres simples, et les renforce en **dépeçant** les monstres adverses après chaque victoire : ce qui l'a battu devient sa force.

Inspiration : les jeux de cartes Yu-Gi-Oh sur GBA et DS.

## Collection

- Le joueur commence avec **7 Essences** : des esprits fabriqués à la Forge, chacun autour d'un thème (voir [monstres.md](monstres.md)).
- Chaque Essence a **une seule compétence** au départ : son attaque de base.
- Une Essence prend peu à peu la forme des monstres dont elle absorbe les compétences et les boosts.
- Le seul moyen d'obtenir de nouveaux monstres est de réussir des **quêtes secondaires**.
  - Ces monstres ont une **compétence unique**, qui ne peut pas être remplacée.
- En fin d'aventure, le joueur possède **8 à 12 monstres**, selon le nombre de quêtes secondaires réussies.

## Dépeçage

Après chaque **victoire** :
1. Un monstre est **tiré au hasard** parmi tous les monstres de l'équipe adverse, qu'il ait été vaincu ou non. Contre un **boss ou un semi-boss**, c'est toujours son chef qui est dépecé (par exemple Thrâk le Perpétuel).
2. Le joueur choisit **une** récompense :
   - **une compétence** de ce monstre, au choix, ajoutée à l'un de ses monstres ;
   - **ou le boost de stats** de ce monstre, ajouté à l'un de ses monstres.

**Compétences**
- Un monstre a au maximum 3 compétences.
- Si le monstre qui reçoit la compétence en a déjà 3, le joueur choisit l'une d'elles à remplacer. La compétence remplacée est perdue.
- Une compétence unique, obtenue par une quête secondaire, ne peut jamais être remplacée.
- Un monstre peut recevoir une compétence qu'il possède déjà. Les deux exemplaires se cumulent selon la règle de cumul.
- **Compétences spéciales :** les compétences propres aux boss et semi-boss (Esprit perpétuel, Ponte, Essaim) sont spéciales. Un monstre n'en porte qu'**une** : s'il en a déjà une, la nouvelle ne peut que la remplacer, même s'il a un emplacement libre. Elles s'affichent « Active spéciale » ou « Passive spéciale ».

**Boost de stats**
- Chaque monstre adverse a un **boost de stats** défini à l'avance, indépendant de ses propres statistiques (par exemple : +40 PV).
- Un monstre du joueur ne peut porter qu'**un seul** boost de stats. En ajouter un nouveau remplace le précédent.

## La Forge (à concevoir)

- Les Essences sont fabriquées à la Forge.
- Le joueur peut y retourner pour **redonner à une Essence son attaque de base**, contre des ressources. Les ressources restent à définir.
- Après un palier de l'aventure (par exemple la défaite du boss des Plaines), la Forge peut aussi donner **une passive** à une Essence.

## Combats

- Les combats du **scénario** sont uniques.
- Chaque zone propose aussi des **combats libres**, rejouables, pour s'entraîner et progresser.

## Principe de conception de l'aventure

- Le joueur accumule des outils au fil de l'aventure, y compris des combinaisons très fortes (par exemple plusieurs exemplaires d'une même compétence active). C'est voulu.
- En contrepartie, les monstres adverses deviennent plus puissants et plus dangereux à mesure que l'aventure avance.
- La difficulté se règle donc par la conception des adversaires de chaque zone, pas en limitant ce que le joueur peut construire.

## Échelle des boosts de stats

Les boosts ne concernent que les PV pour l'instant.

| Adversaire | Boost |
|---|---|
| Combat d'introduction | +20 PV |
| Monstre normal de zone | +30 à +40 PV |
| Monstre d'élite | +50 à +60 PV |
| Boss de zone | +80 PV |
