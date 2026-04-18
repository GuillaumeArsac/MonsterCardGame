---
title: 'Epic 1 — Fondations Techniques'
slug: 'epic-1-fondations-techniques'
created: '2026-04-15'
status: 'implementation-complete'
stepsCompleted: [1, 2, 3, 4]
tech_stack:
  - 'Unity 6.3 LTS (6000.3.12f1)'
  - 'C# (.NET Standard 2.1)'
  - 'URP 17.3.0'
  - 'UI Toolkit (com.unity.modules.uielements)'
  - 'Unity Test Framework 1.6.0'
  - 'System.Text.Json (inclus Unity 6)'
  - 'New Input System 1.19.0'
files_to_modify:
  - 'Assets/Scripts/Core/Core.asmdef'
  - 'Assets/Scripts/Core/GameLog.cs'
  - 'Assets/Scripts/Core/Services/IService.cs'
  - 'Assets/Scripts/Core/Services/ServiceLocator.cs'
  - 'Assets/Scripts/Core/Services/BootLoader.cs'
  - 'Assets/Scripts/Core/Events/GameEvent.cs'
  - 'Assets/Scripts/Core/Events/GameEventListener.cs'
  - 'Assets/Scripts/Core/Rules/GameRules.cs'
  - 'Assets/Scripts/Gameplay/Gameplay.asmdef'
  - 'Assets/Scripts/Gameplay/Cards/CardType.cs'
  - 'Assets/Scripts/Gameplay/Cards/CardRarity.cs'
  - 'Assets/Scripts/Gameplay/Cards/Keyword.cs'
  - 'Assets/Scripts/Gameplay/Cards/CardData.cs'
  - 'Assets/Scripts/Gameplay/Deck/DeckData.cs'
  - 'Assets/Scripts/Gameplay/Deck/DeckValidator.cs'
  - 'Assets/Scripts/UI/UI.asmdef'
  - 'Assets/Scripts/Infrastructure/Infrastructure.asmdef'
  - 'Assets/Tests/EditMode/Tests.EditMode.asmdef'
  - 'Assets/Tests/EditMode/DeckValidatorTests.cs'
code_patterns:
  - 'ScriptableObject Architecture — données de jeu comme assets SO dans Assets/Data/'
  - 'Service Locator — Services.Register<T>() au Boot, Services.Get<T>() partout'
  - 'Assembly Definitions — Core <- Gameplay <- UI, Core <- Infrastructure'
  - 'GameLog wrapper — GameLog.Info/Warning/Error("[Système]", "msg")'
  - '[SerializeField] + [Header] + [Tooltip] sur tous les champs SO'
  - 'SO Event Channels — GameEvent SO + GameEventListener MonoBehaviour'
test_patterns:
  - 'Edit Mode uniquement — Tests/EditMode/ — logique pure sans runtime Unity'
  - 'Assets de test SO réels dans Assets/Tests/TestData/ — pas de mocks'
  - 'Naming : [Méthode]_[Condition]_[RésultatAttendu]'
  - 'Assembly Tests.EditMode référence Core + Gameplay uniquement'
---

# Tech-Spec: Epic 1 — Fondations Techniques

**Created:** 2026-04-15

## Overview

### Problem Statement

Le projet Unity MonsterCardGame est à l'état initial (SampleScene + URP par défaut). Aucune structure de code, aucun modèle de données, aucune scène de jeu n'existe. Tous les Epics suivants (Combat, Craft, Monde) dépendent de cette couche fondationnelle.

### Solution

Mettre en place la structure complète du projet Unity : organisation des dossiers `Assets/`, 4 Assembly Definitions, modèles de données ScriptableObject (`CardData`, `DeckData`), 5 scènes de base enregistrées dans les Build Settings, `ServiceLocator` + `BootLoader`, utilitaires transverses (`GameRules`, `GameLog`), base des SO Event Channels, et tests unitaires Edit Mode pour `DeckValidator`.

### Scope

**In Scope:**
- Structure de dossiers `Assets/` complète (Art, Audio, Data, Events, Prefabs, Scenes, Scripts, Tests)
- 4 Assembly Definitions : `Core.asmdef`, `Gameplay.asmdef`, `UI.asmdef`, `Infrastructure.asmdef`
- `CardData` ScriptableObject avec tous les champs (nom, coût, mana généré, type, rareté, mots-clés, description)
- `DeckData` ScriptableObject avec `List<CardData>` + `DeckValidator` (40 cartes, poids max 15, copies max)
- 5 scènes vides : `Boot.unity`, `MainMenu.unity`, `Combat.unity`, `WorldMap.unity`, `Forge.unity` — enregistrées dans Build Settings
- `ServiceLocator.cs` + `IService` interface
- `BootLoader.cs` MonoBehaviour (Boot scene — initialise les services)
- `GameRules.cs` classe statique (constantes du jeu)
- `GameLog.cs` wrapper de log
- Classes de base Event Channels : `GameEvent.cs` (SO), `GameEventListener.cs` (MonoBehaviour)
- Tests Edit Mode : `DeckValidatorTests.cs`

**Out of Scope:**
- Tout gameplay fonctionnel
- Assets visuels ou audio définitifs
- `MonsterData`, `RecipeData`, `ZoneData` (Epics 2+)
- `PlayerData` SO — les constantes joueur vont dans `GameRules`
- DOTween, Steamworks.NET, Unity Localization (installés dans Epics suivants)
- UI Toolkit UXML/USS (Epics suivants)

## Context for Development

### Codebase Patterns

- **ScriptableObject Architecture** : toutes les données de jeu sont des assets SO dans `Assets/Data/`. Jamais `new CardData()` en runtime.
- **Service Locator** : les services globaux s'enregistrent via `Services.Register<IService>()` au démarrage (BootLoader) et s'accèdent via `Services.Get<IService>()`.
- **Assembly Definitions** : dépendances strictes `Core ← Gameplay ← UI`, `Core ← Infrastructure`. Jamais l'inverse.
- **GameLog** : tous les `Debug.Log` passent par `GameLog.Info/Warning/Error("[Système]", "message")`. Les Info sont `[Conditional("UNITY_EDITOR")]`.
- **Naming** : classes `PascalCase`, champs privés `_camelCase`, interfaces `IPascalCase`, assets SO `[Type]_[Zone]_[Nom].asset`, events SO `Event_On[Sujet][Verbe].asset`.
- **[SerializeField]** préféré à `public` pour l'Inspector. `[Header]` et `[Tooltip]` obligatoires sur les groupes de champs SO.
- **Pas de `FindObjectOfType`** — utiliser `Services.Get<>()`.

### Files to Reference

| File | Purpose |
| ---- | ------- |
| `_bmad-output/game-architecture.md` | Architecture complète : patterns, structure, ADRs |
| `_bmad-output/project-context.md` | Règles critiques d'implémentation pour les agents IA |
| `_bmad-output/planning-artifacts/epics.md` | Stories officielles Epic 1 avec ACs |
| `_bmad-output/gdd.md` | Valeurs de balance (rarités, poids, copies) |

### Technical Decisions

- **`DeckData`** utilise `List<CardData>` (avec doublons possibles) — plus lisible qu'un Dictionary pour Guillaume
- **Scènes** : créées vides et enregistrées dans Build Settings (pas de UIDocument placeholder)
- **`PlayerData` SO** : pas créé — les constantes joueur (PV de base, taille main) vont dans `GameRules`
- **`GameRules`** : classe C# statique avec `const int` — immuable, pas de SO
- **`Keyword`** : enum `[Flags]` pour combinaisons. `Rituel(X)` = flag `Rituel` + champ `RitualValue` dédié sur `CardData`
- **`CardData.Weight`** : calculé depuis la rareté (Commune=0, Rare=1, Légo/Unique=2) — stocké comme `int` dans le SO pour override éventuel
- **`GameRules.PlayerStartingHP`** : valeur placeholder (30) — à confirmer lors de la balance en Epic 2
- **Clean Slate confirmé** : aucun code C# existant, aucune contrainte de rétrocompatibilité
- **Packages déjà présents** dans manifest.json : test-framework 1.6.0, inputsystem 1.19.0, uielements, jsonserialize, MCP Unity — aucune installation requise pour Epic 1
- **Scènes Unity** : création manuelle dans l'éditeur Unity (les fichiers .unity ne peuvent pas être écrits en texte brut de façon fiable)

---

## Implementation Plan

### Tasks

- [ ] **Tâche 1 : Créer la structure de dossiers Assets/**
  - Action : Créer tous les dossiers vides listés ci-dessous dans `Assets/` (Unity génère les .meta automatiquement)
  - Dossiers à créer :
    ```
    Assets/Art/Sprites/Cards/
    Assets/Art/Sprites/Monsters/
    Assets/Art/Sprites/Zones/
    Assets/Art/Sprites/UI/
    Assets/Art/Animations/
    Assets/Art/Materials/
    Assets/Art/Shaders/
    Assets/Audio/Music/Zones/
    Assets/Audio/Music/Bosses/
    Assets/Audio/SFX/Combat/
    Assets/Audio/SFX/UI/
    Assets/Audio/SFX/Ambient/
    Assets/Data/Cards/
    Assets/Data/Monsters/
    Assets/Data/Zones/
    Assets/Data/Recipes/
    Assets/Data/Keywords/
    Assets/Data/Config/
    Assets/Events/Combat/
    Assets/Events/Craft/
    Assets/Events/World/
    Assets/Prefabs/Cards/
    Assets/Prefabs/Combat/
    Assets/Prefabs/UI/
    Assets/Prefabs/World/
    Assets/Scripts/Core/Services/
    Assets/Scripts/Core/Events/
    Assets/Scripts/Core/Rules/
    Assets/Scripts/Gameplay/Cards/
    Assets/Scripts/Gameplay/Deck/
    Assets/Scripts/UI/
    Assets/Scripts/Infrastructure/
    Assets/Tests/EditMode/
    Assets/Tests/TestData/
    ```
  - Note : Créer un fichier `.gitkeep` vide dans chaque dossier vide (Art, Audio, Data, Events, Prefabs, TestData) pour que Git les suive.

- [ ] **Tâche 2 : Créer les 4 Assembly Definitions**
  - Fichier : `Assets/Scripts/Core/Core.asmdef`
    ```json
    {
        "name": "MonsterCardGame.Core",
        "rootNamespace": "MonsterCardGame.Core",
        "references": [],
        "includePlatforms": [],
        "excludePlatforms": [],
        "allowUnsafeCode": false,
        "overrideReferences": false,
        "precompiledReferences": [],
        "autoReferenced": true,
        "defineConstraints": [],
        "versionDefines": [],
        "noEngineReferences": false
    }
    ```
  - Fichier : `Assets/Scripts/Gameplay/Gameplay.asmdef`
    ```json
    {
        "name": "MonsterCardGame.Gameplay",
        "rootNamespace": "MonsterCardGame.Gameplay",
        "references": ["MonsterCardGame.Core"],
        "includePlatforms": [],
        "excludePlatforms": [],
        "allowUnsafeCode": false,
        "overrideReferences": false,
        "precompiledReferences": [],
        "autoReferenced": true,
        "defineConstraints": [],
        "versionDefines": [],
        "noEngineReferences": false
    }
    ```
  - Fichier : `Assets/Scripts/UI/UI.asmdef`
    ```json
    {
        "name": "MonsterCardGame.UI",
        "rootNamespace": "MonsterCardGame.UI",
        "references": ["MonsterCardGame.Core", "MonsterCardGame.Gameplay"],
        "includePlatforms": [],
        "excludePlatforms": [],
        "allowUnsafeCode": false,
        "overrideReferences": false,
        "precompiledReferences": [],
        "autoReferenced": true,
        "defineConstraints": [],
        "versionDefines": [],
        "noEngineReferences": false
    }
    ```
  - Fichier : `Assets/Scripts/Infrastructure/Infrastructure.asmdef`
    ```json
    {
        "name": "MonsterCardGame.Infrastructure",
        "rootNamespace": "MonsterCardGame.Infrastructure",
        "references": ["MonsterCardGame.Core"],
        "includePlatforms": [],
        "excludePlatforms": [],
        "allowUnsafeCode": false,
        "overrideReferences": false,
        "precompiledReferences": [],
        "autoReferenced": true,
        "defineConstraints": [],
        "versionDefines": [],
        "noEngineReferences": false
    }
    ```

- [ ] **Tâche 3 : Créer `GameLog.cs`**
  - Fichier : `Assets/Scripts/Core/GameLog.cs`
  - Namespace : `MonsterCardGame.Core`
  - Contenu :
    ```csharp
    namespace MonsterCardGame.Core
    {
        public static class GameLog
        {
            [System.Diagnostics.Conditional("UNITY_EDITOR")]
            public static void Info(string system, string message)
                => UnityEngine.Debug.Log($"[{system}] {message}");

            public static void Warning(string system, string message)
                => UnityEngine.Debug.LogWarning($"[{system}] {message}");

            public static void Error(string system, string message)
                => UnityEngine.Debug.LogError($"[{system}] {message}");
        }
    }
    ```

- [ ] **Tâche 4 : Créer `GameRules.cs`**
  - Fichier : `Assets/Scripts/Core/Rules/GameRules.cs`
  - Namespace : `MonsterCardGame.Core`
  - Contenu :
    ```csharp
    namespace MonsterCardGame.Core
    {
        public static class GameRules
        {
            // Construction du deck
            public const int DeckSize        = 40;
            public const int MaxDeckWeight   = 15;
            public const int MaxHandSize     = 6;
            public const int MaxCopiesCommon    = 3;
            public const int MaxCopiesRare      = 3;
            public const int MaxCopiesLegendary = 1;
            public const int MaxCopiesUnique    = 1;

            // Poids par rareté (Commune=0, Rare=1, Légendaire=2, Unique=2)
            public const int WeightCommon    = 0;
            public const int WeightRare      = 1;
            public const int WeightLegendary = 2;
            public const int WeightUnique    = 2;

            // Combat — placeholder à confirmer lors du balance Epic 2
            public const int PlayerStartingHP = 30;
        }
    }
    ```

- [ ] **Tâche 5 : Créer `IService.cs` et `ServiceLocator.cs`**
  - Fichier : `Assets/Scripts/Core/Services/IService.cs`
    ```csharp
    namespace MonsterCardGame.Core.Services
    {
        public interface IService { }
    }
    ```
  - Fichier : `Assets/Scripts/Core/Services/ServiceLocator.cs`
    ```csharp
    using System;
    using System.Collections.Generic;

    namespace MonsterCardGame.Core.Services
    {
        public static class Services
        {
            private static readonly Dictionary<Type, IService> _registry = new();

            public static void Register<T>(T service) where T : IService
            {
                _registry[typeof(T)] = service;
                GameLog.Info("Services", $"Enregistré : {typeof(T).Name}");
            }

            public static T Get<T>() where T : IService
            {
                if (_registry.TryGetValue(typeof(T), out var service))
                    return (T)service;

                GameLog.Error("Services", $"Service non enregistré : {typeof(T).Name}");
                return default;
            }

            public static bool Has<T>() where T : IService
                => _registry.ContainsKey(typeof(T));

            public static void Clear() => _registry.Clear();
        }
    }
    ```

- [ ] **Tâche 6 : Créer `BootLoader.cs`**
  - Fichier : `Assets/Scripts/Core/Services/BootLoader.cs`
  - Namespace : `MonsterCardGame.Core.Services`
  - Note : MonoBehaviour à attacher à un GameObject "BootLoader" dans Boot.unity
    ```csharp
    using UnityEngine;
    using UnityEngine.SceneManagement;

    namespace MonsterCardGame.Core.Services
    {
        public class BootLoader : MonoBehaviour
        {
            [Header("Boot Configuration")]
            [SerializeField, Tooltip("Nom de la scène à charger après l'initialisation")]
            private string _nextScene = "MainMenu";

            private void Awake()
            {
                RegisterServices();
            }

            private void Start()
            {
                GameLog.Info("BootLoader", $"Boot terminé — chargement de {_nextScene}");
                SceneManager.LoadScene(_nextScene);
            }

            private void RegisterServices()
            {
                // Les services seront ajoutés ici au fil des Epics :
                // Epic 4 : Services.Register<ISaveSystem>(new JsonSaveSystem());
                // Epic 2 : Services.Register<IAudioManager>(audioManager);
                GameLog.Info("BootLoader", "Services initialisés");
            }
        }
    }
    ```

- [ ] **Tâche 7 : Créer `GameEvent.cs` et `GameEventListener.cs`**
  - Fichier : `Assets/Scripts/Core/Events/GameEvent.cs`
    ```csharp
    using System.Collections.Generic;
    using UnityEngine;

    namespace MonsterCardGame.Core.Events
    {
        [CreateAssetMenu(menuName = "MonsterCardGame/Events/Game Event")]
        public class GameEvent : ScriptableObject
        {
            [Header("Debug")]
            [SerializeField, Tooltip("Loguer chaque déclenchement dans l'éditeur")]
            private bool _debugLog = false;

            private readonly List<GameEventListener> _listeners = new();

            public void Raise()
            {
                if (_debugLog)
                    GameLog.Info("GameEvent", $"{name} déclenché ({_listeners.Count} listeners)");

                for (int i = _listeners.Count - 1; i >= 0; i--)
                    _listeners[i].OnEventRaised();
            }

            public void RegisterListener(GameEventListener listener)
            {
                if (!_listeners.Contains(listener))
                    _listeners.Add(listener);
            }

            public void UnregisterListener(GameEventListener listener)
                => _listeners.Remove(listener);
        }
    }
    ```
  - Fichier : `Assets/Scripts/Core/Events/GameEventListener.cs`
    ```csharp
    using UnityEngine;
    using UnityEngine.Events;

    namespace MonsterCardGame.Core.Events
    {
        public class GameEventListener : MonoBehaviour
        {
            [Header("Event Channel")]
            [SerializeField, Tooltip("L'événement SO à écouter")]
            private GameEvent _event;

            [Header("Response")]
            [SerializeField, Tooltip("Actions déclenchées à la réception de l'événement")]
            private UnityEvent _response;

            private void OnEnable() => _event?.RegisterListener(this);
            private void OnDisable() => _event?.UnregisterListener(this);

            public void OnEventRaised() => _response?.Invoke();
        }
    }
    ```

- [ ] **Tâche 8 : Créer les enums `CardType`, `CardRarity`, `Keyword`**
  - Fichier : `Assets/Scripts/Gameplay/Cards/CardType.cs`
    ```csharp
    namespace MonsterCardGame.Gameplay.Cards
    {
        public enum CardType
        {
            Action,
            Blocage,
            Allié,
            Équipement,
            Réaction
        }
    }
    ```
  - Fichier : `Assets/Scripts/Gameplay/Cards/CardRarity.cs`
    ```csharp
    namespace MonsterCardGame.Gameplay.Cards
    {
        public enum CardRarity
        {
            Commune,
            Rare,
            Légendaire,
            Unique
        }
    }
    ```
  - Fichier : `Assets/Scripts/Gameplay/Cards/Keyword.cs`
    ```csharp
    using System;

    namespace MonsterCardGame.Gameplay.Cards
    {
        /// <summary>
        /// Mots-clés combinables par flags. Rituel(X) utilise le flag Rituel
        /// + le champ CardData.RitualValue pour la valeur de X.
        /// </summary>
        [Flags]
        public enum Keyword
        {
            None        = 0,
            Vol         = 1 << 0,
            Portée      = 1 << 1,
            Instantané  = 1 << 2,
            Provocation = 1 << 3,
            Éveillé     = 1 << 4,
            Rituel      = 1 << 5,
            Invincible  = 1 << 6,
            Rampant     = 1 << 7
        }
    }
    ```

- [ ] **Tâche 9 : Créer `CardData.cs`**
  - Fichier : `Assets/Scripts/Gameplay/Cards/CardData.cs`
  - Namespace : `MonsterCardGame.Gameplay.Cards`
    ```csharp
    using UnityEngine;
    using MonsterCardGame.Core;

    namespace MonsterCardGame.Gameplay.Cards
    {
        [CreateAssetMenu(menuName = "MonsterCardGame/Cards/Card Data")]
        public class CardData : ScriptableObject
        {
            [Header("Identité")]
            [SerializeField, Tooltip("Nom affiché sur la carte")]
            private string _cardName;

            [SerializeField, Tooltip("Description de l'effet de la carte")]
            [TextArea(2, 4)]
            private string _description;

            [Header("Coûts")]
            [SerializeField, Tooltip("Coût en mana pour jouer la carte")]
            private int _manaCost;

            [SerializeField, Tooltip("Mana généré si la carte est sacrifiée")]
            private int _manaGenerated;

            [Header("Classification")]
            [SerializeField, Tooltip("Type de carte")]
            private CardType _cardType;

            [SerializeField, Tooltip("Rareté de la carte (détermine le poids dans le deck)")]
            private CardRarity _rarity;

            [Header("Mots-clés")]
            [SerializeField, Tooltip("Mots-clés actifs (combinables via flags)")]
            private Keyword _keywords;

            [SerializeField, Tooltip("Valeur X du mot-clé Rituel(X). Ignoré si Rituel absent.")]
            private int _ritualValue;

            [Header("Combat — Alliés uniquement")]
            [SerializeField, Tooltip("Points d'attaque")]
            private int _attack;

            [SerializeField, Tooltip("Points de défense")]
            private int _defense;

            // Propriétés publiques
            public string CardName      => _cardName;
            public string Description   => _description;
            public int    ManaCost      => _manaCost;
            public int    ManaGenerated => _manaGenerated;
            public CardType  CardType   => _cardType;
            public CardRarity Rarity    => _rarity;
            public Keyword Keywords     => _keywords;
            public int    RitualValue   => _ritualValue;
            public int    Attack        => _attack;
            public int    Defense       => _defense;

            public bool HasKeyword(Keyword keyword) => (_keywords & keyword) != 0;

            /// <summary>Poids de la carte dans le deck, dérivé de sa rareté.</summary>
            public int Weight => _rarity switch
            {
                CardRarity.Commune    => GameRules.WeightCommon,
                CardRarity.Rare       => GameRules.WeightRare,
                CardRarity.Légendaire => GameRules.WeightLegendary,
                CardRarity.Unique     => GameRules.WeightUnique,
                _                     => 0
            };
        }
    }
    ```

- [ ] **Tâche 10 : Créer `DeckData.cs`**
  - Fichier : `Assets/Scripts/Gameplay/Deck/DeckData.cs`
  - Note : `AddCard`/`RemoveCard` sont exposés ici car le DeckBuilder (Epic 3) en aura besoin. Pas une abstraction prématurée.
    ```csharp
    using System.Collections.Generic;
    using UnityEngine;
    using MonsterCardGame.Gameplay.Cards;

    namespace MonsterCardGame.Gameplay.Deck
    {
        [CreateAssetMenu(menuName = "MonsterCardGame/Deck/Deck Data")]
        public class DeckData : ScriptableObject
        {
            [Header("Cartes")]
            [SerializeField, Tooltip("Liste des cartes (doublons autorisés, représentent des copies)")]
            private List<CardData> _cards = new();

            public IReadOnlyList<CardData> Cards => _cards;

            public void AddCard(CardData card)
            {
                if (card != null) _cards.Add(card);
            }

            public void RemoveCard(CardData card)
                => _cards.Remove(card);

            public void Clear()
                => _cards.Clear();
        }
    }
    ```

- [ ] **Tâche 11 : Créer `DeckValidator.cs`**
  - Fichier : `Assets/Scripts/Gameplay/Deck/DeckValidator.cs`
    ```csharp
    using System.Collections.Generic;
    using MonsterCardGame.Core;
    using MonsterCardGame.Gameplay.Cards;

    namespace MonsterCardGame.Gameplay.Deck
    {
        public static class DeckValidator
        {
            /// <summary>Valide les 3 règles de construction : taille, poids, copies.</summary>
            public static bool Validate(DeckData deck)
            {
                if (deck == null)
                {
                    GameLog.Error("DeckValidator", "Deck null passé à Validate");
                    return false;
                }
                return HasCorrectSize(deck)
                    && IsWithinWeightLimit(deck)
                    && RespectsCopyLimits(deck);
            }

            public static bool HasCorrectSize(DeckData deck)
                => deck.Cards.Count == GameRules.DeckSize;

            public static bool IsWithinWeightLimit(DeckData deck)
                => GetTotalWeight(deck) <= GameRules.MaxDeckWeight;

            public static bool RespectsCopyLimits(DeckData deck)
            {
                var copyCount = new Dictionary<CardData, int>();
                foreach (var card in deck.Cards)
                {
                    if (card == null) continue;
                    copyCount.TryGetValue(card, out int count);
                    copyCount[card] = count + 1;
                }
                foreach (var (card, count) in copyCount)
                {
                    if (count > GetMaxCopies(card)) return false;
                }
                return true;
            }

            public static int GetTotalWeight(DeckData deck)
            {
                int total = 0;
                foreach (var card in deck.Cards)
                    if (card != null) total += card.Weight;
                return total;
            }

            public static int GetCardCopies(DeckData deck, CardData card)
            {
                int count = 0;
                foreach (var c in deck.Cards)
                    if (c == card) count++;
                return count;
            }

            private static int GetMaxCopies(CardData card) => card.Rarity switch
            {
                CardRarity.Commune    => GameRules.MaxCopiesCommon,
                CardRarity.Rare       => GameRules.MaxCopiesRare,
                CardRarity.Légendaire => GameRules.MaxCopiesLegendary,
                CardRarity.Unique     => GameRules.MaxCopiesUnique,
                _                     => GameRules.MaxCopiesCommon
            };
        }
    }
    ```

- [ ] **Tâche 12 : Créer l'Assembly Definition des tests et `DeckValidatorTests.cs`**
  - Fichier : `Assets/Tests/EditMode/Tests.EditMode.asmdef`
    ```json
    {
        "name": "MonsterCardGame.Tests.EditMode",
        "rootNamespace": "MonsterCardGame.Tests.EditMode",
        "references": [
            "MonsterCardGame.Core",
            "MonsterCardGame.Gameplay",
            "UnityEngine.TestRunner",
            "UnityEditor.TestRunner"
        ],
        "includePlatforms": ["Editor"],
        "excludePlatforms": [],
        "allowUnsafeCode": false,
        "overrideReferences": true,
        "precompiledReferences": ["nunit.framework.dll"],
        "autoReferenced": false,
        "defineConstraints": ["UNITY_INCLUDE_TESTS"],
        "versionDefines": [],
        "noEngineReferences": false
    }
    ```
  - Fichier : `Assets/Tests/EditMode/DeckValidatorTests.cs`
    ```csharp
    using NUnit.Framework;
    using UnityEngine;
    using UnityEditor;
    using MonsterCardGame.Gameplay.Cards;
    using MonsterCardGame.Gameplay.Deck;

    namespace MonsterCardGame.Tests.EditMode
    {
        public class DeckValidatorTests
        {
            // Crée une CardData avec une rareté précise via SerializedObject (Edit Mode)
            private static CardData CreateCard(CardRarity rarity = CardRarity.Commune)
            {
                var card = ScriptableObject.CreateInstance<CardData>();
                var so = new SerializedObject(card);
                so.FindProperty("_rarity").enumValueIndex = (int)rarity;
                so.ApplyModifiedPropertiesWithoutUndo();
                return card;
            }

            // Crée un DeckData peuplé avec le nombre demandé de cartes par rareté
            private static DeckData CreateDeck(int communes = 0, int rares = 0, int legendaires = 0, int uniques = 0)
            {
                var deck = ScriptableObject.CreateInstance<DeckData>();
                for (int i = 0; i < communes;    i++) deck.AddCard(CreateCard(CardRarity.Commune));
                for (int i = 0; i < rares;       i++) deck.AddCard(CreateCard(CardRarity.Rare));
                for (int i = 0; i < legendaires; i++) deck.AddCard(CreateCard(CardRarity.Légendaire));
                for (int i = 0; i < uniques;     i++) deck.AddCard(CreateCard(CardRarity.Unique));
                return deck;
            }

            // --- Taille du deck ---

            [Test]
            public void Validate_ExactlyFortyCommonCards_ReturnsTrue()
            {
                var deck = CreateDeck(communes: 40);
                Assert.IsTrue(DeckValidator.Validate(deck));
            }

            [Test]
            public void HasCorrectSize_ThirtyNineCards_ReturnsFalse()
            {
                var deck = CreateDeck(communes: 39);
                Assert.IsFalse(DeckValidator.HasCorrectSize(deck));
            }

            [Test]
            public void HasCorrectSize_FortyOneCards_ReturnsFalse()
            {
                var deck = CreateDeck(communes: 41);
                Assert.IsFalse(DeckValidator.HasCorrectSize(deck));
            }

            // --- Poids du deck ---

            [Test]
            public void IsWithinWeightLimit_FortyCommonCards_ReturnsTrue()
            {
                // Communes poids=0, total=0 ≤ 15
                var deck = CreateDeck(communes: 40);
                Assert.IsTrue(DeckValidator.IsWithinWeightLimit(deck));
            }

            [Test]
            public void IsWithinWeightLimit_FifteenRareCards_ReturnsTrue()
            {
                // 15 Rares × poids 1 = 15 = limite exacte
                var deck = CreateDeck(communes: 25, rares: 15);
                Assert.IsTrue(DeckValidator.IsWithinWeightLimit(deck));
            }

            [Test]
            public void IsWithinWeightLimit_SixteenRareCards_ReturnsFalse()
            {
                // 16 Rares × poids 1 = 16 > 15
                var deck = CreateDeck(communes: 24, rares: 16);
                Assert.IsFalse(DeckValidator.IsWithinWeightLimit(deck));
            }

            [Test]
            public void GetTotalWeight_TenRareCards_ReturnsTen()
            {
                var deck = CreateDeck(communes: 30, rares: 10);
                Assert.AreEqual(10, DeckValidator.GetTotalWeight(deck));
            }

            // --- Copies max ---

            [Test]
            public void RespectsCopyLimits_ThreeCopiesOfSameCommon_ReturnsTrue()
            {
                var deck = ScriptableObject.CreateInstance<DeckData>();
                var card = CreateCard(CardRarity.Commune);
                for (int i = 0; i < 3;  i++) deck.AddCard(card);
                for (int i = 0; i < 37; i++) deck.AddCard(CreateCard());
                Assert.IsTrue(DeckValidator.RespectsCopyLimits(deck));
            }

            [Test]
            public void RespectsCopyLimits_FourCopiesOfSameCommon_ReturnsFalse()
            {
                var deck = ScriptableObject.CreateInstance<DeckData>();
                var card = CreateCard(CardRarity.Commune);
                for (int i = 0; i < 4;  i++) deck.AddCard(card);
                for (int i = 0; i < 36; i++) deck.AddCard(CreateCard());
                Assert.IsFalse(DeckValidator.RespectsCopyLimits(deck));
            }

            [Test]
            public void RespectsCopyLimits_TwoCopiesOfLegendary_ReturnsFalse()
            {
                var deck = ScriptableObject.CreateInstance<DeckData>();
                var legendary = CreateCard(CardRarity.Légendaire);
                deck.AddCard(legendary);
                deck.AddCard(legendary);
                for (int i = 0; i < 38; i++) deck.AddCard(CreateCard());
                Assert.IsFalse(DeckValidator.RespectsCopyLimits(deck));
            }

            [Test]
            public void RespectsCopyLimits_OneCopyOfLegendary_ReturnsTrue()
            {
                var deck = ScriptableObject.CreateInstance<DeckData>();
                deck.AddCard(CreateCard(CardRarity.Légendaire));
                for (int i = 0; i < 39; i++) deck.AddCard(CreateCard());
                Assert.IsTrue(DeckValidator.RespectsCopyLimits(deck));
            }
        }
    }
    ```

- [ ] **Tâche 13 : Créer les 5 scènes et configurer le Build Settings** *(étape manuelle dans Unity Editor)*
  - Ouvrir Unity Editor
  - Créer les scènes : `File > New Scene > Basic (Built-in)` × 5, sauvegarder dans `Assets/Scenes/` :
    - `Boot.unity`
    - `MainMenu.unity`
    - `Combat.unity`
    - `WorldMap.unity`
    - `Forge.unity`
  - Supprimer `SampleScene.unity` (obsolète)
  - Ouvrir `File > Build Settings` et ajouter les 5 scènes dans cet ordre :
    1. `Scenes/Boot.unity` (index 0 — obligatoire)
    2. `Scenes/MainMenu.unity`
    3. `Scenes/Combat.unity`
    4. `Scenes/WorldMap.unity`
    5. `Scenes/Forge.unity`
  - Dans `Boot.unity` : créer un GameObject "BootLoader", y attacher le composant `BootLoader.cs`

---

### Acceptance Criteria

- [ ] **AC 1 — Structure** : Given le projet Unity ouvert, when on navigue dans le Project view, then les dossiers `Art/`, `Audio/`, `Data/`, `Events/`, `Prefabs/`, `Scripts/Core/`, `Scripts/Gameplay/`, `Scripts/UI/`, `Scripts/Infrastructure/`, `Tests/EditMode/`, `Tests/TestData/` existent tous sous `Assets/`.

- [ ] **AC 2 — Assembly Definitions** : Given les 4 .asmdef créés, when Unity recompile, then la console ne montre aucune erreur de compilation et les 4 assemblies apparaissent dans `Edit > Project Settings > Player > Other Settings > Assembly Definitions`.

- [ ] **AC 3 — CardData (Story 1.1)** : Given l'éditeur Unity, when on fait `Assets > Create > MonsterCardGame > Cards > Card Data`, then un asset CardData est créé avec tous les champs visibles dans l'Inspector (Identité, Coûts, Classification, Mots-clés, Combat).

- [ ] **AC 4 — CardData.Weight** : Given une CardData de rareté Rare, when on accède à `card.Weight`, then la valeur retournée est `1` (conforme à `GameRules.WeightRare`).

- [ ] **AC 5 — CardData.HasKeyword** : Given une CardData avec `_keywords = Keyword.Vol | Keyword.Portée`, when on appelle `HasKeyword(Keyword.Vol)`, then le résultat est `true` ; when on appelle `HasKeyword(Keyword.Invincible)`, then le résultat est `false`.

- [ ] **AC 6 — DeckData (Story 1.2)** : Given un DeckData SO créé via `Assets > Create > MonsterCardGame > Deck > Deck Data`, when on appelle `AddCard(card)` puis `Cards`, then la carte apparaît dans la liste.

- [ ] **AC 7 — DeckValidator : taille** : Given un DeckData avec 40 cartes Communes, when `DeckValidator.Validate(deck)`, then `true`. Given 39 cartes, then `false`. Given 41 cartes, then `false`.

- [ ] **AC 8 — DeckValidator : poids** : Given un DeckData avec 25 Communes + 15 Rares (poids total = 15), when `DeckValidator.IsWithinWeightLimit(deck)`, then `true`. Given 16 Rares + 24 Communes (poids = 16), then `false`.

- [ ] **AC 9 — DeckValidator : copies** : Given un DeckData contenant 4 copies de la même CardData Commune, when `DeckValidator.RespectsCopyLimits(deck)`, then `false`. Given 3 copies, then `true`. Given 2 copies d'une Légendaire, then `false`.

- [ ] **AC 10 — Tests Edit Mode passent** : Given le projet compilé, when on lance les tests via `Window > General > Test Runner > Edit Mode > Run All`, then tous les tests `DeckValidatorTests` passent au vert.

- [ ] **AC 11 — ServiceLocator** : Given un appel `Services.Register<IService>(impl)` suivi de `Services.Get<IService>()`, then l'instance enregistrée est retournée. Given un type non enregistré, when `Services.Get<T>()`, then `null` est retourné et une erreur est loguée.

- [ ] **AC 12 — Scènes (Story 1.3)** : Given le Build Settings ouvert, when on vérifie la liste des scènes, then `Boot.unity` est à l'index 0 et les 4 autres scènes y figurent. Given Play mode dans Boot.unity, when la scène démarre, then `BootLoader` charge automatiquement `MainMenu.unity`.

- [ ] **AC 13 — GameLog** : Given du code appelant `GameLog.Info("Test", "msg")` dans l'éditeur, when le code s'exécute en mode Play dans l'éditeur, then le message apparaît dans la console Unity. Given une build release, then les appels `GameLog.Info` sont strip (pas de logs Info en release).

## Additional Context

### Dependencies

- Aucune dépendance externe — tous les packages nécessaires sont déjà dans `Packages/manifest.json`
- `com.unity.test-framework` 1.6.0 ✅ déjà présent
- `com.unity.modules.uielements` 1.0.0 ✅ déjà présent
- `System.Text.Json` ✅ inclus dans Unity 6 (.NET Standard 2.1)

### Testing Strategy

- **Edit Mode uniquement** — `Assets/Tests/EditMode/DeckValidatorTests.cs`
- Les instances SO créées via `ScriptableObject.CreateInstance<T>()` sont valides en Edit Mode sans fichier .asset
- Les champs `[SerializeField]` sont peuplés via `UnityEditor.SerializedObject` dans les tests (pattern standard Unity Edit Mode)
- 10 tests couvrant : taille exacte, taille invalide (±1), poids limite, poids dépassé, copies max par rareté
- Lancement : `Window > General > Test Runner > Edit Mode > Run All`

### Notes

- **Boot.unity index 0 obligatoire** : `SceneManager.LoadScene("MainMenu")` dans `BootLoader.Start()` suppose que MainMenu est bien enregistrée dans Build Settings
- **Keyword.Rituel** : le flag indique la présence du mot-clé ; `CardData.RitualValue` contient la valeur X. Si `Rituel` est absent du flags, `RitualValue` est ignoré.
- **GameRules.PlayerStartingHP = 30** : valeur placeholder — à ajuster lors du balance combat en Epic 2
- **MaxCopiesRare = 3** : même limite que Commune (confirmé dans le GDD — seules Légendaire et Unique sont limitées à 1)
- **Caractères accentués dans les enums** (`Légendaire`, `Éveillé`, `Équipement`) : valides en C# mais à surveiller si besoin de sérialisation JSON future — documenter si problème en Epic 4
