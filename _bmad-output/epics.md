---
game_name: 'Monster Card Game'
date: '2026-04-05'
status: 'draft'
source: 'gdd-step-12'
---

# Monster Card Game — Development Epics

## Epic Overview

| # | Epic | Dépendances | Jalon jouable |
|---|---|---|---|
| 1 | Fondations Techniques | — | Non |
| 2 | Système de Combat (MVP) | Epic 1 | Combat avec cartes hardcodées |
| 3 | Système de Craft & Forge | Epic 2 | Construction de deck depuis ressources |
| 4 | Monde & Progression | Epic 3 | Aventure complète navigable |
| 5 | Zone Pilote : Les Plaines Libres | Epic 4 | Zone complète de A à Z |
| 6 | Zones Thématiques (×4) | Epic 5 | Monde principal complet |
| 7 | Zone Finale : Les Ruines Oubliées | Epic 6 | Aventure terminée |
| 8 | Polish & Release | Epic 7 | Release ready |

---

## Epic 1 : Fondations Techniques

### Goal

Mettre en place l'architecture Unity, le modèle de données carte/deck et la structure de scènes. Aucune fonctionnalité jouable — tout le reste du projet en dépend.

### Scope

**Inclut :**
- Mise en place du projet Unity 6.3 LTS
- Architecture de base (structure de dossiers, namespaces, patterns)
- Modèle de données : Card, Deck, Player, Monster (ScriptableObjects ou équivalent)
- Structure de scènes (MainMenu, Combat, WorldMap, Forge)
- Système de gestion de scènes
- Pipeline de build de base

**Exclut :**
- Tout gameplay fonctionnel
- Assets visuels définitifs
- Systèmes réseau ou Steam

### Dependencies

Aucune.

### Deliverable

Projet Unity structuré, compilant sans erreur, avec modèles de données de base instanciables en éditeur.

### Stories

- En tant que développeur, je peux créer une CardData avec tous ses champs (nom, coût, mana généré, type, rareté, mots-clés) via un ScriptableObject
- En tant que développeur, je peux créer un DeckData contenant une liste de CardData et valider les règles de construction (40 cartes, poids max 15)
- En tant que développeur, je peux naviguer entre les scènes principales depuis l'éditeur
- En tant que développeur, j'ai une architecture de base documentée (patterns utilisés, conventions de nommage)

---

## Epic 2 : Système de Combat (MVP)

### Goal

Implémenter le système de combat complet avec des cartes hardcodées — plateau 6 zones, types de cartes, mana, structure de tour, IA monstre scriptée, conditions de victoire et de défaite.

### Scope

**Inclut :**
- Plateau de combat : zones Main, Deck, Cimetière, Recrutement, Affrontement, Équipement
- Types de cartes : Action, Blocage, Allié, Équipement, Réaction
- Système de mana (génération par sacrifice, perte en fin de tour)
- Structure de tour complète (Pioche → Sacrifice → Jeu → Fin de tour)
- Phase réactive adversaire (Blocages, Réactions)
- Mots-clés : Vol, Portée, Instantané, Provocation, Éveillé, Rituel(X), Invincible, Rampant
- Alliés : délai d'un tour, ATK/DEF, ciblage
- IA monstre : deck scripté, comportement simple et lisible
- Mécaniques de boss : passive unique par boss
- Conditions de victoire (PV adversaire à 0) et défaite (PV joueur à 0 ou deck vide)
- Verrou temporaire post-défaite (non punitif)

**Exclut :**
- Craft ou matériaux
- Monde et navigation entre zones
- Assets visuels définitifs (placeholder accepté)

### Dependencies

Epic 1.

### Deliverable

Un combat de carte jouable de bout en bout avec 2–3 decks hardcodés (joueur + 1–2 monstres). Toutes les mécaniques de combat sont fonctionnelles.

### Stories

- En tant que joueur, je peux voir mon plateau de combat avec les 6 zones clairement identifiées
- En tant que joueur, je peux piocher jusqu'à 6 cartes en début de tour
- En tant que joueur, je peux sacrifier des cartes pour générer du mana
- En tant que joueur, je peux jouer une Action pour attaquer un allié ou le personnage adverse
- En tant que joueur, je peux poser un Allié qui attend un tour avant d'attaquer
- En tant que joueur, je peux jouer un Équipement depuis sa zone dédiée
- En tant que joueur adverse (IA), je peux bloquer une Action non-Instantanée avec un Blocage
- En tant que joueur, je peux jouer une Réaction hors de mon tour
- En tant que joueur, les mots-clés Vol, Portée, Instantané, Provocation, Éveillé, Rituel, Invincible et Rampant fonctionnent correctement
- En tant que joueur, la partie se termine quand les PV d'un des deux joueurs atteignent 0
- En tant que joueur, la défaite verrouille temporairement le monstre sans perdre ma progression

---

## Epic 3 : Système de Craft & Forge

### Goal

Implémenter le système de récolte de matériaux, la Forge (niveau 1), le craft de cartes Communes et Rares, et le deck builder. Le joueur peut construire son premier deck depuis ses ressources.

### Scope

**Inclut :**
- Drops de matériaux (communs/rares) après victoire en combat
- Forge niveau 1 (disponible dès le début)
- Interface de craft : sélection de recettes, coût en matériaux, confirmation
- Recettes cachées par défaut (découverte empirique ou PNJ)
- Craft de cartes Communes et Rares
- Deck builder : liste de cartes possédées, ajout/retrait, validation des règles (40 cartes, poids max 15, copies max)
- Inventaire de matériaux et de cartes

**Exclut :**
- Forge niveaux 2 et 3
- Cartes Légendaires et Uniques
- Matériaux de zones spécifiques (sauf Plaines pour les tests)
- Navigation monde (monde hardcodé pour les tests)

### Dependencies

Epic 2.

### Deliverable

Le joueur peut affronter un monstre, récolter ses matériaux, crafter des cartes et construire un deck jouable. Boucle de jeu principale complète.

### Stories

- En tant que joueur, je reçois des matériaux communs et potentiellement rares après chaque victoire
- En tant que joueur, je peux accéder à la Forge et voir les recettes disponibles (certaines découvertes, d'autres cachées)
- En tant que joueur, je peux crafter une carte Commune ou Rare en dépensant les matériaux requis
- En tant que joueur, je découvre une nouvelle recette en combinant des matériaux de façon empirique
- En tant que joueur, je peux ouvrir le deck builder, voir mes cartes possédées et construire un deck valide
- En tant que joueur, les règles de construction (poids, copies, taille) sont vérifiées en temps réel

---

## Epic 4 : Monde & Progression

### Goal

Implémenter la carte du monde, la navigation entre zones, le système de sauvegarde complet, les niveaux 2 et 3 de la Forge, et le déverrouillage conditionnel des Ruines.

### Scope

**Inclut :**
- Carte du monde interactive (6 zones visibles)
- Déverrouillage progressif de la carte (zones se révèlent au fil de l'exploration)
- Navigation entre zones (sélection depuis la carte)
- Système de sauvegarde : automatique après chaque combat + manuelle à tout moment
- Steam Cloud Save (si intégration Steam disponible à ce stade, sinon epic 8)
- Forge niveau 2 (Légendaires + nouvelles recettes) et niveau 3 (Uniques + nouvelles recettes)
- Craft de cartes Légendaires et Uniques
- Matériaux rares par zone (chaque zone a ses matériaux propres)
- Amélioration de la Forge (coût : 1 matériau rare de chaque zone + communs)
- Déverrouillage des Ruines Oubliées (condition : progression suffisante dans les 5 zones)
- PNJ cross-zones (apparaissent après progression dans plusieurs zones)

**Exclut :**
- Contenu narratif des zones (épics 5–7)
- Recettes liées à des zones non encore implémentées (placeholders acceptés)

### Dependencies

Epic 3.

### Deliverable

L'aventure complète est navigable de bout en bout — carte du monde, zones, Forge complète, sauvegarde, et verrou des Ruines fonctionnel.

### Stories

- En tant que joueur, je vois la carte du monde et peux sélectionner une zone
- En tant que joueur, la carte se révèle progressivement au fil de mon exploration
- En tant que joueur, ma progression est sauvegardée automatiquement après chaque combat
- En tant que joueur, je peux sauvegarder manuellement à tout moment
- En tant que joueur, je peux améliorer la Forge au niveau 2 puis 3 en réunissant les matériaux requis
- En tant que joueur, je peux crafter des cartes Légendaires et Uniques une fois la Forge améliorée
- En tant que joueur, les Ruines Oubliées se déverrouillent quand j'ai suffisamment progressé dans les autres zones
- En tant que joueur, certains PNJ apparaissent dans une zone après que j'ai progressé dans une autre

---

## Epic 5 : Zone Pilote — Les Plaines Libres

### Goal

Implémenter la première zone complète — histoire, monstres, boss, PNJ, drops de matériaux spécifiques. Valide tout le pipeline de contenu de A à Z.

### Scope

**Inclut :**
- Monstres des Plaines (design, decks scriptés, drops)
- Boss des Plaines avec mécanique passive unique
- Histoire de zone : début, milieu, fin
- PNJ locaux avec dialogues
- Matériaux spécifiques aux Plaines
- Cartes craftables depuis les matériaux des Plaines
- Intégration dans la carte du monde

**Exclut :**
- Contenu des autres zones
- PNJ cross-zones (implémentés en epic 4, contenus en epic 6)

### Dependencies

Epic 4.

### Deliverable

Les Plaines Libres jouables de bout en bout — histoire complète, tous les monstres et boss, craft des cartes de la zone. Zone de référence pour les suivantes.

### Stories

- En tant que joueur, je peux explorer les Plaines Libres depuis la carte du monde
- En tant que joueur, je rencontre plusieurs monstres de difficulté croissante dans les Plaines
- En tant que joueur, j'affronte le boss des Plaines avec sa mécanique passive unique
- En tant que joueur, l'histoire des Plaines a un début, un développement et une conclusion
- En tant que joueur, les PNJ des Plaines me donnent des informations sur des recettes de craft
- En tant que joueur, les matériaux des Plaines me permettent de crafter des cartes polyvalentes

---

## Epic 6 : Zones Thématiques (×4)

### Goal

Implémenter les 4 zones thématiques restantes — chacune avec sa mécanique dominante, son histoire et son contenu complet.

### Scope

**Inclut (×4 zones) :**
- Les Marais de la Putréfaction (cimetière, réanimation, malus)
- La Grande Forêt des Esprits (tokens, buffs, synergies alliés)
- Les Montagnes Éternelles (défense, blocages, verrouillage)
- L'Archipel des Abysses (dualité colossal/rapide, soins)

Pour chaque zone :
- Monstres (decks scriptés, drops)
- Boss avec mécanique passive unique
- Histoire locale (début, milieu, fin)
- PNJ locaux + cross-zone si applicable
- Matériaux et recettes spécifiques

**Exclut :**
- Les Ruines Oubliées (epic 7)

### Dependencies

Epic 5.

### Deliverable

Les 5 zones principales sont complètes et jouables. Le monde principal est terminé — le joueur peut explorer librement, construire des decks thématiques, et progresser vers les Ruines.

---

## Epic 7 : Zone Finale — Les Ruines Oubliées

### Goal

Implémenter la zone finale — monstres uniques, boss ultra, histoire culminante, et déverrouillage conditionnel depuis la carte du monde.

### Scope

**Inclut :**
- Monstres des Ruines (entités difformes, gardiens corrompus)
- Boss final (puissant, mécanique multi-phases ou complexe)
- Histoire culminante qui résout l'aventure
- Matériaux ultra-rares et cartes Uniques spécifiques aux Ruines
- Écran de fin / conclusion narrative après la victoire finale

**Exclut :**
- New game+, modes post-game

### Dependencies

Epic 6.

### Deliverable

L'aventure Monster Card Game a une fin. Le joueur peut terminer le jeu complet de bout en bout.

### Stories

- En tant que joueur, les Ruines se déverrouillent sur la carte après ma progression dans toutes les autres zones
- En tant que joueur, les monstres des Ruines représentent le niveau de difficulté le plus élevé du jeu
- En tant que joueur, le boss final a une mécanique unique et mémorable
- En tant que joueur, l'histoire se conclut de façon satisfaisante après la victoire finale
- En tant que joueur, je peux continuer à explorer et crafter après la fin si je le souhaite

---

## Epic 8 : Polish & Release

### Goal

Finaliser l'expérience joueur pour la release — UI polish, animations de cartes, audio définitif, localisation, intégration Steam complète, optimisation et QA.

### Scope

**Inclut :**
- Polish UI (animations, transitions, feedback visuel)
- Animations de cartes (tween, effets visuels de jeu de carte)
- Implémentation audio définitif (musiques de zones et boss, SFX complets)
- Localisation complète FR/EN
- Intégration Steam complète (Achievements, Cloud Saves, page Steam)
- Optimisation des performances (60 fps cible, profiling)
- QA : playtest complet, correction de bugs
- Build de release

**Exclut :**
- Nouveau contenu post-release
- Modes multijoueur

### Dependencies

Epic 7.

### Deliverable

Monster Card Game est prêt pour une release sur Steam.
