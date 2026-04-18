---
title: 'Game Architecture'
project: 'MonsterCardGame'
date: '2026-04-05'
author: 'Guillaume'
version: '1.0'
stepsCompleted: [1, 2, 3, 4, 5, 6, 7, 8, 9]
status: 'complete'
engine: 'Unity 6.3 LTS'
platform: 'PC (Steam)'

# Source Documents
gdd: '_bmad-output/gdd.md'
epics: '_bmad-output/epics.md'
brief: '_bmad-output/game-brief.md'
---

# Game Architecture

## Document Status

This architecture document is being created through the GDS Architecture Workflow.

**Steps Completed:** 9 of 9 (Complete) ✅

---

## Executive Summary

**Monster Card Game** est un jeu de cartes aventure solo développé avec **Unity 6.3 LTS** ciblant **PC (Steam)**.

**Décisions architecturales clés :**
- **ScriptableObject Architecture + Service Locator** — toutes les données de jeu sont des assets SO ; les services globaux sont accessibles via un registre léger
- **State Machine explicite** pour le combat — états distincts par phase avec gestion propre des fenêtres réactives hors-tour
- **JSON local + Steamworks.NET** pour la sauvegarde — pas de sauvegarde mid-combat
- **UI Toolkit** pour toute l'interface — layouts UXML + styles USS
- **SO Event Channels** pour la communication inter-systèmes, C# events en intra-système

**Structure du projet :** Organisation hybride (par type à la racine, par domaine dans Scripts/) avec 8 systèmes principaux mappés à des dossiers dédiés.

**Patterns définis :** 5 patterns (dont 2 novel : Hidden Recipe Discovery + Keyword Resolution Chain) assurant la cohérence entre toutes les sessions d'implémentation.

**Prêt pour :** Phase d'implémentation — Epic 1 (Fondations Techniques)

---

## Development Environment

### Prerequisites

- Unity 6.3 LTS (6000.3.12f1) — via Unity Hub
- Node.js 18+ — pour MCP Unity
- Git — gestion de version
- IDE : Rider ou VS Code avec extension C# Dev Kit

### AI Tooling (MCP Servers)

Les MCP servers suivants ont été sélectionnés pour améliorer le développement assisté par IA :

| MCP Server | Usage | Installation |
|---|---|---|
| **MCP Unity** (CoderGamester) | Inspecter/modifier scènes, lancer tests, accéder logs | Unity Package Manager (git URL) + Node.js |
| **Context7** (upstash) | Documentation Unity à jour dans chaque prompt | `claude mcp add context7 -- npx -y @upstash/context7-mcp` |

**Setup MCP Unity :**
1. Installer Node.js 18+
2. Unity : `Window > Package Manager > + > Add package from git URL`
   → `https://github.com/CoderGamester/mcp-unity.git`
3. Unity : `Tools > MCP Unity > Server Window > Start Server`

**Setup Context7 :**
```bash
claude mcp add context7 -- npx -y @upstash/context7-mcp
```

### First Steps

1. Créer le projet Unity 6.3 LTS avec URP (Universal Render Pipeline) template
2. Créer la structure de dossiers `Assets/` telle que définie dans Project Structure
3. Configurer les Assembly Definitions (`Core.asmdef`, `Gameplay.asmdef`, `UI.asmdef`, `Infrastructure.asmdef`)
4. Installer les packages via Package Manager : Input System, UI Toolkit, Unity Localization, Unity Test Framework
5. Installer les packages tiers : DOTween (Asset Store), Steamworks.NET (GitHub)
6. Configurer MCP Unity et Context7
7. Créer la scène `Boot.unity` avec le `BootLoader` et le `ServiceLocator`

---

## Project Context

### Game Overview

**Monster Card Game** — Jeu de cartes aventure solo où le joueur forge des cartes depuis les ressources des monstres vaincus. Chaque deck est unique et reflète le parcours du joueur.

### Technical Scope

**Plateforme :** PC (Steam)
**Genre :** Card Game / Adventure
**Moteur :** Unity 6.3 LTS — C#
**Développeur :** Solo (Guillaume)
**Niveau de complexité projet :** Moyenne-Haute

### Core Systems

| Système | Complexité | Priorité |
|---|---|---|
| Système de combat (plateau, tour, résolution) | Haute | Critique |
| Modèle de données cartes (ScriptableObjects) | Moyenne | Critique |
| Deck Builder + validation | Moyenne | Haute |
| Craft & Forge (recettes, matériaux) | Haute | Haute |
| IA Monstre (decks scriptés) | Faible-Moyenne | Haute |
| Carte du Monde & Navigation | Faible | Moyenne |
| Système de Sauvegarde | Moyenne | Critique |
| UI / Plateau de combat | Moyenne | Haute |

### Technical Requirements

- 60fps cible (30fps minimum), 1080p, build < 1GB
- Zéro réseau — entièrement offline (hors Steam Cloud)
- Sauvegarde automatique post-combat + manuelle à tout moment
- Localisation FR/EN dès le lancement
- Contenu entièrement designé à la main (pas de procédural)

### Complexity Drivers

1. **Résolution de mots-clés en combat** — Vol/Portée, Instantané/Blocage/Réaction forment un système d'interactions qui doit être architecturé explicitement
2. **Phases réactives hors-tour** — Machine à états de combat multi-fenêtres d'action
3. **Recettes cachées & découverte empirique** — Système de craft flexible sans exposer la logique de combinaison
4. **Sérialisation d'état complet** — Deck, Forge, matériaux, zones, recettes

### Technical Risks

| Risque | Impact | Mitigation |
|---|---|---|
| Interactions mots-clés (edge cases) | Élevé | Architecture explicite de résolution, tests unitaires |
| Sauvegarde mid-combat | Moyen | Décider tôt : sauver en combat ou uniquement entre combats |
| Volume de données contenu (6 zones) | Moyen | Conventions ScriptableObjects strictes dès Epic 1 |
| Expérience Unity avancée limitée | Moyen | Patterns simples et explicites, éviter over-engineering |

---

## Engine & Framework

### Selected Engine

**Unity 6.3 LTS** (6000.3.12f1) — C#

**Rationale :** Moteur déjà maîtrisé par le développeur, excellent support 2D avec URP, écosystème riche (assets store, documentation), pipeline Steam bien documenté. Version LTS garantissant la stabilité sur toute la durée du projet.

### Engine-Provided Architecture

| Composant | Solution | Notes |
|---|---|---|
| **Rendu 2D** | Universal Render Pipeline (URP) | Optimisé pour sprites 2D, effets visuels accessibles |
| **Physique** | Unity Physics 2D | Disponible mais usage minimal (jeu de cartes) |
| **Audio** | Unity Audio — AudioSource / AudioMixer | Suffisant pour musiques de zones + SFX |
| **Input** | Unity Input System (nouveau) | Souris + clavier, extensible gamepad |
| **Gestion de scènes** | Unity SceneManager | Transitions entre scènes principales |
| **Build** | Unity Build Profiles (Unity 6) | Export PC / Steam Deck |
| **Tests** | Unity Test Runner | Edit Mode + Play Mode |
| **Packages** | Unity Package Manager | Gestion des dépendances |

### Remaining Architectural Decisions

Les décisions suivantes doivent être prises explicitement :

- Pattern de code principal (MVC, Service Locator, ScriptableObject Architecture…)
- Machine à états du combat (état de tour, fenêtres réactives)
- Système d'événements (Unity Events vs C# events vs event bus)
- Stratégie de sérialisation / sauvegarde (JSON, binary…)
- Choix UI : UI Toolkit vs uGUI
- Organisation des assets et conventions de nommage
- Gestion de la localisation (Unity Localization Package ou custom)

### Development Environment — MCP Setup

Outils recommandés pour le développement assisté par IA :

#### MCP Unity (CoderGamester)

**Repo :** `CoderGamester/mcp-unity`
**Compatibilité :** Unity 6+, Claude Code ✓

**Capacités :**
- Inspecter et modifier GameObjects, composants, matériaux directement depuis l'IA
- Lancer les tests Unity Test Runner
- Recompiler les scripts, exécuter des commandes Editor
- Accéder aux logs de console Unity en temps réel
- Opérations batch atomiques (plusieurs actions en une seule requête)

**Installation :**
1. Installer Node.js 18+
2. Dans Unity : `Window > Package Manager > + > Add package from git URL`
   URL : `https://github.com/CoderGamester/mcp-unity.git`
3. Dans Unity : `Tools > MCP Unity > Server Window > Configure > Start Server`

#### Context7 (upstash)

**Repo :** `upstash/context7`
**Usage :** Documentation Unity à jour dans chaque prompt IA

**Installation :**
```bash
claude mcp add context7 -- npx -y @upstash/context7-mcp
```

*(API key optionnelle sur context7.com/dashboard pour limites plus élevées)*

---

## Architectural Decisions

### Decision Summary

| Catégorie | Décision | Justification |
|---|---|---|
| Pattern principal | ScriptableObject Architecture + Service Locator | Data-driven, natif Unity, testable |
| Machine à états combat | State Machine explicite (classes C#) | Gère proprement les fenêtres réactives |
| Sauvegarde | JSON local + Steamworks.NET (pas de save mid-combat) | Simple, debuggable, sync Steam |
| Système d'événements | SO Event Channels (inter) + C# events (intra) | Découplage sans over-engineering |
| UI Framework | UI Toolkit | Standard Unity 6, CSS-like, adapté jeu de cartes |
| Packages tiers | DOTween, Steamworks.NET, Unity Localization | Léger, adapté scope solo |

### State Management

**Approche : ScriptableObject Architecture + Service Locator**

Toutes les données de jeu sont des ScriptableObjects :
- `CardData` — définition d'une carte (coût, mana, type, rareté, mots-clés, effets)
- `MonsterData` — stats du monstre, deck scripté, drops, mécanique passive
- `ZoneData` — configuration de zone, liste de monstres, histoire
- `RecipeData` — combinaison de matériaux → carte produite
- `KeywordDefinition` — définition d'un mot-clé et sa résolution

Les services globaux sont accessibles via un Service Locator léger :

```csharp
// Enregistrement au démarrage (Boot scene)
Services.Register<ISaveSystem>(new JsonSaveSystem());
Services.Register<IAudioManager>(audioManager);
Services.Register<ICraftSystem>(craftSystem);

// Accès depuis n'importe où
var save = Services.Get<ISaveSystem>();
```

**Règle :** Les MonoBehaviours ne se référencent pas directement entre systèmes — ils passent par le Service Locator ou les Event Channels.

### Combat State Machine

**Approche : State Machine explicite avec classes C#**

Le combat est géré par un `CombatStateMachine` avec des états distincts :

```
CombatStateMachine
├── DrawState           → Pioche jusqu'à main pleine
├── SacrificeState      → Fenêtre optionnelle de sacrifice pour mana
├── PlayState           → Tour actif : jouer actions/alliés/équipements
├── ReactiveWindowState → Fenêtre adversaire : Blocages et Réactions
├── MonsterTurnState    → Tour IA monstre (deck scripté)
└── CombatEndState      → Victoire / Défaite / résolution fin de combat
```

Chaque état implémente `IEnterState`, `IUpdateState`, `IExitState`. Les transitions sont explicites et documentées.

Les mots-clés sont résolus par un `KeywordResolver` séparé :
- Ordre de résolution défini et fixe
- Chaque mot-clé est une classe indépendante (`IKeywordEffect`)
- Les interactions (Vol + Portée, Instantané + Réaction) sont des cas explicitement testés

### Data Persistence

**Approche : JSON local — pas de sauvegarde en cours de combat**

- Sérialisation : `System.Text.Json` (.NET 6+, inclus Unity 6)
- Sauvegarde **uniquement entre combats** (post-victoire, post-défaite, manuelle)
- Structure de la sauvegarde :

```
SaveData
├── PlayerProgress
│   ├── ZonesCompleted[]
│   ├── MonstersDefeated[]
│   └── DiscoveredRecipes[]
├── PlayerDeck (CardData references by ID)
├── PlayerInventory
│   ├── Materials[]
│   └── OwnedCards[]
└── ForgeLevel (1, 2 ou 3)
```

- Sync Steam Cloud via **Steamworks.NET** (fichier unique uploadé après chaque sauvegarde)
- Chemin local : `Application.persistentDataPath/save.json`

### Event System

**Inter-systèmes : ScriptableObject Event Channels**

```csharp
[CreateAssetMenu(menuName = "Events/Card Played Event")]
public class CardPlayedEvent : ScriptableObject
{
    public event Action<CardData> OnRaised;
    public void Raise(CardData card) => OnRaised?.Invoke(card);
}
```

**Intra-système : C# events/delegates standard**

### UI Framework

**UI Toolkit** (Unity 6 natif)

- Layouts en UXML, styles en USS (CSS-like)
- Un document par écran majeur (Combat, WorldMap, DeckBuilder, Forge)
- Le plateau de combat utilise des éléments visuels custom pour le drag & drop des cartes
- Pas de Canvas uGUI (sauf si un composant spécifique le requiert)

### Third-Party Packages

| Package | Version | Source | Usage |
|---|---|---|---|
| **DOTween** | Dernière stable | Asset Store | Animations cartes, transitions UI |
| **Steamworks.NET** | Dernière stable | GitHub | API Steam, achievements, Cloud Save |
| **Unity Localization** | Via Package Manager | Unity | FR/EN strings, tables de traduction |
| **Unity Test Framework** | Inclus Unity 6 | Package Manager | Tests Edit Mode + Play Mode |

### Architecture Decision Records

**ADR-001 : Pas de sauvegarde mid-combat**
Décision : La sauvegarde ne capture pas l'état d'un combat en cours.
Justification : Simplification majeure de la sérialisation (état du combat = volatile).
Conséquence : Si le jeu se ferme pendant un combat, ce combat est perdu — acceptable.

**ADR-002 : KeywordResolver séparé du CombatStateMachine**
Décision : La résolution des mots-clés est un système indépendant.
Justification : Les interactions entre mots-clés (edge cases) doivent être testables unitairement sans instancier le combat complet.

**ADR-003 : ScriptableObjects comme source de vérité des données**
Décision : Aucune donnée de jeu n'est hardcodée dans les MonoBehaviours.
Justification : Facilite la modification de contenu (balance, recettes, zones) sans recompiler. Permet de modifier des valeurs dans l'éditeur sans toucher au code.

---

## Cross-cutting Concerns

Ces patterns s'appliquent à TOUS les systèmes et doivent être suivis par chaque implémentation.

### Error Handling

**Stratégie : Try-Catch ciblé + Graceful Degradation**

- Les erreurs **critiques** (sauvegarde corrompue, ScriptableObject manquant) sont loggées et affichent un message d'erreur explicite au joueur
- Les erreurs **récupérables** (recette non trouvée, audio absent) sont loggées et silencieuses pour le joueur (fallback gracieux)
- Pas de `try-catch` génériques qui avalent les exceptions silencieusement

```csharp
// OBLIGATOIRE : log + message clair pour les erreurs critiques
public bool LoadSave(string path)
{
    try
    {
        var json = File.ReadAllText(path);
        _saveData = JsonSerializer.Deserialize<SaveData>(json);
        return true;
    }
    catch (Exception e)
    {
        Debug.LogError($"[SaveSystem] Échec du chargement : {e.Message}");
        return false; // L'appelant gère le fallback (nouvelle partie)
    }
}

// OBLIGATOIRE : validation défensive pour les données critiques
public void PlayCard(CardData card)
{
    if (card == null)
    {
        Debug.LogError("[CombatSystem] PlayCard appelé avec une carte null");
        return;
    }
    // ...
}
```

### Logging

**Format : Unity Debug avec préfixes de système**

- Préfixe obligatoire : `[NomDuSystème]` en début de message
- Niveaux : `Debug.Log` (info), `Debug.LogWarning` (inattendu), `Debug.LogError` (erreur)
- Les logs `Debug.Log` sont strip en release via une classe wrapper :

```csharp
public static class GameLog
{
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Info(string system, string message)
        => Debug.Log($"[{system}] {message}");

    public static void Warning(string system, string message)
        => Debug.LogWarning($"[{system}] {message}");

    public static void Error(string system, string message)
        => Debug.LogError($"[{system}] {message}");
}

// Usage
GameLog.Info("CombatSystem", "Tour joueur commencé");
GameLog.Error("SaveSystem", "Fichier de sauvegarde introuvable");
```

### Configuration

**Approche : ScriptableObjects pour le balance, constantes C# pour les règles**

| Type de config | Stockage | Exemple |
|---|---|---|
| Valeurs de balance (coûts, PV) | `ScriptableObject` | `GameBalanceConfig.asset` |
| Règles fixes du jeu | Classe C# statique `GameRules` | `GameRules.MaxDeckSize = 40` |
| Préférences joueur | `PlayerPrefs` | Volume audio, langue |
| Settings plateforme | Build defines | `STEAM_BUILD`, `DEBUG_BUILD` |

```csharp
// Règles immuables du jeu — jamais modifiées en runtime
public static class GameRules
{
    public const int DeckSize = 40;
    public const int MaxHandSize = 6;
    public const int MaxDeckWeight = 15;
    public const int MaxCopiesCommon = 3;
    public const int MaxCopiesLegendary = 1;
}
```

### Event System

*Voir ADR dans Architectural Decisions.*

**Règles d'application :**
- Communication **inter-systèmes** → `ScriptableObject Event Channel`
- Communication **intra-système** → C# `event Action<T>`
- **Jamais** de `FindObjectOfType` pour communiquer entre systèmes
- Les listeners se désabonnent dans `OnDisable()` ou `OnDestroy()`

**Nommage des events :**
- ScriptableObject channels : `On[Sujet][Verbe]Event.asset` (ex. `OnCardPlayedEvent.asset`)
- C# events : `On[Verbe][Sujet]` (ex. `OnStateChanged`, `OnCardDrawn`)

### Debug Tools

**Activation : `#if UNITY_EDITOR` + flag `[SerializeField] bool _debugMode`**

```csharp
[Header("Debug (Editor Only)")]
[SerializeField] private bool _debugMode = false;

#if UNITY_EDITOR
[ContextMenu("Debug/Skip To Monster Turn")]
private void Debug_SkipToMonsterTurn() { ... }

[ContextMenu("Debug/Give All Materials")]
private void Debug_GiveAllMaterials() { ... }

[ContextMenu("Debug/Unlock All Zones")]
private void Debug_UnlockAllZones() { ... }
#endif
```

**Commandes debug disponibles (via ContextMenu Inspector) :**
- Skip to Monster Turn
- Give Materials (commun / rare / tous)
- Unlock All Zones
- Force Forge Level (1/2/3)
- Draw Specific Card

**En release :** Toutes les méthodes `#if UNITY_EDITOR` sont retirées du build.

---

## Project Structure

### Organization Pattern

**Pattern : Hybride — Par type à la racine, par domaine dans Scripts/**

Conforme aux conventions Unity (Assets/ à la racine, dossiers standards), avec une organisation par domaine fonctionnel dans le code C#. Chaque système majeur a un dossier Scripts/ dédié avec ses propres données.

### Directory Structure

```
Assets/
├── Art/
│   ├── Sprites/
│   │   ├── Cards/          # Illustrations de cartes
│   │   ├── Monsters/       # Illustrations de monstres
│   │   ├── Zones/          # Backgrounds et éléments de zones
│   │   └── UI/             # Icônes, boutons, éléments UI
│   ├── Animations/
│   ├── Materials/
│   └── Shaders/
│
├── Audio/
│   ├── Music/
│   │   ├── Zones/          # Thèmes par zone (6 fichiers)
│   │   └── Bosses/         # Thèmes de boss (6-8 fichiers)
│   └── SFX/
│       ├── Combat/
│       ├── UI/
│       └── Ambient/
│
├── Data/                   # Instances de ScriptableObjects
│   ├── Cards/
│   ├── Monsters/
│   ├── Zones/
│   ├── Recipes/
│   ├── Keywords/
│   └── Config/
│
├── Events/                 # ScriptableObject Event Channels
│   ├── Combat/
│   ├── Craft/
│   └── World/
│
├── Prefabs/
│   ├── Cards/
│   ├── Combat/
│   ├── UI/
│   └── World/
│
├── Scenes/
│   ├── Boot.unity
│   ├── MainMenu.unity
│   ├── WorldMap.unity
│   ├── Combat.unity
│   └── Forge.unity
│
├── Scripts/
│   ├── Core/               # Core.asmdef
│   │   ├── Services/       # ServiceLocator.cs + interfaces
│   │   ├── Events/         # Base classes Event Channels
│   │   ├── Rules/          # GameRules.cs (constantes)
│   │   └── GameLog.cs
│   │
│   ├── Combat/             # Gameplay.asmdef (ref: Core)
│   │   ├── States/         # CombatStateMachine, IState, DrawState,
│   │   │                   # SacrificeState, PlayState,
│   │   │                   # ReactiveWindowState, MonsterTurnState,
│   │   │                   # CombatEndState
│   │   ├── Keywords/       # IKeywordEffect, KeywordResolver,
│   │   │                   # VolKeyword, PorteeKeyword, etc.
│   │   ├── MonsterAI/      # MonsterAIController, scripts IA
│   │   └── CombatManager.cs
│   │
│   ├── Cards/
│   │   ├── Data/           # CardData.cs, CardType.cs, CardRarity.cs
│   │   └── DeckValidator.cs
│   │
│   ├── Craft/
│   │   ├── Data/           # RecipeData.cs, MaterialData.cs
│   │   ├── ForgeSystem.cs
│   │   └── CraftSystem.cs
│   │
│   ├── World/
│   │   ├── Data/           # ZoneData.cs
│   │   ├── WorldMapManager.cs
│   │   └── ZoneProgressionSystem.cs
│   │
│   ├── Save/
│   │   ├── SaveData.cs     # Modèles de sérialisation
│   │   ├── ISaveSystem.cs
│   │   └── JsonSaveSystem.cs
│   │
│   ├── Audio/              # Infrastructure.asmdef (ref: Core)
│   │   └── AudioManager.cs
│   │
│   ├── UI/                 # UI.asmdef (ref: Core, Gameplay)
│   │   ├── Combat/
│   │   ├── WorldMap/
│   │   ├── DeckBuilder/
│   │   └── Forge/
│   │
│   └── Infrastructure/
│       └── Boot/           # BootLoader.cs
│
├── UI/
│   ├── UXML/
│   │   ├── Combat.uxml
│   │   ├── WorldMap.uxml
│   │   ├── DeckBuilder.uxml
│   │   └── Forge.uxml
│   └── USS/
│       ├── Common.uss
│       └── Themes/
│
└── Tests/
    ├── EditMode/
    │   ├── Combat/
    │   ├── Cards/
    │   └── Save/
    └── PlayMode/
```

### System Location Mapping

| Système | Dossier Scripts/ | Données (ScriptableObjects) |
|---|---|---|
| Combat & État | `Scripts/Combat/` | `Data/Config/` |
| Résolution mots-clés | `Scripts/Combat/Keywords/` | `Data/Keywords/` |
| IA Monstre | `Scripts/Combat/MonsterAI/` | `Data/Monsters/` |
| Modèle carte | `Scripts/Cards/Data/` | `Data/Cards/` |
| Craft & Forge | `Scripts/Craft/` | `Data/Recipes/` |
| Monde & Zones | `Scripts/World/` | `Data/Zones/` |
| Sauvegarde | `Scripts/Save/` | — |
| UI | `Scripts/UI/` + `UI/UXML/` + `UI/USS/` | — |
| Services / Events | `Scripts/Core/` | `Events/` |

### Naming Conventions

#### Fichiers & Dossiers

| Type | Convention | Exemple |
|---|---|---|
| Scripts C# | `PascalCase` | `CombatManager.cs` |
| ScriptableObject instances | `PascalCase_Descriptif` | `Zone_Plaines.asset` |
| Scènes | `PascalCase` | `WorldMap.unity` |
| Prefabs | `PascalCase` | `CardView.prefab` |
| UXML / USS | `PascalCase` | `CombatHUD.uxml` |
| Sprites | `kebab-case` | `monster-loup-geant.png` |
| Audio | `kebab-case` | `zone-plaines-theme.ogg` |

#### Code C#

| Élément | Convention | Exemple |
|---|---|---|
| Classes | `PascalCase` | `CombatStateMachine` |
| Interfaces | `IPascalCase` | `IKeywordEffect` |
| Méthodes | `PascalCase` | `PlayCard()` |
| Champs privés | `_camelCase` | `_currentState` |
| Propriétés publiques | `PascalCase` | `CurrentMana` |
| Constantes | `PascalCase` | `GameRules.DeckSize` |
| Paramètres / locals | `camelCase` | `cardData` |

#### ScriptableObject Assets

- Cartes : `Card_[Zone]_[Nom].asset` — ex. `Card_Plaines_ChargeBrutale.asset`
- Monstres : `Monster_[Zone]_[Nom].asset` — ex. `Monster_Marais_ZombieGeant.asset`
- Recettes : `Recipe_[CarteResultat].asset`
- Event channels : `Event_On[Sujet][Verbe].asset` — ex. `Event_OnCardPlayed.asset`

### Architectural Boundaries

**Règles strictes de dépendance :**

```
Core ← Gameplay ← UI
Core ← Infrastructure
```

- `UI` peut référencer `Gameplay` et `Core`, jamais l'inverse
- `Infrastructure` (Save, Audio, Boot) référence uniquement `Core`
- `Gameplay` (Combat, Craft, World, Cards) référence uniquement `Core`
- Les systèmes Gameplay ne se référencent **pas directement entre eux** → communication via Event Channels ou Service Locator

---

## Implementation Patterns

Ces patterns garantissent une implémentation cohérente entre tous les agents IA.

### Novel Patterns

#### Pattern 1 : Hidden Recipe Discovery (Craft empirique)

**Problème :** Le joueur peut tenter de combiner n'importe quels matériaux. Si la combinaison correspond à une recette existante, la carte est craftée et la recette est "découverte". Sinon, rien ne se passe (pas d'erreur visible).

**Composants :**
- `RecipeData` (ScriptableObject) — combinaison de matériaux + carte résultante
- `CraftSystem` — cherche une recette correspondante
- `PlayerInventory` — stocke les recettes découvertes (par ID)

**Flux :**
```
Joueur sélectionne matériaux → CraftSystem.TryCraft(materials[])
  → Cherche RecipeData dont ingredients[] correspond exactement
  → Si trouvé ET joueur a les matériaux :
      - Déduire matériaux de l'inventaire
      - Créer la CardData dans l'inventaire
      - Marquer recette comme découverte dans SaveData
      - Lever Event_OnCardCrafted
  → Si non trouvé : retourner false (aucun feedback négatif au joueur)
```

**Implémentation :**
```csharp
public class CraftSystem : MonoBehaviour, ICraftSystem
{
    [SerializeField] private RecipeData[] _allRecipes;
    [SerializeField] private CardCraftedEvent _onCardCrafted;

    public CraftResult TryCraft(List<MaterialData> materials, PlayerInventory inventory)
    {
        var recipe = FindMatchingRecipe(materials);

        if (recipe == null)
            return CraftResult.NoRecipeFound;

        if (!inventory.HasMaterials(recipe.RequiredMaterials))
            return CraftResult.InsufficientMaterials;

        inventory.RemoveMaterials(recipe.RequiredMaterials);
        inventory.AddCard(recipe.ResultCard);
        inventory.DiscoverRecipe(recipe.Id);

        _onCardCrafted.Raise(recipe.ResultCard);
        return CraftResult.Success;
    }

    private RecipeData FindMatchingRecipe(List<MaterialData> materials)
        => Array.Find(_allRecipes, r => r.Matches(materials));
}

public enum CraftResult { Success, NoRecipeFound, InsufficientMaterials }
```

**Règle :** `CraftSystem` cherche toujours dans TOUTES les recettes. L'état "découvert" est uniquement dans `SaveData` pour l'affichage UI.

---

#### Pattern 2 : Keyword Resolution Chain

**Problème :** Plusieurs mots-clés interagissent lors de la résolution d'une action. L'ordre de résolution doit être déterministe et testable.

**Ordre de résolution fixe :**
1. `Éveillé` (pré-jeu de l'allié)
2. `Provocation` (vérification de ciblage)
3. `Vol` + `Portée` (vérification de ciblabilité)
4. `Instantané` (vérification de bloquabilité)
5. `Rituel(X)` (coût supplémentaire)
6. Résolution de l'effet principal
7. `Invincible` (vérification de destruction)
8. `Rampant` (résolution post-cimetière)

**Implémentation :**
```csharp
public class KeywordResolver
{
    public bool CanTarget(CardData attacker, CardData target, CombatContext ctx)
    {
        if (target.HasKeyword(Keyword.Vol) && !attacker.HasKeyword(Keyword.Portee))
            return false;

        var tauntAllies = ctx.GetAlliesWithKeyword(Keyword.Provocation);
        if (tauntAllies.Count > 0 && !tauntAllies.Contains(target))
            return false;

        return true;
    }

    public bool CanBlock(CardData action, CombatContext ctx)
        => !action.HasKeyword(Keyword.Instantane);

    public bool SurvivesDestruction(CardData card)
        => card.HasKeyword(Keyword.Invincible);

    public bool CanPlayFromGraveyard(CardData card, PlayerInventory inventory)
        => card.HasKeyword(Keyword.Rampant) && inventory.Graveyard.Contains(card);
}
```

**Règle :** `KeywordResolver` est un service pur (pas de MonoBehaviour). Il ne modifie jamais l'état — il répond uniquement à des questions booléennes. Le `CombatStateMachine` l'interroge et prend les décisions.

---

### Communication Patterns

**Inter-systèmes : ScriptableObject Event Channel**

```csharp
// TOUJOURS : s'abonner dans OnEnable, se désabonner dans OnDisable
public class CombatHUD : MonoBehaviour
{
    [SerializeField] private CardPlayedEvent _onCardPlayed;

    private void OnEnable() => _onCardPlayed.OnRaised += HandleCardPlayed;
    private void OnDisable() => _onCardPlayed.OnRaised -= HandleCardPlayed;

    private void HandleCardPlayed(CardData card) { /* update UI */ }
}
```

**Intra-système : C# event**

```csharp
public class CombatStateMachine
{
    public event Action<ICombatState> OnStateChanged;

    private void TransitionTo(ICombatState newState)
    {
        _currentState.Exit();
        _currentState = newState;
        _currentState.Enter(_context);
        OnStateChanged?.Invoke(newState);
    }
}
```

### Entity Creation Patterns

**Visuels de cartes : CardViewFactory**

```csharp
public class CardViewFactory : MonoBehaviour
{
    [SerializeField] private CardView _cardPrefab;

    public CardView CreateCardView(CardData data, Transform parent)
    {
        var view = Instantiate(_cardPrefab, parent);
        view.Initialize(data);
        return view;
    }
}
```

**Données de jeu : toujours depuis ScriptableObjects** — jamais `new CardData()` en runtime.

### State Patterns

**Interface commune à tous les états de combat :**

```csharp
public interface ICombatState
{
    void Enter(CombatContext context);
    void Update();
    void Exit();
}

public class DrawState : ICombatState
{
    public void Enter(CombatContext ctx)
    {
        int cardsToDraw = GameRules.MaxHandSize - ctx.PlayerHand.Count;
        for (int i = 0; i < cardsToDraw; i++)
            ctx.DrawCard();
    }
    public void Update() { }
    public void Exit() { }
}
```

### Data Access Patterns

```csharp
// BON : référence SO assignée dans l'Inspector
[SerializeField] private CardData _cardData;

// BON : accès via service enregistré
var saveSystem = Services.Get<ISaveSystem>();

// INTERDIT : Resources.Load ou FindObjectOfType pour les données
// var data = Resources.Load<CardData>("Cards/Foo"); // NE PAS FAIRE
```

### Consistency Rules

| Situation | Pattern obligatoire |
|---|---|
| Nouvelle donnée de jeu | ScriptableObject dans `Data/` |
| Communication entre systèmes | SO Event Channel dans `Events/` |
| Accès à un service global | `Services.Get<IService>()` |
| Nouvelle mécanique de combat | Nouvel `ICombatState` dans `Scripts/Combat/States/` |
| Nouveau mot-clé | Nouvelle classe `IKeywordEffect` + enregistrement dans `KeywordResolver` |
| Log d'information | `GameLog.Info("[Système]", "message")` |
| Log d'erreur | `GameLog.Error("[Système]", "message")` |
| Test unitaire | `Tests/EditMode/[Système]/` |

---

## Architecture Validation

### Validation Summary

| Check | Résultat | Notes |
|---|---|---|
| Compatibilité des décisions | ✅ PASS | Toutes les décisions sont compatibles entre elles |
| Couverture GDD | ✅ PASS | 11/11 systèmes couverts |
| Complétude des patterns | ✅ PASS | 8 scénarios couverts dont 2 patterns novel |
| Mapping Epics | ✅ PASS | 8 epics mappés à l'architecture |
| Complétude du document | ✅ PASS | Aucun placeholder restant |

### Coverage Report

**Systèmes couverts :** 11/11
**Patterns définis :** 5 (2 novel + 3 standards)
**Décisions documentées :** 6 + 3 ADR
**Epics mappés :** 8/8

### Validation Date

2026-04-05
