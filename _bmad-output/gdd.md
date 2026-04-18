---
stepsCompleted: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14]
inputDocuments:
  - '_bmad-output/game-brief.md'
  - '_bmad-output/brainstorming-session-2026-04-04.md'
documentCounts:
  briefs: 1
  research: 0
  brainstorming: 1
  projectDocs: 0
workflowType: 'gdd'
lastStep: 0
project_name: 'MonsterCardGame'
user_name: 'Guillaume'
date: '2026-04-05'
game_type: 'card-game'
game_name: 'Monster Card Game'
---

# Monster Card Game - Game Design Document

**Author:** Guillaume
**Game Type:** Card Game (Adventure)
**Target Platform(s):** PC (Steam)

---

## Executive Summary

### Core Concept

Monster Card Game est une aventure de construction de deck dans laquelle les cartes représentent l'histoire que le joueur a vécue. Le joueur explore un monde Fantasy peuplé de monstres gigantesques, les affronte en duel de cartes, récolte leurs ressources et forge ses propres cartes — dont les effets reflètent mécaniquement les comportements des monstres vaincus.

Chaque deck est unique, personnel, et raconte une aventure différente. À la fin de l'aventure, le joueur se souvient de chaque carte qu'il a créée et pourquoi.

### Target Audience

**Principale :** Joueurs de jeux de cartes expérimentés (Hearthstone, Slay the Spire, Magic: The Gathering) cherchant une expérience narrative personnalisée, en sessions de 1-2h.

**Secondaire :** Joueurs RPG attirés par la personnalisation et l'histoire ; joueurs occasionnels de jeux vidéo habitués aux cartes physiques.

### Unique Selling Points (USPs)

1. **Cartes forgées, pas trouvées** — Chaque carte est créée par le joueur depuis les ressources des monstres vaincus.
2. **Mémoire mécanique** — Les effets des cartes reflètent le comportement des monstres dont elles sont issues.
3. **Aventure one-shot non-punitive** — Une grande aventure complète sans roguelike ni permadeath.
4. **Fierté de fin d'aventure** — Le deck final est l'œuvre du joueur.

### Game Type

**Type :** Card Game
**Framework :** Ce GDD utilise le template card-game avec sections spécifiques pour : types de cartes, construction de deck, système de ressources, structure des tours, progression de collection, modes de jeu.

---

## Target Platform(s)

### Primary Platform

**PC (Steam)** — Plateforme principale de lancement.

### Platform Considerations

- **Moteur :** Unity 6.3 LTS (6000.3.12f1)
- **Performance :** 60 fps stable sur PC gaming standard
- **Sauvegardes :** Locales uniquement (pas de cloud save)
- **Localisation :** Français et Anglais au minimum
- **Portage mobile :** Optionnel, non prioritaire pour le lancement

### Control Scheme

Souris + clavier. Le jeu de cartes se prête naturellement au pointer-clic :
- Clic gauche : sélectionner/jouer une carte
- Drag & drop : déplacer des cartes vers les zones de jeu
- Clic droit : inspecter une carte (zoom, description)
- Interface navigable entièrement à la souris

---

## Target Audience

### Demographics

Joueurs adultes, 18-35 ans, familiers avec les jeux de cartes numériques ou physiques.

### Gaming Experience

**Core gamers** — Joueurs réguliers qui consacrent plusieurs heures par semaine au jeu vidéo, avec une préférence pour les jeux de stratégie et de réflexion.

### Genre Familiarity

Connaissance préalable des jeux de cartes attendue (Hearthstone, Slay the Spire, Magic: The Gathering ou équivalents physiques). Les mécaniques de base (deck, ressources, effets de cartes) n'ont pas besoin d'être expliquées en détail.

### Session Length

**1-2 heures** en soirée. Le jeu est conçu pour des sessions modulables avec possibilité de pause et reprise — pas besoin de finir une zone en une seule session.

### Player Motivations

- Construire un deck unique qui leur ressemble
- Vivre une aventure narrative sans la frustration du roguelike
- Découvrir un nouvel univers Fantasy tout en jouant à un genre familier
- S'attacher à leurs cartes comme à des trophées personnels

---

## Goals and Context

### Project Goals

1. **Créer un jeu dont je suis fier** — Réaliser une expérience de jeu complète qui correspond à la vision créative de Guillaume, avec des mécaniques de craft et d'attachement aux cartes abouties.

2. **Progresser en développement Unity** — Utiliser ce projet comme terrain d'apprentissage des fonctionnalités avancées de Unity 6 : systèmes de données (ScriptableObjects), UI, localisation, sauvegarde.

3. **Valider le concept de mémoire mécanique** — Prouver que des cartes dont les effets reflètent le comportement des monstres créent un attachement émotionnel réel chez le joueur.

4. **Livrer une aventure complète et jouable** — Un jeu avec un début, un développement et une fin, jouable de bout en bout avec les 6 zones et la zone finale.

### Background and Rationale

Monster Card Game naît de la frustration avec les jeux de cartes existants : les roguelikes perdent tout à chaque run, les card games compétitifs ont des cartes communes où certaines dominent les autres, et aucun ne propose de forger ses cartes depuis ses victoires.

L'idée centrale — que ton deck raconte ton histoire — répond à un manque identifié : un jeu de cartes où chaque joueur repart avec quelque chose d'unique et de personnel, sans la brutalité d'un Elden Ring ni la perte totale d'un Slay the Spire.

---

## Core Gameplay

### Game Pillars

1. **Réflexion Stratégique**
   Chaque décision de combat et de craft doit avoir du poids. Aucun monstre ne peut être vaincu par une seule stratégie — il existe toujours plusieurs chemins vers la victoire.

2. **Attachement**
   Le joueur développe un lien émotionnel avec ses cartes. Chaque carte craftée porte la mémoire d'un monstre vaincu et d'un choix fait librement.

3. **Personnalisation**
   Le deck du joueur est son identité. Ses choix de craft définissent son style de jeu et aucun deck ne ressemble à un autre.

**Priorité des piliers :** Quand un conflit surgit, la Personnalisation prime sur la Réflexion Stratégique — un joueur doit toujours pouvoir jouer à sa façon, même si ce n'est pas la façon optimale.

### Core Gameplay Loop

**Boucle macro (l'aventure) :**
Explorer une zone → Découvrir des monstres → Affronter un monstre → Récolter des ressources → Forger des cartes → Renforcer son deck → Explorer plus loin → [Zone suivante / Zone finale]

**Boucle micro (le combat) :**
Piocher jusqu'à main pleine → Sacrifier des cartes pour des ressources → Jouer des actions / recruter des alliés → L'adversaire réagit (blocages/réactions) → Fin de tour → Tour adversaire → Recommencer

**Durée d'une boucle :**
- Micro (un combat) : 5-15 minutes selon le monstre
- Macro (une zone) : 1-3 sessions de 1-2h
- Aventure complète : plusieurs semaines de jeu en soirée

**Variation entre itérations :**
Chaque combat est différent car le deck du joueur évolue — les cartes craftées après chaque victoire modifient les stratégies disponibles. Chaque monstre a un deck fixe et unique, rendant chaque affrontement prévisible mais stratégiquement distinct.

### Win/Loss Conditions

#### Victory Conditions

- **Combat** : Les points de vie du monstre tombent à zéro
- **Zone** : Tous les monstres requis de la zone sont vaincus et l'histoire de la zone est complétée
- **Jeu** : Le boss final de la zone des Ruines est vaincu

#### Failure Conditions

- **Combat** : Les points de vie du joueur tombent à zéro
- **Jeu** : Pas de condition de perte définitive — pas de permadeath

#### Failure Recovery

En cas de défaite contre un monstre :
- Le monstre est **verrouillé temporairement** (ses ressources ne sont plus disponibles pendant un certain temps)
- Le joueur est renvoyé à la carte de la zone, libre d'aller affronter d'autres monstres
- Le joueur peut **revenir à une sauvegarde précédente** à tout moment
- La progression des autres zones et du deck est conservée

---

## Game Mechanics

### Primary Mechanics

**1. Explorer**
- Le joueur navigue sur la carte d'une zone qui se dévoile progressivement
- Interagit avec les PNJ via des boîtes de dialogue à embranchements
- Découvre de nouveaux monstres, lieux et histoires au fil de la progression
- *Sert : Personnalisation (choisir où aller et quoi affronter)*

**2. Affronter (Combat)**
- Duel de decks tour par tour contre un monstre avec son propre deck fixe
- Cible les PV du monstre (ou de ses alliés) via des cartes actions
- Se défend contre les attaques du monstre via réactions et blocages
- *Sert : Réflexion Stratégique*

**3. Récolter**
- Après une victoire, le monstre donne des matériaux communs et rares
- Les matériaux rares sont uniques au monstre et thématiques
- *Sert : Attachement (le butin rappelle le monstre vaincu)*

**4. Réfléchir & Choisir**
- Le joueur décide quelles cartes fabriquer avec ses ressources
- Priorise certaines mécaniques selon son style de jeu souhaité
- *Sert : Personnalisation + Réflexion Stratégique*

**5. Forger**
- Création de cartes à la Forge, lieu central à améliorer
- Le niveau de la Forge contrôle la puissance max des cartes craftables
- Possibilité de cartes hybrides (combinaison de matériaux de plusieurs monstres)
- *Sert : Personnalisation + Attachement*

**6. Jouer (Combat)**
- Sacrifier des cartes de sa main pour générer des ressources
- Jouer des actions (attaque ciblée), alliés (permanents ATK/DEF), équipements (usage unique, puissants)
- Réagir pendant le tour adverse avec blocages et réactions
- *Sert : Réflexion Stratégique*

### Mechanic Interactions

- **Récolter → Forger → Jouer** : Le cycle fondamental — les victoires alimentent le craft qui renforce le combat
- **Sacrifice vs. Jouer** : Tension centrale à chaque tour — chaque carte en main est à la fois un outil et un carburant potentiel
- **Alliés + Actions** : Les alliés persistent d'un tour à l'autre et peuvent être ciblés ; les actions résolues retournent en bas du deck (recyclage permanent)
- **Équipements** : Cartes puissantes disponibles dès le début du combat, usage unique — créent des "moments signature"
- **Forge + Zones** : Améliorer la Forge avec des ressources de zones spécifiques peut débloquer des recettes thématiques

### Mechanic Progression

- La Forge se débloque tôt et s'améliore avec des ressources spécifiques
- Les zones plus avancées donnent accès à des matériaux rares plus puissants
- Les monstres boss ont des mécaniques passives uniques (rares, fort impact)
- Le deck grandit progressivement — jamais trop grand, toujours personnel

### Controls and Input

#### Control Scheme (PC — Souris + Clavier)

| Action | Input |
|---|---|
| Sélectionner une carte | Clic gauche |
| Jouer une carte (zone cible) | Drag & drop vers la zone |
| Sacrifier une carte | Drag & drop vers zone ressources |
| Inspecter une carte | Clic droit (zoom + description) |
| Choisir une cible | Clic gauche sur la cible |
| Fin de tour | Bouton dédié (ou touche Entrée) |
| Naviguer la carte du monde | Clic gauche / défilement souris |
| Accéder à la Forge | Clic sur l'icône Forge |
| Ouvrir le deck / inventaire | Touche dédiée (ex : I ou Tab) |

#### Input Feel

Interface réactive et lisible — le jeu de cartes repose sur la clarté des choix. Chaque action doit donner un feedback visuel immédiat (animation de carte, highlight de zone cible valide, indication de coût en ressources).

#### Accessibility Controls

- Touches reconfigurables (rebinding clavier)
- Taille de texte ajustable pour les descriptions de cartes
- Localisation FR / EN dès le lancement
- (Futur) Options daltonisme pour la lisibilité des zones de plateau

---

## Card Game Specific Design

### Card Types and Effects

Monster Card Game utilise 5 types de cartes :

| Type | Description | Après résolution |
|---|---|---|
| **Action** | Attaque ciblée (allié adverse ou personnage) | Bas du deck |
| **Blocage** | Contre une action adverse (hors tour) | Bas du deck |
| **Allié** | Permanent ATK/DEF, actif après 1 tour d'attente | Cimetière si détruit |
| **Équipement** | Puissant, usage unique, zone dédiée dès le début du combat | Détruit après usage |
| **Réaction** | Effet hors tour, peut bloquer les actions Instantanées | Bas du deck |

**Système de Rareté :**

| Rareté | Origine | Copies max | Poids |
|---|---|---|---|
| **Commune** | Ressources de base de monstres communs | 3 | 0 |
| **Rare** | Ressources de base de boss + monstres standards | 3 | 1 |
| **Légendaire** | Matériaux rares thématiques des monstres | 1 | 2 |
| **Unique** | Matériaux ultra-rares de boss multi-zones | 1 | 2 |

**Mots-clés :**

| Mot-clé | Effet |
|---|---|
| **Vol** | L'allié ne peut pas être ciblé par une action, sauf si elle a Portée |
| **Portée** | Permet de cibler un allié adverse avec Vol |
| **Instantané** | L'action ne peut pas être bloquée par un Blocage (Réaction requise) |
| **Provocation** | Doit être ciblé en premier avant tout autre allié ou le joueur |
| **Éveillé** | L'allié peut attaquer le tour où il est joué |
| **Rituel (X)** | Nécessite d'envoyer X cartes de sa main au cimetière en plus du coût |
| **Invincible** | Ne peut pas être détruit par une action ou un allié (Équipement/Réaction requis) |
| **Rampant** | Peut être joué depuis le cimetière pour son coût de base |

### Deck Building

- **Taille du deck :** 40 cartes exactement
- **Copies maximum :** 3 exemplaires par carte (1 pour Légendaire et Unique)
- **Système de poids :** Poids maximum de 15 par deck
  - Commune = 0 | Rare = 1 | Légendaire = 2 | Unique = 2
- **Taille de main :** 6 cartes (pioche en début de tour jusqu'à 6)
- **Decks monstres :** Construits spécifiquement par monstre, ne suivent pas les règles de construction joueur (mais les respectent généralement)

### Mana / Resource System

- **Type unique :** Un seul type de ressource — le **Mana**
- **Génération :** Chaque carte sacrifiée génère le mana indiqué sur la carte (valeur définie lors du craft)
- **Coût :** Chaque carte a un coût en mana défini lors du craft
- **Persistance :** Le mana non utilisé en fin de tour est perdu
- **Aucune limite d'actions** par tour — le joueur est limité par le mana disponible et les cartes en main

### Turn Structure

**Tour du joueur actif :**
1. **Pioche** — jusqu'à 6 cartes en main
2. **Sacrifice (optionnel)** — envoyer des cartes en bas du deck, générer du mana
3. **Jeu de cartes** — jouer actions, alliés, équipements (pas de limite d'actions)
4. **Fin de tour** — mana non utilisé perdu

**Tour réactif (adversaire) :**
- Jouer des **Blocages** contre les actions non-Instantanées
- Jouer des **Réactions** contre tout type d'action

**Règles spéciales :**
- Les actions/blocages/réactions retournent en **bas du deck** après résolution
- Si le deck est vide et qu'il faut piocher → **défaite** (rare en pratique)
- Certains effets de cartes permettent de piocher pendant le tour (mécanique rare, liée à des monstres spécifiques)

### Card Collection and Progression

**La Forge — Système de Craft :**

| Niveau | Débloque | Coût d'amélioration |
|---|---|---|
| **Niveau 1** | Cartes Communes et Rares | Disponible dès le début |
| **Niveau 2** | Cartes Légendaires + nouvelles recettes Communes/Rares | 1 matériau rare de chaque zone + matériaux communs |
| **Niveau 3** | Cartes Uniques + nouvelles recettes Légendaires/Rares | 1 matériau rare de chaque zone + matériaux communs |

**Recettes de Craft :**
- Cachées par défaut
- Découvertes par **expérimentation empirique** (tester des combinaisons)
- Ou révélées par des **PNJ** au fil de l'histoire des zones
- Les recettes découvertes sont **conservées entre les parties**

### Game Modes

- **Mode unique :** Mode Histoire — l'aventure complète en une seule campagne
- **Sauvegarde :** Automatique après chaque combat + manuelle à tout instant
- **Nouvelle partie :** Repart de zéro (deck vide, Forge niveau 1), à l'exception des **recettes découvertes** qui sont conservées
- **Pas de multijoueur**, pas de classement en ligne, pas de modes annexes

---

## Progression and Balance

### Player Progression

La progression du joueur repose **exclusivement sur son deck** — il n'y a pas de stats de personnage qui évoluent. Les PV du joueur sont fixes du début à la fin de l'aventure.

#### Progression Types

- **Puissance (Deck)** : Le deck grandit au fil des victoires — plus de cartes, de meilleures synergies, des cartes plus rares craftées à la Forge
- **Puissance (Forge)** : 3 niveaux de Forge débloquent des cartes de plus en plus puissantes et de nouvelles recettes
- **Compétence** : Le joueur apprend les decks des monstres, anticipe leurs stratégies, optimise ses propres synergies
- **Narrative** : Les histoires des zones se déroulent, de nouveaux PNJ et monstres apparaissent au fil de la progression
- **Contenu** : Nouvelles zones explorées, PNJ cross-zones qui apparaissent lorsque le joueur a progressé dans plusieurs zones

#### Progression Pacing

- Chaque victoire donne des matériaux → craft immédiat possible
- La Forge monte en niveau après avoir collecté des matériaux rares de chaque zone → jalons naturels de progression
- La zone des Ruines (finale) se débloque après avoir avancé dans toutes les autres zones → objectif long terme visible

### Difficulty Curve

**Structure en dents de scie par zone :**
Chaque zone commence avec des monstres accessibles et monte en difficulté vers un ou plusieurs boss — puis la zone suivante repart d'un niveau intermédiaire avec de nouveaux défis mécaniques.

#### Challenge Scaling

- **Intra-zone** : Monstres de difficulté croissante au sein de chaque zone, culminant sur des boss avec mécaniques passives uniques
- **Inter-zones** : Pas d'ordre imposé — le joueur choisit librement quelle zone explorer selon son style de deck en construction
- **Zone finale (Ruines)** : Accessible uniquement après avoir progressé dans toutes les autres zones — boss puissants tirant parti de toutes les mécaniques rencontrées

#### Difficulty Options

Pas de sélection de difficulté explicite. Le jeu gère la difficulté via :
- **Liberté de zone** : Le joueur bloqué peut changer de zone pour affronter des monstres plus abordables et renforcer son deck
- **Monstres faibles disponibles** : Dans chaque zone, des monstres de début de zone restent disponibles pour accumuler des ressources
- **Pas de permadeath** : La défaite ne détruit pas la progression, elle verrouille temporairement un monstre et invite à explorer ailleurs
- **Sauvegarde libre** : Le joueur peut revenir à une sauvegarde précédente à tout moment

### Economy and Resources

Monster Card Game n'a pas d'économie monétaire traditionnelle. Le système de ressources est centré sur les **matériaux de craft**.

#### Resources

| Ressource | Obtention | Utilisation |
|---|---|---|
| **Matériaux communs** | Victoire contre tout monstre | Craft de cartes Communes/Rares, amélioration Forge |
| **Matériaux rares** | Victoire contre monstres spécifiques | Craft de cartes Légendaires/Uniques, amélioration Forge |
| **Mana (combat)** | Sacrifice de cartes pendant un combat | Jouer des cartes pendant le combat (perdu en fin de tour) |

#### Economy Flow

Victoire → Matériaux récoltés → Forge → Cartes craftées → Deck amélioré → Victoires plus faciles → Matériaux plus rares accessibles

**Équilibre anti-inflation :**
- Le deck est limité à 40 cartes et un poids max de 15
- Les matériaux rares sont limités (un par monstre vaincu)
- Les matériaux pour améliorer la Forge exigent au moins un rare de chaque zone — impossible de rusher le niveau 3

---

## Level Design Framework

### Structure Type

**Hub-Based avec sélection libre**

Le jeu adopte une structure de type Hub-Based : le joueur accède à une carte du monde depuis laquelle il choisit librement dans quelle zone progresser. Il n'y a pas d'ordre imposé entre les 5 zones principales — chaque zone est une histoire indépendante, complète en elle-même. La zone finale (Les Ruines Oubliées) est verrouillée jusqu'à ce que le joueur ait suffisamment progressé dans les autres zones.

### Types de Zones

Le jeu contient 6 zones thématiques, chacune avec sa propre ambiance, ses monstres uniques et sa mécanique dominante :

| Zone | Ambiance | Mécanique dominante |
|---|---|---|
| **Les Marais de la Putréfaction** | Brume, mort-vivants | Cimetière, réanimation, malus |
| **La Grande Forêt des Esprits** | Canopée, esprits animaux | Tokens, buffs, synergies alliés |
| **Les Montagnes Éternelles** | Pics enneigés, grottes | Défense, blocages, verrouillage |
| **L'Archipel des Abysses** | Îles et océan profond | Dualité colossal/rapide, soins |
| **Les Plaines Libres** | Horizon infini, cités | Polyvalence, decks hybrides |
| **Les Ruines Oubliées** *(finale)* | Cités effondrées | Ressources, risque/récompense |

Chaque zone contient :
- Plusieurs rencontres de monstres normaux (farm de matériaux)
- Un boss de zone avec mécanique passive unique
- Une histoire locale avec début, milieu et fin
- Des PNJ dont certains n'apparaissent qu'après progression dans d'autres zones

#### Intégration Tutoriel

Il n'y a pas de zone tutoriel dédiée. L'apprentissage se fait par le jeu lui-même :
- Premier combat guidé au lancement de la partie (mécaniques de base)
- Les Plaines Libres constituent la zone d'entrée recommandée (monstres polyvalents, mécanique la plus accessible, cartes hybrides)
- La Forge est introduite après le premier combat remporté

#### Zone Finale

Les Ruines Oubliées sont verrouillées au départ. Elles s'ouvrent lorsque le joueur a accompli un niveau de progression suffisant dans les 5 autres zones (boss vaincus ou histoires terminées — à préciser en production). Elles constituent le climax narratif et mécanique du jeu.

### Level Progression

**Modèle : Sélection libre + Déverrouillage narratif**

Le joueur choisit librement l'ordre dans lequel il explore les 5 zones principales. Les Ruines Oubliées se déverrouillent automatiquement une fois les conditions remplies dans les autres zones.

#### Système de Déverrouillage

- Les 5 zones principales sont accessibles dès le début
- Les Ruines Oubliées se déverrouillent par progression narrative/mécanique
- Certains PNJ secondaires dans une zone n'apparaissent qu'après des actions dans une autre zone (cross-zone storytelling)
- Les recettes de la Forge avancées (niveaux 2 et 3) nécessitent des matériaux rares provenant de zones spécifiques

#### Rejouabilité

Le joueur peut toujours revenir dans une zone déjà terminée pour :
- Affronter à nouveau des monstres et récolter des matériaux
- Explorer des alternatives de craft
- Compléter des rencontres PNJ débloquées ultérieurement

La sauvegarde automatique (après chaque combat) et manuelle permet de conserver la progression sans risque de perte. Il n'y a pas de permadeath.

### Principes de Level Design

- **Cohérence thème/mécanique** : chaque zone enseigne une mécanique via ses monstres — les vaincre, c'est comprendre leur pouvoir pour se l'approprier
- **Histoire complète par zone** : chaque zone a un début, un développement et une fin narrative — pas d'open-world fatigue
- **Liberté sans punition** : défaite = verrou temporaire sur le monstre, jamais de perte de progression
- **La carte comme mémoire** : la carte du monde se révèle progressivement et reflète le parcours unique du joueur

---

## Art and Audio Direction

### Art Style

**2D illustré, coloré et vivant**

Monster Card Game adopte un style 2D illustré avec une palette lumineuse et contrastée. Chaque zone possède sa propre identité visuelle — couleurs, ambiance lumineuse et tonalité propres qui la rendent instantanément reconnaissable.

#### Éléments visuels clés

- **Monstres** : Pièces maîtresses visuelles du jeu. Illustrations impressionnantes et majestueuses, conçues pour marquer le joueur et susciter l'attachement. Chaque monstre doit être mémorable.
- **Cartes** : Design orienté lisibilité avant tout — texte clair, mots-clés visibles, hiérarchie de l'information immédiatement lisible. Quelques cartes Uniques emblématiques bénéficient d'illustrations personnalisées distinctives.
- **UI** : Sobre et fonctionnelle. L'interface est au service du combat, jamais en compétition visuelle avec les cartes et les monstres.

#### Références visuelles

- **Ambiance générale** : Conte fantastique illustré — chaleureux, vivant, narratif
- **Lisibilité des cartes** : Hearthstone — clarté de l'information, hiérarchie visuelle
- **Monstres** : Monster Hunter — majesté, présence, caractère

#### Palette et lumière

Palette lumineuse et contrastée, variée par zone :
- Chaque zone a sa dominante colorimétrique propre (ex. Marais = verts troubles et violets, Montagnes = bleus glacés et gris acier)
- Contraste suffisant pour une lisibilité parfaite sur tous types d'écrans
- Les illustrations de monstres et de cartes tirent parti de cette identité par zone

#### Caméra et perspective

Vue 2D. Le plateau de combat est présenté en vue de dessus stylisée ou en face-à-face (à définir en production). La carte du monde est vue de dessus.

### Audio and Music

**Ambient atmosphérique, ancré dans chaque zone**

La direction audio cherche l'immersion et l'identité de chaque zone plutôt que l'énergie ou la performance.

#### Style musical

Un thème musical dédié par zone, reflétant son ambiance narrative et mécanique :
- Marais : sons organiques, cordes graves, atmosphère trouble
- Forêt : instruments acoustiques, nappes aériennes, calme vivant
- Montagnes : percussions lourdes, harmoniques froides
- Archipel : textures aquatiques, contraste calme/tension
- Plaines : thème ouvert et neutre, adaptable
- Ruines : ambient inquiet, dissonances subtiles

Les boss emblématiques ont leur propre thème — montée en intensité par rapport au thème de zone.

#### Design sonore

Approche pragmatique adaptée au développement solo :
- Assets store et outils génératifs en phase de développement (placeholders)
- Finalisation des effets sonores en fin de projet, avant release

#### Voix et dialogues

Aucun doublage. Tous les dialogues et textes narratifs sont en texte uniquement.

### Production Approach

| Élément | Approche |
|---|---|
| Illustrations monstres & cartes | Collaboration avec artiste (école d'art, passion Fantasy/jeux de cartes) |
| UI et éléments graphiques simples | Production interne |
| Musique et effets sonores | Assets store + outils génératifs (placeholders), finalisation en fin de projet |

### Aesthetic Goals

- **Réflexion Stratégique** : L'UI épurée et les cartes lisibles permettent au joueur de se concentrer sur les décisions sans friction visuelle
- **Attachement** : Les illustrations majestueuses de monstres et les cartes uniques créent un lien émotionnel fort avec le butin récolté
- **Personnalisation** : L'identité visuelle distincte par zone renforce la sensation que le deck du joueur raconte son voyage personnel

---

## Technical Specifications

### Performance Requirements

Monster Card Game cible une expérience fluide sur PC, priorité à la lisibilité et la réactivité de l'interface plutôt qu'aux effets visuels lourds.

#### Frame Rate Target

- **Cible principale :** 60 fps stables
- **Minimum acceptable :** 30 fps sur configurations modestes
- La nature du jeu (jeu de cartes au tour par tour) rend les performances moins critiques qu'un jeu d'action — la priorité est la fluidité de l'UI et des animations de cartes

#### Resolution Support

- **Résolution cible :** 1080p (priorité)
- **Support :** 1440p et 4K (scaling UI)
- Interface adaptative (UI scalable selon résolution)
- Support des formats d'écran 16:9 et 16:10 au minimum

#### Load Times

- Chargement initial < 10 secondes sur machine recommandée
- Transitions entre zones : instantanées ou avec écran de chargement court
- Pas de streaming d'assets nécessaire (jeu 2D, taille modeste)

### Platform-Specific Details

#### PC (Steam) — Cible principale

| Critère | Minimum | Recommandé |
|---|---|---|
| OS | Windows 10 64-bit | Windows 10/11 64-bit |
| RAM | 4 GB | 8 GB |
| GPU | Carte intégrée récente | Carte dédiée entry-level |
| Stockage | ~500 MB | ~1 GB |

**Fonctionnalités Steam :**
- Steam Cloud Saves (synchronisation des sauvegardes)
- Succès Steam (Achievements) — à définir en production
- Pas de DLC, pas de microtransactions, pas de mod support

**Input :** Souris + clavier (configuration principale), gamepad optionnel (à évaluer)

**Localisation :** Français et Anglais dès le lancement

**Sauvegarde :** Locale + Steam Cloud. Sauvegarde automatique après chaque combat, sauvegarde manuelle à tout moment.

### Asset Requirements

#### Art Assets

| Catégorie | Estimation | Notes |
|---|---|---|
| Illustrations de monstres | ~30–50 illustrations | Pièces maîtresses — production externe (artiste) |
| Illustrations de cartes | ~100–200 cartes | Majoritairement templates lisibles, quelques illustrations uniques |
| Sprites UI | À définir | Production interne |
| Backgrounds de zones | 6 zones + Forge | Production externe ou assets store |
| Animations de cartes | Simple (tween/shader) | Pas d'animation frame-by-frame complexe |

#### Audio Assets

| Catégorie | Estimation | Approche |
|---|---|---|
| Thèmes musicaux de zones | 6 pistes | Assets store / outils génératifs (placeholders) |
| Thèmes de boss | ~6–8 pistes | Assets store / outils génératifs (placeholders) |
| Effets sonores (UI, combat) | ~50–100 SFX | Assets store (placeholders), finalisation pré-release |

#### External Assets

- Assets store Unity pour SFX, musiques et potentiellement certains backgrounds
- Outils génératifs (ex. Suno, ElevenLabs audio) comme placeholders en développement
- Finalisation audio en fin de projet avant release

### Technical Constraints

- **Moteur :** Unity 6.3 LTS (6000.3.12f1) — C#
- **Développeur solo** — niveau intermédiaire Unity (forte expérience C#, expertise Unity avancée limitée)
- **Pas de réseau** — entièrement offline (hors Steam Cloud)
- **Pas de système procédural** — contenu entièrement designé à la main
- **Taille cible du build :** < 1 GB
- L'architecture détaillée (systèmes, patterns, structure du projet) sera définie dans le workflow d'Architecture Technique séparé

---

## Development Epics

### Epic Overview

| # | Epic | Scope | Dépendances | Jalon jouable |
|---|---|---|---|---|
| 1 | Fondations Techniques | Architecture Unity, modèle de données, structure de scènes | — | Non |
| 2 | Système de Combat (MVP) | Combat complet, plateau 6 zones, IA monstre, conditions victoire/défaite | Epic 1 | Oui — combat avec cartes hardcodées |
| 3 | Système de Craft & Forge | Matériaux, Forge niv. 1, craft Communes/Rares, deck builder | Epic 2 | Oui — construction de deck depuis ressources |
| 4 | Monde & Progression | Carte du monde, navigation, sauvegarde, Forge niv. 2–3, recettes | Epic 3 | Oui — aventure complète navigable |
| 5 | Zone Pilote : Les Plaines Libres | Zone complète : monstres, boss, histoire, PNJ, drops | Epic 4 | Oui — une zone de A à Z |
| 6 | Zones Thématiques (×4) | Marais, Forêt, Montagnes, Archipel — zones complètes | Epic 5 | Oui — monde principal complet |
| 7 | Zone Finale : Les Ruines Oubliées | Zone finale, boss ultra, déverrouillage conditionnel | Epic 6 | Oui — aventure terminée |
| 8 | Polish & Release | UI, audio, localisation FR/EN, Steam, QA | Epic 7 | Oui — release ready |

### Recommended Sequence

**1 → 2 → 3 → 4 → 5 → 6 → 7 → 8**

- Les épics 1–4 posent les fondations systémiques — elles doivent être solides avant d'injecter du contenu
- L'epic 5 (Zone Pilote) valide que tout le pipeline fonctionne de bout en bout sur une zone réelle
- Les épics 6–7 sont du contenu pur, réalisables en parallèle une fois le pipeline validé
- L'epic 8 est volontairement en dernier — le polish sur des systèmes instables est du travail perdu

### Vertical Slice

**Premier jalon jouable : fin de l'Epic 2**

Un combat de carte fonctionnel complet avec des cartes hardcodées — plateau 6 zones, mana, tour structuré, IA monstre scriptée, victoire et défaite. Aucun craft, aucun monde, mais la mécanique centrale est prouvée.

---

## Success Metrics

### Technical Metrics

Métriques techniques mesurables avant et après release.

#### Key Technical KPIs

| Métrique | Cible | Comment mesurer |
|---|---|---|
| Frame rate | 60 fps stables (config recommandée) | Profiler Unity + tests sur machines cibles |
| Frame rate minimum | ≥ 30 fps (config minimum) | Tests sur machine bas de gamme |
| Temps de chargement initial | < 10 secondes | Chronométrage manuel + profiler |
| Taille du build | < 1 GB | Pipeline de build |
| Bugs critiques à la release | 0 (crash, perte de sauvegarde) | QA playtest |
| Bugs majeurs à la release | < 5 (bloquants gameplay) | QA playtest |
| Stabilité sauvegarde | 100% — aucune perte de progression | Tests de sauvegarde/chargement |

### Gameplay Metrics

Métriques de design — comment savoir si l'expérience fonctionne comme prévu.

#### Key Gameplay KPIs

| Métrique | Cible | Comment mesurer |
|---|---|---|
| Complétion du premier combat | > 90% des joueurs terminent le tutoriel | Playtest observé |
| Temps jusqu'au premier craft | < 20 minutes (première session) | Playtest chronométré |
| Taux de progression zone pilote | > 70% des joueurs finissent les Plaines | Playtest + retours |
| Diversité des decks | Pas deux decks identiques entre playtesteurs | Comparaison manuelle |
| Taux de victoire boss (première zone) | 60–80% (accessible mais résistant) | Playtest |
| Complétion de l'aventure | Le joueur peut terminer le jeu en 1 sauvegarde | QA end-to-end |
| Sentiment post-victoire boss | "J'ai gagné grâce à mon deck" (pas à la chance) | Retours qualitatifs |

### Qualitative Success Criteria

**Réflexion Stratégique :**
- Les playtesteurs parlent spontanément de leurs choix de deck et de leurs synergies
- Les défaites génèrent "je vais adapter mon deck" plutôt que "le jeu est trop dur"

**Attachement :**
- Les joueurs mentionnent des cartes spécifiques avec affection ("ma carte préférée c'est...")
- Les illustrations de monstres sont commentées positivement
- Les joueurs ressentent de la fierté en montrant leur deck

**Personnalisation :**
- Aucun playtesteur n'a le même deck qu'un autre après la zone pilote
- Les joueurs expriment une identité de jeu ("moi je joue réanimation" / "je fais un deck vol")

**Objectifs personnels (développeur) :**
- Le projet est terminé et publié sur Steam
- Guillaume a progressé significativement sur Unity au fil du développement
- Guillaume est fier de montrer le jeu à son entourage

### Metric Review Cadence

- **Pendant le développement** : Playtest informel à chaque fin d'epic, retours qualitatifs notés
- **Pre-release** : QA playtest structuré sur Epic 8, correction des bugs critiques
- **Post-release** : Lecture des reviews Steam, retours communauté (si applicable)
- Pas d'analytics en temps réel (pas de réseau) — toutes les mesures sont manuelles ou via Steam

---

## Out of Scope

Les éléments suivants sont **explicitement hors scope** pour Monster Card Game v1.0 :

### Fonctionnalités exclues

- **Multijoueur** (PvP ou co-op) — jeu entièrement solo
- **Éditeur de niveau ou de cartes** — pas d'outils joueur
- **Mod support** — pas d'API ou de fichiers exposés
- **New Game+** — une seule aventure complète, pas de cycle
- **Modes de jeu alternatifs** (défi quotidien, roguelike, endless) — mode histoire uniquement
- **Classements en ligne** — pas de réseau
- **Microtransactions ou DLC** — jeu complet à l'achat

### Plateformes exclues (v1.0)

- Consoles (Switch, PlayStation, Xbox)
- Mobile (iOS / Android) — envisageable post-launch si pertinent
- VR / Web

### Polish exclu (v1.0)

- Doublage intégral — dialogues en texte uniquement
- Score orchestral intégral — approche assets store + génératif
- Accessibilité avancée (mode daltonisme, etc.) — déféré post-launch

### Deferred to Post-Launch

- Portage mobile (si la demande existe)
- Modes de jeu additionnels (défi, arène PvP)
- Extension de contenu (nouvelles zones, nouveaux monstres)
- Options d'accessibilité avancées

---

## Assumptions and Dependencies

### Key Assumptions

- Unity 6.3 LTS reste stable tout au long du développement
- Le développeur solo (Guillaume) dispose d'environ 5–6h/semaine de développement
- L'artiste (cousin) est disponible pour les illustrations de monstres et cartes clés
- Les assets store Unity couvrent les besoins audio et visuels de placeholder
- Steam approuve le jeu dans des délais raisonnables (2–4 semaines)
- Le scope v1.0 (8 epics) est réalisable dans le cadre du temps disponible

### External Dependencies

| Dépendance | Type | Risque |
|---|---|---|
| Artiste (illustrations monstres/cartes) | Collaborateur externe (bénévole) | Moyen — disponibilité non garantie |
| Unity 6.3 LTS | Moteur de jeu | Faible — LTS = stable |
| Assets store Unity | Contenu audio/visuel | Faible — large disponibilité |
| Steam (Steamworks SDK) | Distribution | Faible — processus bien documenté |
| Outils génératifs audio | Placeholders | Faible — remplaçables |

### Risk Factors

- **Disponibilité artiste** : Si le cousin n'est pas disponible, prévoir un budget pour des illustrations commandées ou utiliser des placeholders plus longtemps
- **Scope creep** : Chaque zone est une histoire complète — risque d'allongement. Traiter l'Epic 5 (Zone Pilote) comme signal d'alerte de scope
- **Expérience Unity limitée** : Certaines fonctionnalités avancées (animations, shaders, Steam SDK) peuvent nécessiter plus de temps que prévu — prévoir du buffer sur les épics techniques

---

## Document Information

**Document :** Monster Card Game — Game Design Document
**Version :** 1.0
**Créé le :** 2026-04-05
**Auteur :** Guillaume
**Status :** Complete

### Change Log

| Version | Date | Changements |
|---|---|---|
| 1.0 | 2026-04-05 | GDD initial complet |
