# Plan d'implémentation

Ce fichier suit l'implémentation dans Unity de la nouvelle conception du jeu.
La conception elle-même est décrite dans :
- [combat.md](combat.md) : les règles du combat ;
- [competences.md](competences.md) : les règles communes et le catalogue des compétences ;
- [monstres.md](monstres.md) : les valeurs de référence, les équipes de test et la collection de départ ;
- [progression.md](progression.md) : la collection, le dépeçage et les boosts ;
- [combats-scenario.md](combats-scenario.md) : les combats du scénario.

## Contexte

- **Moteur :** Unity 6 (6000.3), C#.
- **Branche de travail :** `ccr-1ea092c4-jithqy`.
- **Sauvegarde de l'ancienne version :** la branche `sauvegarde/avant-refonte` contient l'ancien système complet (deck, mana, Forge, matériaux), avant tout nettoyage. Elle sert de référence pour récupérer du code utile, par exemple les vues de cartes (`CardView2D`, `PlayerHandController`) ou les animations DOTween.
- **Simulateur :** [tools/simulation](../tools/simulation/README.md), en Python. Il sert de référence pour le moteur C#.

## Décisions techniques

- **Le moteur de combat est en C# pur**, sans dépendance à Unity (pas de MonoBehaviour). Il est donc testable avec les tests EditMode, sans scène. L'interface écoute ses événements.
- **Chaque règle a son test EditMode**, en reprenant les exemples de `combat.md` et `competences.md`.
- **Les données sont des ScriptableObjects** : compétences, monstres et combats. Les effets reprennent le principe de l'ancien `CardEffect` : une sous-classe par brique d'effet.
- **L'interface de combat utilise des GameObjects avec un Canvas** (et non UI Toolkit), dans la continuité du dernier choix fait sur le projet.
- **Les assets et la scène de combat sont générés par des scripts d'éditeur** (menu MonsterCardGame), pour éviter le travail manuel et pouvoir les régénérer.
- **Les ajustements visuels** (placement, tailles, animations) se font de préférence avec Claude Code en local et MCP Unity (`com.gamelovers.mcp-unity`, déjà installé), pour voir le résultat dans l'éditeur.

## Étapes

### 1. Nettoyage — fait

- [x] Supprimer les cartes, le combat, les mots-clés, le deck, la Forge, l'inventaire de matériaux, leurs interfaces et leurs tests.
- [x] Supprimer les assets, prefabs et scènes associés (Combat, DeckBuilder, Forge).
- [x] Adapter le monde et la sauvegarde : ils ne suivent pour l'instant que les combats gagnés, identifiés par leur nom.
- [x] `GameRules` contient les constantes des nouvelles règles.
- [x] **Vérifié dans Unity :** aucune erreur de compilation, la scène Boot enchaîne sur la carte du monde, qui affiche les Plaines.
  La scène Boot chargeait encore DeckBuilder, supprimée : elle charge maintenant directement WorldMap. MainMenu ne contient qu'une caméra et n'est pas utilisée pour l'instant.

### 2. Modèle de données — fait

- [x] `SkillData` : nom, type (active ou passive), rôle visé, cible, temps de recharge, effets. Les passives ont un déclencheur : toujours, début de phase du monstre, début du combat.
- [x] Effets : dégâts (option « ignore les réductions »), soin, bonus ou réduction de dégâts avec durée, ralentissement, exécution avec seuil, retour en main, Provocation, emplacement marqué (Chasseur de trophées).
  Ce sont des classes C# pures (une par brique, dans `Gameplay/Skills/Effects`), rangées dans `SkillData` avec `[SerializeReference]` : pas d'asset par effet, et le moteur peut les lire sans dépendre de Unity. L'inspecteur de `SkillData` a un bouton « Ajouter un effet ».
- [x] `MonsterCardData` : nom, illustration, PV, jusqu'à 3 compétences, boost de stats. Elle a aussi un rôle (pour l'IA, non affiché) et une compétence unique optionnelle (monstres de quête).
- [x] `EncounterData` : 5 monstres adverses, type (scénario ou libre), identifiant unique.
- [x] `OwnedMonster`, le monstre possédé par le joueur : carte d'origine, compétences actuelles, boost, compétence unique verrouillée (monstres de quête). Il applique les règles du dépeçage : 3 compétences au maximum, remplacement, un seul boost.
- [x] Tests EditMode : règles d'`OwnedMonster`, et le catalogue des docs décrit avec ce modèle (`Tests/EditMode/TestCatalog.cs`, réutilisable pour les tests du moteur).

### 3. Moteur de combat (C# pur) — fait, sauf la comparaison

Le moteur (`Gameplay/Combat/CombatEngine.cs`) se pilote par commandes : `Setup`, `Place`, `UseSkill`, `EndPhase`. Le joueur et l'IA passent par les mêmes commandes, et `CanPlace`, `CanUseSkill` et `GetValidTargets` disent ce qui est permis. Il n'utilise aucune API Unity : le hasard vient d'un `System.Random` qu'on lui donne.

- [x] Main, 3 emplacements par camp, pose secrète simultanée au départ.
- [x] Tours : phase du joueur, puis phase de l'adversaire. Pose libre, puis chaque monstre utilise toutes ses compétences disponibles, chacune une fois par phase (chaque exemplaire d'un doublon compte).
- [x] Compteurs de recharge : au maximum à l'entrée sur le terrain, baisse de 1 à la fin de la phase du camp, ralentissement.
- [x] Durées des effets : jusqu'à la fin de la X-ième phase de l'autre camp.
- [x] Provocation, calcul des dégâts (bonus puis réductions), soin plafonné aux PV de départ.
- [x] Points, emplacements marqués, fatigue à partir du tour 16, conditions de victoire et de défaite.
- [x] Événements pour l'interface : pose, compétence utilisée, dégâts, soin, ralentissement, effet appliqué, retour en main, monstre vaincu, début et fin de phase, fin de combat.
- [x] Tests EditMode pour chaque règle (`Tests/EditMode/Combat`).
- [ ] Comparaison avec le simulateur sur les équipes A et B. Elle demande l'IA : elle se fera avec l'étape 4, en comparant aux résultats notés dans `monstres.md` et `combats-scenario.md`.

Points que les docs ne précisent pas, réglés comme dans le simulateur :
- la fatigue ignore les réductions de dégâts (Carapace, Bouclier) ;
- quand Exécution ne vainc pas la cible, son dégât passe par le calcul normal (bonus, puis réductions) ;
- le compteur d'une compétence prend son temps de recharge avant ses effets : un monstre qui se ralentit lui-même ralentit aussi la compétence qu'il vient d'utiliser.

### 4. IA adverse

- [ ] Porter l'IA du simulateur : choix de la pose, puis note de chaque action possible.

### 5. Interface de combat

- [ ] Main de 5 cartes, 3 emplacements de chaque côté.
- [ ] Carte : nom, illustration, PV, compétences avec leurs compteurs.
- [ ] Choix de la cible au clic.
- [ ] Affichage des points, du tour, de la fatigue et de l'emplacement marqué.
- [ ] **Jalon 2 :** un combat A contre B jouable à la souris.

### 6. Progression

- [ ] Collection de départ (7 monstres).
- [ ] Écran de composition d'équipe (5 monstres parmi la collection).
- [ ] Écran de dépeçage : tirage du monstre, choix d'une compétence ou du boost, choix du monstre qui la reçoit, remplacement si besoin.
- [ ] Sauvegarde des monstres possédés.
- [ ] **Jalon 3 :** collection, équipe, dépeçage et sauvegarde fonctionnels.

### 7. Contenu

- [ ] Script d'éditeur qui génère les assets depuis les docs : les 13 compétences, les 7 monstres de départ, les équipes de test A et B, la patrouille centaure.

### 8. Branchement au monde

- [ ] `ZoneData` : combats du scénario (uniques) et combats libres (rejouables).
- [ ] Lancer la patrouille centaure depuis les Plaines.
- [ ] Réactiver le verrouillage des zones (les Ruines) à partir des boss vaincus.
- [ ] **Jalon 4 :** la patrouille centaure jouable depuis la carte des Plaines.

## Points de conception encore ouverts

- **La Forge dans le récit :** la fiche de la Cité du Foyer (`docs/scenario/zones/00-cite-du-foyer.md`) parle encore d'améliorer la Forge aux niveaux 2 et 3, une mécanique de l'ancienne version. Le dépeçage la remplace. Il faut décider ce que devient la Forge : simple élément narratif, ou lieu du dépeçage.
- **La patrouille centaure** (`combats-scenario.md`) attend une validation. Une mauvaise composition d'équipe y est très punie : il faudra peut-être un dialogue qui conseille l'équipe avant ce premier combat.
- **L'équilibre entre les équipes de test A et B** a bougé depuis la règle « un monstre utilise toutes ses compétences disponibles » : voir les résultats à jour dans `monstres.md`.
