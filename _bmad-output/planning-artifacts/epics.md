---
stepsCompleted: [1, 2, 3]
inputDocuments:
  - '_bmad-output/gdd.md'
  - '_bmad-output/game-architecture.md'
  - '_bmad-output/epics.md'
---

# MonsterCardGame - Epic Breakdown

## Overview

This document provides the complete epic and story breakdown for MonsterCardGame, decomposing the requirements from the GDD and Architecture into implementable stories with acceptance criteria.

## Requirements Inventory

### Functional Requirements

FR1:  Le joueur peut affronter un monstre en duel de cartes sur un plateau à 6 zones (Main, Deck, Cimetière, Recrutement, Affrontement, Équipement)
FR2:  Le joueur peut jouer des cartes de type Action (attaque ciblée)
FR3:  Le joueur peut jouer des cartes de type Blocage (contre une Action adverse, hors tour)
FR4:  Le joueur peut jouer des cartes de type Allié (permanent ATK/DEF, actif après 1 tour)
FR5:  Le joueur peut jouer des cartes de type Équipement (usage unique, zone dédiée)
FR6:  Le joueur peut jouer des cartes de type Réaction (hors tour, contre Instantané)
FR7:  Le joueur peut sacrifier des cartes de sa main pour générer du mana
FR8:  Le mana non utilisé en fin de tour est perdu
FR9:  Le joueur peut piocher jusqu'à 6 cartes en début de tour
FR10: Les 8 mots-clés (Vol, Portée, Instantané, Provocation, Éveillé, Rituel(X), Invincible, Rampant) fonctionnent selon leurs définitions
FR11: L'IA monstre joue depuis un deck scripté avec comportement simple et lisible
FR12: Les boss possèdent une mécanique passive unique activée en combat
FR13: La partie se termine quand les PV d'un protagoniste atteignent 0
FR14: La défaite verrouille temporairement le monstre (non punitif)
FR15: Le joueur peut construire un deck de 40 cartes (max 3 copies, 1 pour Légo/Unique, poids max 15)
FR16: Le joueur récolte des matériaux communs et rares après chaque victoire
FR17: Le joueur peut crafter des cartes à la Forge en combinant des matériaux
FR18: Les recettes de craft sont cachées par défaut et découvertes empiriquement ou via PNJ
FR19: La Forge possède 3 niveaux débloqués par des matériaux rares de chaque zone
FR20: Le joueur peut naviguer sur une carte du monde et choisir une zone à explorer
FR21: Les 5 zones principales sont accessibles librement ; les Ruines se déverrouillent après progression dans toutes les autres zones
FR22: Chaque zone possède une histoire complète (début, milieu, fin) avec PNJ
FR23: Certains PNJ apparaissent uniquement après progression dans d'autres zones
FR24: La progression est sauvegardée automatiquement après chaque combat
FR25: Le joueur peut sauvegarder manuellement à tout moment
FR26: Une nouvelle partie repart de zéro sauf les recettes découvertes (conservées)
FR27: L'interface est disponible en Français et en Anglais
FR28: Les sauvegardes sont synchronisées avec Steam Cloud

### NonFunctional Requirements

NFR1: 60 fps stables sur configuration recommandée (30 fps minimum sur config minimum)
NFR2: Résolution 1080p cible, 1440p et 4K supportés (UI scalable)
NFR3: Chargement initial < 10 secondes
NFR4: Taille du build < 1 GB
NFR5: Zéro perte de progression (sauvegarde auto fiable)
NFR6: Feedback visuel immédiat sur chaque action joueur (< 100ms)
NFR7: Aucune fonctionnalité réseau (offline sauf Steam Cloud Sync)
NFR8: Accessibilité : texte redimensionnable, touches reconfigurables

### Additional Requirements

- Moteur : Unity 6.3 LTS (6000.3.12f1), URP 17.3.0
- Pattern : ScriptableObject Architecture + Service Locator
- 4 Assembly Definitions (Core, Gameplay, UI, Infrastructure) dès l'Epic 1
- State Machine explicite pour le combat (ICombatState)
- Pas de sauvegarde mid-combat (ADR-001)
- UI : UI Toolkit exclusivement (UXML/USS) — pas de Canvas uGUI
- Communication inter-systèmes : SO Event Channels
- KeywordResolver : service pur C# (pas MonoBehaviour, pas d'état)
- JsonSaveSystem : System.Text.Json, chemin Application.persistentDataPath
- Boot scene avec ServiceLocator initialisé avant toute scène de gameplay
- Tests unitaires Edit Mode pour : KeywordResolver, DeckValidator, CraftSystem, JsonSaveSystem

### UX Design Requirements

N/A — Pas de document UX Design pour ce projet.

### FR Coverage Map

| FR | Epic | Description |
|---|---|---|
| FR1 | Epic 2 | Plateau 6 zones, duel contre monstre |
| FR2 | Epic 2 | Cartes Action (attaque ciblée) |
| FR3 | Epic 2 | Cartes Blocage (hors tour) |
| FR4 | Epic 2 | Cartes Allié (permanent, actif après 1 tour) |
| FR5 | Epic 2 | Cartes Équipement (usage unique) |
| FR6 | Epic 2 | Cartes Réaction (contre Instantané) |
| FR7 | Epic 2 | Sacrifice de cartes → mana |
| FR8 | Epic 2 | Mana perdu fin de tour |
| FR9 | Epic 2 | Piocher jusqu'à 6 cartes |
| FR10 | Epic 2 | 8 mots-clés fonctionnels |
| FR11 | Epic 2 | IA monstre deck scripté |
| FR12 | Epic 5/6/7 | Boss mécanique passive unique (1+4+1) |
| FR13 | Epic 2 | Fin de partie quand PV = 0 |
| FR14 | Epic 4 | Défaite → verrouillage temporaire |
| FR15 | Epic 2 | Deck 40 cartes, contraintes de construction |
| FR16 | Epic 3 | Récolte matériaux après victoire |
| FR17 | Epic 3 | Craft de cartes à la Forge |
| FR18 | Epic 3 | Recettes cachées, découverte empirique |
| FR19 | Epic 3 | Forge 3 niveaux déblocables |
| FR20 | Epic 4 | Navigation carte du monde |
| FR21 | Epic 4/7 | 5 zones libres + Ruines verrouillées |
| FR22 | Epic 5/6/7 | Histoire complète par zone avec PNJ |
| FR23 | Epic 4/6 | PNJ conditionnels inter-zones |
| FR24 | Epic 1 | Sauvegarde auto post-combat |
| FR25 | Epic 1 | Sauvegarde manuelle |
| FR26 | Epic 4 | Nouvelle partie : reset sauf recettes |
| FR27 | Epic 8 | Localisation FR/EN |
| FR28 | Epic 8 | Steam Cloud Sync |

## Epic List

### Epic 1 : Fondations Techniques
Le joueur peut lancer le jeu, qui démarre de manière fiable avec tous les systèmes initialisés, et sa progression est sauvegardée automatiquement.
**FRs couverts :** FR24, FR25
**NFRs adressés :** NFR1, NFR2, NFR3, NFR5, NFR7

### Epic 2 : Système de Combat (MVP)
Le joueur peut affronter un monstre en duel de cartes complet — jouer des cartes des 5 types, gérer le mana, utiliser les 8 mots-clés, et remporter ou perdre le combat.
**FRs couverts :** FR1, FR2, FR3, FR4, FR5, FR6, FR7, FR8, FR9, FR10, FR11, FR13, FR15

### Epic 3 : Système de Craft & Forge
Le joueur peut récolter des matériaux après victoire, crafter de nouvelles cartes à la Forge, et débloquer progressivement les niveaux de la Forge.
**FRs couverts :** FR16, FR17, FR18, FR19

### Epic 4 : Monde & Navigation
Le joueur peut naviguer sur la carte du monde, choisir une zone, suivre sa progression, et recommencer une partie en conservant ses recettes découvertes.
**FRs couverts :** FR14, FR20, FR21, FR23, FR26

### Epic 5 : Zone Pilote — Les Plaines Libres
Le joueur peut explorer la zone des Plaines avec son histoire complète, ses PNJ, et affronter un boss doté d'une mécanique passive unique.
**FRs couverts :** FR12 (1/6), FR22 (1/6)

### Epic 6 : Zones Thématiques (×4)
Le joueur peut explorer les 4 zones thématiques (Marais, Forêt, Montagnes, Archipel), chacune avec son histoire, ses PNJ conditionnels, et son boss unique.
**FRs couverts :** FR12 (4/6), FR22 (4/6), FR23 (PNJ inter-zones)

### Epic 7 : Zone Finale — Les Ruines Oubliées
Le joueur peut accéder aux Ruines après avoir progressé dans toutes les autres zones, et vivre le climax narratif avec le boss final.
**FRs couverts :** FR12 (1/6), FR21 (déverrouillage Ruines), FR22 (1/6)

### Epic 8 : Polish & Release
Le jeu est disponible en Français et en Anglais, et les sauvegardes sont synchronisées avec Steam Cloud.
**FRs couverts :** FR27, FR28
**NFRs adressés :** NFR4, NFR8

---

## Epic 1 : Fondations Techniques

### Story 1.1 : Démarrage fiable du jeu

As a **joueur**,
I want **que le jeu se lance et initialise tous ses systèmes en moins de 10 secondes**,
So that **je peux commencer à jouer immédiatement sans erreurs d'initialisation**.

**Acceptance Criteria :**

**Given** que le joueur lance l'exécutable
**When** la Boot Scene se charge
**Then** le ServiceLocator est initialisé avant toute scène de gameplay
**And** les 4 Assembly Definitions (Core, Gameplay, UI, Infrastructure) sont compilées sans erreur
**And** le chargement initial est inférieur à 10 secondes (NFR3)
**And** le jeu tourne à 60fps stables sur la config recommandée (NFR1)
**And** la résolution 1080p est appliquée par défaut avec UI scalable (NFR2)

---

### Story 1.2 : Système de sauvegarde automatique post-combat

As a **joueur**,
I want **que ma progression soit automatiquement sauvegardée après chaque combat**,
So that **je ne perds jamais ma progression en cas de crash ou fermeture accidentelle**.

**Acceptance Criteria :**

**Given** qu'un combat vient de se terminer (victoire ou défaite)
**When** la scène post-combat se charge
**Then** le JsonSaveSystem écrit un fichier JSON à `Application.persistentDataPath/save.json`
**And** aucune sauvegarde n'est effectuée pendant le combat (ADR-001)
**And** si une sauvegarde existante est corrompue, le système le détecte et propose de repartir de zéro sans crash
**And** les tests Edit Mode de JsonSaveSystem passent sans erreur

---

### Story 1.3 : Sauvegarde manuelle à tout moment

As a **joueur**,
I want **pouvoir sauvegarder manuellement depuis le menu pause**,
So that **je peux quitter le jeu en sécurité quand je le souhaite**.

**Acceptance Criteria :**

**Given** que le joueur est sur la carte du monde ou dans un menu (hors combat)
**When** il accède au menu pause et appuie sur "Sauvegarder"
**Then** la progression est écrite dans `save.json` via le JsonSaveSystem
**And** un feedback visuel de confirmation s'affiche en moins de 100ms (NFR6)
**And** la sauvegarde manuelle et la sauvegarde auto utilisent le même système et le même fichier

---

## Epic 2 : Système de Combat (MVP)

### Story 2.1 : Construction et validation d'un deck

As a **joueur**,
I want **construire un deck de 40 cartes en respectant les contraintes de composition**,
So that **je peux entrer en combat avec un deck légal et personnalisé**.

**Acceptance Criteria :**

**Given** que le joueur est dans l'écran de construction de deck
**When** il sélectionne des cartes pour son deck
**Then** le DeckValidator refuse tout deck de moins ou plus de 40 cartes
**And** le DeckValidator refuse plus de 3 copies d'une carte Commune
**And** le DeckValidator refuse plus de 1 copie d'une carte Légendaire ou Unique
**And** le DeckValidator refuse un deck dont le poids total dépasse 15
**And** les tests Edit Mode de DeckValidator passent sans erreur

---

### Story 2.2 : Initialisation du plateau de combat

As a **joueur**,
I want **voir le plateau de combat s'afficher avec les 6 zones au démarrage d'un duel**,
So that **je comprends l'espace de jeu et peux commencer à jouer**.

**Acceptance Criteria :**

**Given** que le joueur sélectionne un monstre à affronter
**When** le combat démarre
**Then** les 6 zones sont affichées : Main, Deck, Cimetière, Recrutement, Affrontement, Équipement
**And** la CombatStateMachine est initialisée dans l'état DrawState
**And** les points de vie du joueur et du monstre sont affichés
**And** le deck du joueur est mélangé et placé dans la zone Deck

---

### Story 2.3 : Phase de pioche

As a **joueur**,
I want **piocher jusqu'à 6 cartes en début de tour**,
So that **j'ai toujours des options de jeu disponibles**.

**Acceptance Criteria :**

**Given** que la CombatStateMachine entre dans DrawState
**When** la phase de pioche commence
**Then** le joueur pioche des cartes jusqu'à avoir 6 cartes en main (ou jusqu'à vider son deck)
**And** les cartes piochées s'affichent dans la zone Main avec une animation
**And** si le deck est vide lors d'une pioche obligatoire, le joueur subit une défaite (CombatEndState)
**And** si la main est pleine (6 cartes) et que le deck est vide en début de tour, aucune pioche n'est requise — le jeu continue normalement
**And** la CombatStateMachine transite vers SacrificeState après la pioche

---

### Story 2.4 : Sacrifice de cartes pour le mana

As a **joueur**,
I want **sacrifier des cartes de ma main pour générer du mana**,
So that **je peux financer le jeu de mes autres cartes**.

**Acceptance Criteria :**

**Given** que la CombatStateMachine est dans SacrificeState
**When** le joueur glisse une carte de sa main vers la zone Deck (bas du deck)
**Then** la carte est placée en bas du deck et le mana du joueur augmente de la valeur de mana indiquée sur la carte sacrifiée
**And** le joueur peut sacrifier autant de cartes qu'il le souhaite
**When** le joueur confirme la fin de la phase de sacrifice
**Then** la CombatStateMachine transite vers PlayState
**And** tout mana non utilisé en fin de tour (transition vers MonsterTurnState) est remis à 0

---

### Story 2.5 : Jouer une carte Action

As a **joueur**,
I want **jouer une carte Action pour attaquer un adversaire ciblé**,
So that **je peux infliger des dégâts et réduire les PV de mon adversaire**.

**Acceptance Criteria :**

**Given** que la CombatStateMachine est dans PlayState
**When** le joueur joue une carte Action en ciblant le monstre ou un Allié adverse
**Then** le coût en mana est débité et la carte retourne en bas du deck (zone Deck)
**And** les dégâts de l'Action sont appliqués à la cible
**And** un feedback visuel s'affiche en moins de 100ms (NFR6)
**And** si le joueur n'a pas assez de mana, la carte est refusée avec un retour visuel

---

### Story 2.6 : Jouer une carte Allié

As a **joueur**,
I want **jouer une carte Allié qui devient active après 1 tour**,
So that **je peux construire une présence permanente sur le plateau**.

**Acceptance Criteria :**

**Given** que la CombatStateMachine est dans PlayState
**When** le joueur joue une carte Allié en dépensant son mana
**Then** l'Allié est placé dans la zone Recrutement avec un indicateur "en sommeil"
**When** le tour suivant du joueur commence
**Then** l'Allié passe dans la zone Affrontement avec ses valeurs ATK/DEF affichées et devient actif
**And** un Allié actif peut être ciblé par des Actions adverses

---

### Story 2.7 : Jouer une carte Équipement

As a **joueur**,
I want **jouer une carte Équipement à usage unique sur un Allié ciblé**,
So that **je peux renforcer temporairement mes Alliés en combat**.

**Acceptance Criteria :**

**Given** que la CombatStateMachine est dans PlayState et que le joueur a au moins un Allié actif
**When** le joueur joue une carte Équipement sur un Allié cible
**Then** la carte est placée dans la zone Équipement et son effet s'applique à l'Allié
**And** après que l'effet est déclenché (une fois), la carte est envoyée au Cimetière
**And** si aucun Allié n'est disponible, la carte Équipement ne peut pas être jouée

---

### Story 2.8 : Réponses hors-tour — Blocage & Réaction

As a **joueur**,
I want **jouer des cartes Blocage contre une Action adverse et des cartes Réaction contre un Instantané**,
So that **je peux me défendre même en dehors de mon tour**.

**Acceptance Criteria :**

**Given** que la CombatStateMachine est dans ReactiveWindowState (déclenché par une Action ou Instantané adverse)
**When** le joueur a une carte Blocage en main et l'adversaire joue une Action
**Then** le joueur peut jouer sa carte Blocage pour annuler ou réduire l'Action
**And** la carte Blocage retourne en bas du deck après résolution
**When** l'adversaire joue une carte avec le mot-clé Instantané
**Then** le joueur peut répondre avec une carte Réaction
**And** la carte Réaction retourne en bas du deck après résolution
**And** si la fenêtre réactive expire (timer ou confirmation), l'effet adverse s'applique normalement
**And** une carte Blocage ne peut pas être jouée pendant le tour du joueur (PlayState)

---

### Story 2.9 : Les 8 mots-clés

As a **joueur**,
I want **que les 8 mots-clés des cartes fonctionnent selon leurs définitions**,
So that **les effets spéciaux des cartes enrichissent la stratégie de combat**.

**Acceptance Criteria :**

**Given** une carte avec le mot-clé **Vol** dans la zone Affrontement
**Then** elle ne peut être ciblée que par des cartes avec le mot-clé Portée
**Given** une carte avec **Provocation**
**Then** les Actions adverses doivent la cibler en priorité
**Given** une carte avec **Éveillé**
**Then** elle est active dès le tour où elle est jouée (pas de délai d'un tour)
**Given** une carte avec **Invincible**
**Then** elle survit à la prochaine destruction qui devrait l'éliminer
**Given** un **Allié** avec le mot-clé **Rampant** présent dans la zone Cimetière (détruit précédemment)
**When** le joueur est en PlayState et dispose du mana requis
**Then** le joueur peut jouer cet Allié depuis le Cimetière pour son coût de base en mana
**And** Rampant ne s'applique qu'aux cartes Allié (seul type envoyé au Cimetière lors d'une destruction)
**Given** une carte avec **Rituel(X)**
**Then** X cartes doivent être sacrifiées comme coût de jeu
**And** le KeywordResolver est un service pur C# (pas MonoBehaviour) et ses tests Edit Mode passent

---

### Story 2.10 : IA Monstre

As a **joueur**,
I want **que le monstre joue ses cartes de manière lisible et prévisible**,
So that **je comprends sa stratégie et peux réagir en conséquence**.

**Acceptance Criteria :**

**Given** que la CombatStateMachine entre dans MonsterTurnState
**When** l'IA monstre joue son tour
**Then** elle pioche et joue depuis un deck scripté (séquence fixe, pas aléatoire)
**And** chaque action du monstre est annoncée par un texte lisible ("Le monstre attaque !")
**And** l'IA respecte les mêmes règles de ciblage que le joueur (Provocation, Vol, etc.)
**And** après son tour, la CombatStateMachine transite vers DrawState ou CombatEndState

---

### Story 2.11 : Fin de combat et résolution

As a **joueur**,
I want **que le combat se termine quand les PV d'un protagoniste atteignent 0**,
So that **je sache clairement si j'ai gagné ou perdu**.

**Acceptance Criteria :**

**Given** que les PV du monstre tombent à 0
**When** la CombatStateMachine entre dans CombatEndState
**Then** un écran de victoire s'affiche avec les récompenses (matériaux)
**And** la progression est automatiquement sauvegardée (FR24)
**Given** que les PV du joueur tombent à 0
**When** la CombatStateMachine entre dans CombatEndState
**Then** un écran de défaite s'affiche sans pénalité de progression permanente (non punitif)

---

## Epic 3 : Système de Craft & Forge

### Story 3.1 : Récolte de matériaux après victoire

As a **joueur**,
I want **recevoir des matériaux communs et rares à la fin d'un combat victorieux**,
So that **j'accumule les ressources nécessaires pour crafter de nouvelles cartes**.

**Acceptance Criteria :**

**Given** que le joueur vient de remporter un combat (CombatEndState — victoire)
**When** l'écran de récompenses s'affiche
**Then** le joueur reçoit au moins un matériau commun lié à la zone du monstre vaincu
**And** les matériaux rares ont une probabilité plus faible d'apparaître
**And** les matériaux sont ajoutés à l'inventaire du joueur et sauvegardés via JsonSaveSystem
**And** l'inventaire affiche le nom, la rareté et la quantité de chaque matériau possédé

---

### Story 3.2 : Interface de la Forge et craft de cartes

As a **joueur**,
I want **accéder à la Forge et combiner des matériaux pour créer une nouvelle carte**,
So that **je peux enrichir mon deck avec des cartes que je n'aurais pas trouvées autrement**.

**Acceptance Criteria :**

**Given** que le joueur accède à la Forge depuis la carte du monde
**When** il sélectionne des matériaux depuis son inventaire
**Then** le CraftSystem.TryCraft() cherche une correspondance parmi TOUTES les recettes (découvertes ou non)
**And** si une recette correspond, la carte craftée est ajoutée à la collection du joueur
**And** les matériaux utilisés sont retirés de l'inventaire et la sauvegarde est mise à jour
**And** si aucune recette ne correspond, aucun message d'erreur spécifique n'est affiché (échec silencieux)
**And** les tests Edit Mode du CraftSystem passent sans erreur

---

### Story 3.3 : Découverte empirique des recettes

As a **joueur**,
I want **que les recettes de craft soient cachées par défaut et se révèlent quand je les découvre**,
So that **l'exploration et l'expérimentation font partie du plaisir du craft**.

**Acceptance Criteria :**

**Given** qu'une recette n'a jamais été craftée
**When** le joueur consulte la Forge
**Then** la recette n'apparaît pas dans la liste des recettes connues
**When** le joueur combine les bons matériaux pour la première fois
**Then** la recette est marquée comme "découverte" dans SaveData et apparaît désormais dans la liste
**And** l'état "découvert" est uniquement dans SaveData pour l'affichage UI — le CraftSystem cherche toujours dans toutes les recettes
**And** lors d'une nouvelle partie, les recettes découvertes sont conservées (FR26)

---

### Story 3.4 : Déblocage des niveaux de la Forge

As a **joueur**,
I want **débloquer les niveaux 2 et 3 de la Forge en apportant des matériaux rares de chaque zone**,
So that **ma progression dans le monde se traduit par un accès à des recettes plus puissantes**.

**Acceptance Criteria :**

**Given** que la Forge est au niveau 1
**When** le joueur apporte les matériaux rares requis (spécifiques à chaque zone)
**Then** la Forge passe au niveau 2, déverrouillant un nouveau catalogue de recettes
**And** le niveau 3 se déverrouille de la même manière avec des matériaux rares de zones plus avancées
**And** le niveau actuel de la Forge est sauvegardé dans SaveData
**And** les recettes des niveaux supérieurs restent invisibles tant que le niveau requis n'est pas atteint

---

## Epic 4 : Monde & Navigation

### Story 4.1 : Navigation sur la carte du monde

As a **joueur**,
I want **voir la carte du monde et sélectionner une zone à explorer**,
So that **je peux choisir librement où aller et qui affronter**.

**Acceptance Criteria :**

**Given** que le joueur est sur la carte du monde
**When** il survole une zone disponible
**Then** le nom de la zone, une courte description et l'état de progression s'affichent
**When** il clique sur une zone
**Then** il entre dans cette zone et voit les monstres disponibles à affronter
**And** les zones déjà complétées affichent un indicateur visuel de progression
**And** la navigation est gérée via l'UI Toolkit (UXML/USS) — aucun Canvas uGUI

---

### Story 4.2 : Accès aux zones et verrouillage des Ruines

As a **joueur**,
I want **accéder librement aux 5 zones principales, et débloquer les Ruines après avoir progressé partout**,
So that **le monde s'ouvre progressivement au fil de ma progression**.

**Acceptance Criteria :**

**Given** que le joueur démarre une partie
**When** il consulte la carte du monde
**Then** les 5 zones principales (Plaines, Marais, Forêt, Montagnes, Archipel) sont accessibles librement
**And** la zone des Ruines est verrouillée et affiche une indication de condition de déverrouillage
**When** le joueur a atteint la condition de progression dans toutes les autres zones
**Then** les Ruines se déverrouillent et deviennent accessibles
**And** l'état de déverrouillage est sauvegardé dans SaveData

---

### Story 4.3 : Verrouillage temporaire après défaite

As a **joueur**,
I want **qu'un monstre soit temporairement indisponible après que je l'aie affronté et perdu**,
So that **la défaite a une conséquence légère sans être punitive**.

**Acceptance Criteria :**

**Given** que le joueur vient de perdre un combat contre un monstre
**When** il retourne sur la carte du monde
**Then** le monstre vainqueur affiche un état "indisponible" avec un indicateur visuel
**And** le joueur peut toujours accéder à tous les autres monstres de la zone
**When** la condition de temps ou de progression requise est remplie
**Then** le monstre redevient disponible
**And** aucune pénalité permanente n'est appliquée sur l'inventaire ou le deck du joueur

---

### Story 4.4 : PNJ conditionnels selon la progression

As a **joueur**,
I want **rencontrer de nouveaux PNJ qui apparaissent uniquement après avoir progressé dans d'autres zones**,
So that **le monde s'enrichit et récompense l'exploration**.

**Acceptance Criteria :**

**Given** qu'un PNJ a une condition d'apparition liée à la progression d'une autre zone
**When** le joueur n'a pas encore rempli cette condition
**Then** le PNJ n'est pas visible dans la zone
**When** le joueur revient dans la zone après avoir rempli la condition
**Then** le PNJ apparaît avec son dialogue et ses interactions disponibles
**And** les conditions d'apparition sont définies dans le ZoneData ScriptableObject de chaque zone

---

### Story 4.5 : Nouvelle partie avec conservation des recettes

As a **joueur**,
I want **recommencer une nouvelle partie depuis zéro tout en gardant mes recettes découvertes**,
So that **mes découvertes de craft persistent et récompensent mon exploration passée**.

**Acceptance Criteria :**

**Given** que le joueur choisit "Nouvelle partie" depuis le menu principal
**When** la nouvelle partie démarre
**Then** la progression, l'inventaire, le deck et les PNJ rencontrés sont réinitialisés
**And** les recettes découvertes dans SaveData sont conservées et visibles dans la Forge dès le début
**And** une confirmation est demandée avant de lancer la nouvelle partie pour éviter toute perte accidentelle

---

## Epic 5 : Zone Pilote — Les Plaines Libres

### Story 5.1 : Histoire et PNJ des Plaines Libres

As a **joueur**,
I want **explorer la zone des Plaines avec une histoire complète (début, milieu, fin) et des PNJ à rencontrer**,
So that **j'ai une raison narrative de progresser dans cette zone**.

**Acceptance Criteria :**

**Given** que le joueur entre dans la zone des Plaines
**When** il interagit avec les PNJ de la zone
**Then** il accède aux dialogues narratifs couvrant les trois actes de la zone (introduction, développement, résolution)
**And** chaque PNJ a un rôle distinct dans l'histoire (guide, marchand de recettes, témoin des événements)
**And** la progression narrative de la zone est trackée dans SaveData
**And** les données de la zone sont définies dans un `ZoneData` ScriptableObject (Assets/Data/Zones/)

---

### Story 5.2 : Boss des Plaines — Mécanique passive unique

As a **joueur**,
I want **affronter le boss des Plaines qui possède une mécanique passive activée en combat**,
So that **le boss représente un défi distinct des monstres normaux et enrichit la stratégie**.

**Acceptance Criteria :**

**Given** que le joueur affronte le boss des Plaines
**When** le combat commence
**Then** la mécanique passive du boss s'active selon sa définition (ex. : régénération de PV à chaque fin de tour adverse)
**And** la mécanique passive est visible et lisible pour le joueur via un indicateur permanent sur le plateau
**And** la mécanique passive est définie dans le `MonsterData` ScriptableObject du boss (Assets/Data/Monsters/)
**And** vaincre le boss déverrouille l'accès au matériau rare des Plaines (utilisé pour la Forge et la progression)

---

## Epic 6 : Zones Thématiques (×4)

### Story 6.1 : Zone du Marais — Histoire, PNJ et Boss

As a **joueur**,
I want **explorer le Marais avec son histoire complète et affronter son boss unique**,
So that **je vis une expérience narrative et stratégique distincte des Plaines**.

**Acceptance Criteria :**

**Given** que le joueur entre dans la zone du Marais
**When** il progresse dans la zone
**Then** il rencontre les PNJ du Marais avec leurs trois actes narratifs (début, milieu, fin)
**And** le boss du Marais possède une mécanique passive distincte de celle du boss des Plaines (ex. : invocation d'Alliés faibles à chaque tour)
**And** la victoire contre le boss déverrouille le matériau rare du Marais
**And** le contenu est défini dans les assets SO dédiés (`ZoneData_Marais.asset`, `Monster_Marais_Boss.asset`)

---

### Story 6.2 : Zone de la Forêt — Histoire, PNJ et Boss

As a **joueur**,
I want **explorer la Forêt avec son histoire complète et affronter son boss unique**,
So that **je vis une expérience narrative et stratégique distincte des autres zones**.

**Acceptance Criteria :**

**Given** que le joueur entre dans la zone de la Forêt
**When** il progresse dans la zone
**Then** il rencontre les PNJ de la Forêt avec leurs trois actes narratifs
**And** le boss de la Forêt possède une mécanique passive distincte (ex. : les Actions du joueur coûtent 1 mana de plus)
**And** la victoire contre le boss déverrouille le matériau rare de la Forêt
**And** le contenu est défini dans les assets SO dédiés (`ZoneData_Foret.asset`, `Monster_Foret_Boss.asset`)

---

### Story 6.3 : Zone des Montagnes — Histoire, PNJ et Boss

As a **joueur**,
I want **explorer les Montagnes avec son histoire complète et affronter son boss unique**,
So that **je vis une expérience narrative et stratégique distincte des autres zones**.

**Acceptance Criteria :**

**Given** que le joueur entre dans la zone des Montagnes
**When** il progresse dans la zone
**Then** il rencontre les PNJ des Montagnes avec leurs trois actes narratifs
**And** le boss des Montagnes possède une mécanique passive distincte (ex. : gagne +1 DEF à chaque tour)
**And** la victoire contre le boss déverrouille le matériau rare des Montagnes
**And** le contenu est défini dans les assets SO dédiés (`ZoneData_Montagnes.asset`, `Monster_Montagnes_Boss.asset`)

---

### Story 6.4 : Zone de l'Archipel — Histoire, PNJ et Boss

As a **joueur**,
I want **explorer l'Archipel avec son histoire complète et affronter son boss unique**,
So that **je vis une expérience narrative et stratégique distincte des autres zones**.

**Acceptance Criteria :**

**Given** que le joueur entre dans la zone de l'Archipel
**When** il progresse dans la zone
**Then** il rencontre les PNJ de l'Archipel avec leurs trois actes narratifs
**And** le boss de l'Archipel possède une mécanique passive distincte (ex. : immunité aux cartes Blocage)
**And** la victoire contre le boss déverrouille le matériau rare de l'Archipel
**And** le contenu est défini dans les assets SO dédiés (`ZoneData_Archipel.asset`, `Monster_Archipel_Boss.asset`)

---

### Story 6.5 : PNJ conditionnels inter-zones

As a **joueur**,
I want **découvrir de nouveaux PNJ dans les zones thématiques après avoir progressé ailleurs**,
So that **explorer plusieurs zones me récompense par du contenu narratif supplémentaire**.

**Acceptance Criteria :**

**Given** qu'un PNJ inter-zone a une condition liée à la progression d'une autre zone thématique
**When** le joueur revient dans une zone après avoir rempli la condition
**Then** le PNJ apparaît avec un dialogue qui fait référence à la progression réalisée ailleurs
**And** ces PNJ peuvent donner des indices sur des recettes de craft cachées (FR18)
**And** les conditions inter-zones sont définies dans les `ZoneData` ScriptableObjects concernés

---

## Epic 7 : Zone Finale — Les Ruines Oubliées

### Story 7.1 : Déverrouillage et accès aux Ruines Oubliées

As a **joueur**,
I want **débloquer l'accès aux Ruines après avoir complété les 5 zones principales**,
So that **ma progression globale dans le monde est récompensée par le climax de l'aventure**.

**Acceptance Criteria :**

**Given** que le joueur a atteint la condition de fin dans chacune des 5 zones principales (Plaines, Marais, Forêt, Montagnes, Archipel)
**When** il consulte la carte du monde
**Then** les Ruines Oubliées se déverrouillent avec une cinématique ou animation d'ouverture
**And** un message narratif contextualise l'accès aux Ruines
**And** l'état de déverrouillage est sauvegardé dans SaveData et persiste entre les sessions

---

### Story 7.2 : Histoire et PNJ des Ruines Oubliées

As a **joueur**,
I want **explorer les Ruines avec une histoire climax qui fait écho à toute l'aventure**,
So that **mon parcours trouve une conclusion narrative satisfaisante**.

**Acceptance Criteria :**

**Given** que le joueur entre dans les Ruines Oubliées
**When** il interagit avec les PNJ de la zone
**Then** les dialogues font référence aux événements et personnages rencontrés dans les autres zones
**And** l'histoire couvre les trois actes (révélation, confrontation, résolution finale)
**And** le contenu est défini dans les assets SO dédiés (`ZoneData_Ruines.asset`)

---

### Story 7.3 : Boss Final — Mécanique passive et conclusion

As a **joueur**,
I want **affronter le boss final des Ruines avec une mécanique passive qui synthétise les défis de l'aventure**,
So that **le combat final est le point culminant stratégique et narratif du jeu**.

**Acceptance Criteria :**

**Given** que le joueur affronte le boss final des Ruines
**When** le combat commence
**Then** la mécanique passive du boss est plus complexe que celles des zones précédentes (ex. : combine deux effets passifs tirés des autres boss)
**And** la mécanique passive est visible et lisible via un indicateur permanent sur le plateau
**And** la victoire déclenche la séquence de fin du jeu (épilogue narratif)
**And** le contenu est défini dans `Monster_Ruines_BossFinal.asset`

---

## Epic 8 : Polish & Release

### Story 8.1 : Localisation Français / Anglais

As a **joueur**,
I want **choisir la langue du jeu entre le Français et l'Anglais**,
So that **je peux jouer dans ma langue préférée**.

**Acceptance Criteria :**

**Given** que le joueur accède aux paramètres du jeu
**When** il sélectionne une langue (FR ou EN)
**Then** toute l'interface, les dialogues PNJ, les textes de cartes et les menus basculent dans la langue choisie
**And** le Français est la langue par défaut au premier lancement
**And** la langue choisie est persistée dans SaveData
**And** la localisation est implémentée via Unity Localization Package — aucune string hardcodée dans le code

---

### Story 8.2 : Accessibilité — texte et contrôles

As a **joueur**,
I want **ajuster la taille des textes et reconfigurer les touches du jeu**,
So that **je peux adapter l'expérience à mes besoins**.

**Acceptance Criteria :**

**Given** que le joueur accède aux paramètres d'accessibilité
**When** il ajuste le curseur de taille de texte
**Then** tous les textes de l'UI s'adaptent en temps réel sans casser la mise en page (UI Toolkit scalable)
**When** il reconfigure une touche dans le remapping des contrôles
**Then** la nouvelle liaison est sauvegardée et appliquée immédiatement via le New Input System
**And** les paramètres d'accessibilité sont persistés dans SaveData

---

### Story 8.3 : Synchronisation Steam Cloud

As a **joueur**,
I want **que mes sauvegardes soient automatiquement synchronisées avec Steam Cloud**,
So that **je retrouve ma progression sur n'importe quel PC où j'ai Steam**.

**Acceptance Criteria :**

**Given** que le joueur est connecté à Steam
**When** une sauvegarde est écrite à `Application.persistentDataPath/save.json`
**Then** Steamworks.NET déclenche la synchronisation avec Steam Cloud
**And** au démarrage du jeu, si une sauvegarde cloud est plus récente que la sauvegarde locale, le joueur est invité à choisir laquelle utiliser
**And** si Steam est indisponible, le jeu fonctionne normalement en mode offline avec la sauvegarde locale (NFR7)

---

### Story 8.4 : Optimisation et validation du build release

As a **développeur**,
I want **que le build release soit inférieur à 1 GB et passe tous les critères de performance**,
So that **le jeu est distribuable sur Steam sans friction**.

**Acceptance Criteria :**

**Given** que le build release est généré
**When** la taille du build est mesurée
**Then** elle est inférieure à 1 GB (NFR4)
**And** tous les `Debug.Log` sont remplacés par `GameLog` et strippés du build release
**And** tous les blocs `#if UNITY_EDITOR` sont absents du build final
**And** le jeu atteint 60fps stables sur la configuration recommandée et 30fps minimum sur la configuration minimale (NFR1)
