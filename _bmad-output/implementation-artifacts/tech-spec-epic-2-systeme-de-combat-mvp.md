---
title: 'Epic 2 — Système de Combat (MVP)'
slug: 'epic-2-systeme-de-combat-mvp'
created: '2026-04-15'
status: 'completed'
stepsCompleted: [1, 2, 3, 4, 5, 6]
tech_stack:
  - 'Unity 6.3 LTS (6000.3.12f1)'
  - 'C# (.NET Standard 2.1)'
  - 'UI Toolkit (UXML/USS)'
  - 'Unity Input System 1.19.0'
  - 'Unity Test Framework 1.6.0'
files_to_modify:
  # Nouveau — Gameplay.asmdef
  - 'Assets/Scripts/Gameplay/Combat/CombatManager.cs'
  - 'Assets/Scripts/Gameplay/Combat/CombatContext.cs'
  - 'Assets/Scripts/Gameplay/Combat/AlliedInstance.cs'
  - 'Assets/Scripts/Gameplay/Combat/States/ICombatState.cs'
  - 'Assets/Scripts/Gameplay/Combat/States/DrawState.cs'
  - 'Assets/Scripts/Gameplay/Combat/States/SacrificeState.cs'
  - 'Assets/Scripts/Gameplay/Combat/States/PlayState.cs'
  - 'Assets/Scripts/Gameplay/Combat/States/ReactiveWindowState.cs'
  - 'Assets/Scripts/Gameplay/Combat/States/MonsterTurnState.cs'
  - 'Assets/Scripts/Gameplay/Combat/States/CombatEndState.cs'
  - 'Assets/Scripts/Gameplay/Combat/Keywords/IKeywordResolver.cs'
  - 'Assets/Scripts/Gameplay/Combat/Keywords/KeywordResolver.cs'
  - 'Assets/Scripts/Gameplay/Combat/MonsterAI/MonsterAIController.cs'
  - 'Assets/Scripts/Gameplay/Combat/Data/MonsterData.cs'
  - 'Assets/Scripts/Gameplay/Combat/Data/IBossPassive.cs'
  # Nouveau — UI.asmdef
  - 'Assets/Scripts/UI/Combat/CombatBoardUI.cs'
  - 'Assets/Scripts/UI/Combat/CardView.cs'
  # Nouveau — Assets UXML/USS
  - 'Assets/UI/Combat.uxml'
  - 'Assets/UI/Combat.uss'
  # Modifié
  - 'Assets/Scripts/Core/Services/BootLoader.cs'
  # Tests
  - 'Assets/Tests/EditMode/KeywordResolverTests.cs'
code_patterns:
  - 'CombatStateMachine : état courant stocké dans CombatManager, transitions explicites'
  - 'ICombatState : Enter(CombatContext), Update(CombatContext), Exit(CombatContext)'
  - 'CombatContext : classe C# pure passée à chaque état — jamais sérialisée (ADR-001)'
  - 'AlliedInstance : classe C# pure — CardData ref + HP/ATK/DEF courants + IsSleeping'
  - 'KeywordResolver : IService pur C# enregistré via ServiceLocator dans BootLoader'
  - 'MonsterData : ScriptableObject SO dans Assets/Data/Monsters/'
  - 'IBossPassive : interface vide — implémentations dans Epics 5-7'
  - 'CardView : VisualElement custom — placeholder Rectangle + Label dans UI Toolkit'
  - 'Drag & drop : MouseDown/MouseMove/MouseUp events sur VisualElement'
test_patterns:
  - 'Edit Mode — KeywordResolverTests avec AlliedInstance créées en runtime (pas de SO)'
  - 'CardData créées via ScriptableObject.CreateInstance + SerializedObject pour les champs'
  - 'Naming : [Méthode]_[Condition]_[RésultatAttendu]'
---

# Tech-Spec: Epic 2 — Système de Combat (MVP)

**Created:** 2026-04-15

## Overview

### Problem Statement

La scène `Combat.unity` est vide. Aucun système de combat, aucun modèle de monstre, aucune logique de tour n'existent. Epic 2 est le cœur jouable du projet — sans lui, aucun autre Epic ne peut être testé en contexte réel.

### Solution

Implémenter la `CombatStateMachine` (6 états explicites), le `KeywordResolver` (service pur C# — ADR-002), `MonsterData` ScriptableObject avec structure passive vide, `MonsterAIController`, la UI Toolkit du plateau (6 zones avec placeholders visuels), et 2 decks hardcodés (joueur + 1 monstre de test) pour rendre un combat jouable de bout en bout.

### Scope

**In Scope:**
- `ICombatState` + `IEnterState` / `IExitState` interfaces
- 6 états : `DrawState`, `SacrificeState`, `PlayState`, `ReactiveWindowState`, `MonsterTurnState`, `CombatEndState`
- `CombatManager` MonoBehaviour (orchestrateur, dans Combat.unity)
- `CombatContext` class (état runtime partagé : main, deck, cimetière, PV, mana, alliés)
- `AlliedInstance` class (état runtime d'un Allié : CardData ref + HP courant + statut sommeil/actif)
- `MonsterData` ScriptableObject (HP, deck scripté `List<CardData>`, structure passive vide `IBossPassive`)
- `KeywordResolver` service pur C# (8 mots-clés) + enregistrement via ServiceLocator
- `MonsterAIController` (pioche et joue depuis deck scripté, respecte Vol/Provocation)
- UI Toolkit : `Combat.uxml` + `Combat.uss` — 6 zones visuelles placeholder + zone Main + affichage PV/mana
- `CardView` VisualElement custom (carte placeholder dans la main)
- Drag & drop Input System pour jouer/sacrifier des cartes
- Bouton "Passer" dans `ReactiveWindowState` (pas de timer — jeu purement au tour par tour)
- 1 deck joueur hardcodé + 1 deck monstre hardcodé (CardData assets dans `Assets/Data/`)
- `CombatEndState` : affichage victoire/défaite basique
- Tests Edit Mode : `KeywordResolverTests`

**Out of Scope:**
- Animations de cartes DOTween (Epic 8)
- Audio — musique et SFX (Epic 8)
- `JsonSaveSystem` et sauvegarde post-combat (Epic 4)
- Verrouillage temporaire post-défaite (Epic 4)
- Drops de matériaux après victoire (Epic 3)
- `DeckBuilder` UI jouable (Epic 3)
- Localisation FR/EN (Epic 8)
- Contenu de la passive boss (Epic 5-7) — structure `IBossPassive` vide seulement
- Navigation vers d'autres scènes depuis Combat (Epic 4)

## Context for Development

### Codebase Patterns

- **Epic 1 déjà implémenté** : `CardData`, `CardType`, `CardRarity`, `Keyword` (enum sans accents), `DeckData`, `DeckValidator`, `GameRules`, `GameLog`, `ServiceLocator`, `GameEvent/Listener` sont disponibles dans les assemblies `Core` et `Gameplay`.
- **Enum Keyword sans accents** : `Portee`, `Instantane`, `Eveille`, `Legendaire` — cohérent avec Epic 1.
- **ScriptableObject Architecture** : `MonsterData` = asset SO dans `Assets/Data/Monsters/`. Jamais `new MonsterData()` en runtime.
- **Service Locator** : `KeywordResolver` s'enregistre via `Services.Register<IKeywordResolver>()` dans `BootLoader.RegisterServices()`.
- **Assembly** : tout le code combat va dans `Gameplay.asmdef` (`Scripts/Gameplay/Combat/`). UI dans `UI.asmdef` (`Scripts/UI/`).
- **GameLog** : `GameLog.Info/Warning/Error` — jamais `Debug.Log` directement.
- **UI Toolkit** : UXML + USS uniquement, pas de Canvas uGUI. `VisualElement` pour les vues de cartes.
- **ReactiveWindowState** : fenêtre de réponse adverse déclenchée par bouton "Passer" — pas de timer.
- **Pas de sauvegarde mid-combat** (ADR-001) : `CombatContext` est volatile, jamais sérialisé.
- **KeywordResolver séparé** (ADR-002) : le resolver répond à des questions bool (`CanTarget`, `CanBlock`, etc.) — il ne modifie jamais l'état du combat.

### Files to Reference

| File | Purpose |
| ---- | ------- |
| `_bmad-output/game-architecture.md` | CombatStateMachine, états, ADRs |
| `_bmad-output/project-context.md` | Règles critiques Unity/C# |
| `_bmad-output/planning-artifacts/epics.md` | Stories 2.1–2.11 avec ACs officiels |
| `_bmad-output/gdd.md` | Définitions mots-clés, structure de tour, types de cartes |
| `Assets/Scripts/Gameplay/Cards/CardData.cs` | Modèle carte existant |
| `Assets/Scripts/Gameplay/Deck/DeckData.cs` | Modèle deck existant |
| `Assets/Scripts/Core/Services/ServiceLocator.cs` | Pattern enregistrement service |
| `Assets/Scripts/Core/Rules/GameRules.cs` | Constantes (DeckSize, MaxHandSize, etc.) |

### Technical Decisions

- **`ReactiveWindowState`** : bouton "Passer" — pas de timer. Le jeu est purement au tour par tour.
- **`MonsterData`** : structure passive vide via interface `IBossPassive` (champ nullable dans MonsterData). Implémentée dans Epics 5-7.
- **`AlliedInstance`** : classe C# pure (pas MonoBehaviour, pas SO) — état runtime d'un allié pendant le combat (CardData ref + HP courant + bool `_isSleeping`).
- **`CombatContext`** : classe C# pure passée à chaque état — contient tout l'état mutable du combat. Non sérialisée (ADR-001).
- **Deck runtime** : `CombatContext` crée une `List<CardData>` copiée depuis `DeckData.Cards` au début du combat. Les cartes sont ajoutées/retirées de cette liste en runtime, pas modifiées sur l'asset SO.
- **Sacrifice → bas du deck** : `_deck.Add(card)` (pas au cimetière — confirmé dans spec Epic 1).
- **Équipement après usage → Cimetière** : unique cas où une carte non-Allié va au Cimetière.
- **Ritual(X) → X cartes de la main au Cimetière** (différent du sacrifice normal qui va au bas du deck).
- **`KeywordResolver`** : enregistré comme `IKeywordResolver` via ServiceLocator. Méthodes : `CanTarget(attacker, target)`, `CanBlock(action)`, `GetPriorityTarget(allies)`, `IsImmediate(card)`, `IsEveillee(card)`, `IsRampant(card)`, `GetRitualCost(card)`.
- **1 deck joueur hardcodé** : référencé dans `CombatManager` via `[SerializeField] DeckData _playerDeck`.
- **Drag & drop** : Unity UI Toolkit drag events (`RegisterCallback<MouseDownEvent>`, `RegisterCallback<MouseMoveEvent>`, `RegisterCallback<MouseUpEvent>`) — pas besoin de packages tiers pour MVP.

---

## Implementation Plan

### Tasks

#### T1 — Interfaces de base (IBossPassive, ICombatState, IKeywordResolver)

**T1.1 — `Assets/Scripts/Gameplay/Combat/Data/IBossPassive.cs`**
```csharp
namespace MonsterCardGame.Gameplay.Combat.Data
{
    /// <summary>Interface vide — implémentations dans Epics 5-7.</summary>
    public interface IBossPassive { }
}
```

**T1.2 — `Assets/Scripts/Gameplay/Combat/States/ICombatState.cs`**
```csharp
namespace MonsterCardGame.Gameplay.Combat.States
{
    public interface ICombatState
    {
        void Enter(CombatContext ctx);
        void Update(CombatContext ctx);
        void Exit(CombatContext ctx);
    }
}
```

**T1.3 — `Assets/Scripts/Gameplay/Combat/Keywords/IKeywordResolver.cs`**
```csharp
using System.Collections.Generic;
using MonsterCardGame.Core.Services;

namespace MonsterCardGame.Gameplay.Combat.Keywords
{
    public interface IKeywordResolver : IService
    {
        /// <summary>L'attaquant peut-il cibler la cible ? Respecte Vol et Portée.</summary>
        bool CanTarget(AlliedInstance attacker, AlliedInstance target);

        /// <summary>Retourne la cible prioritaire parmi les alliés adverses (Provocation en premier).</summary>
        AlliedInstance GetPriorityTarget(IReadOnlyList<AlliedInstance> allies);

        /// <summary>La carte peut-elle être bloquée ? Les Instantané ne peuvent pas.</summary>
        bool CanBeBlocked(AlliedInstance source);

        /// <summary>La carte est-elle Éveillée (peut agir dès sa mise en jeu) ?</summary>
        bool IsEveille(AlliedInstance ally);

        /// <summary>La carte est-elle Rampante ?</summary>
        bool IsRampant(AlliedInstance ally);

        /// <summary>Retourne le coût Rituel(X) de la carte, ou 0 si absent.</summary>
        int GetRitualCost(MonsterCardGame.Gameplay.Cards.CardData card);
    }
}
```

---

#### T2 — `AlliedInstance`

**`Assets/Scripts/Gameplay/Combat/AlliedInstance.cs`**
```csharp
using MonsterCardGame.Gameplay.Cards;

namespace MonsterCardGame.Gameplay.Combat
{
    /// <summary>État runtime d'un allié pendant le combat. Classe C# pure — jamais MonoBehaviour ni SO.</summary>
    public class AlliedInstance
    {
        public CardData Data     { get; }
        public int      HP       { get; private set; }
        public int      ATK      { get; private set; }
        public int      DEF      { get; private set; }
        public bool     IsSleeping { get; private set; }

        public AlliedInstance(CardData data)
        {
            Data       = data;
            HP         = data.Defense;   // Defense sert de HP pour les alliés
            ATK        = data.Attack;
            DEF        = data.Defense;
            IsSleeping = !data.HasKeyword(Keyword.Eveille); // endormi sauf si Éveillé
        }

        public void TakeDamage(int amount)
        {
            HP -= amount;
            if (HP < 0) HP = 0;
        }

        public void Heal(int amount) => HP += amount;

        public void SetSleeping(bool sleeping) => IsSleeping = sleeping;

        public bool IsAlive => HP > 0;
    }
}
```

---

#### T3 — `MonsterData` ScriptableObject

**`Assets/Scripts/Gameplay/Combat/Data/MonsterData.cs`**
```csharp
using System.Collections.Generic;
using UnityEngine;
using MonsterCardGame.Gameplay.Cards;

namespace MonsterCardGame.Gameplay.Combat.Data
{
    [CreateAssetMenu(fileName = "MonsterData", menuName = "MonsterCardGame/Monster Data")]
    public class MonsterData : ScriptableObject
    {
        [SerializeField] private string            _monsterName = "Monstre";
        [SerializeField] private int               _startingHP  = 30;
        [SerializeField] private List<CardData>    _deck        = new();
        [SerializeField] private ScriptableObject  _bossPassive = null; // IBossPassive, nullable — Epics 5-7

        public string              MonsterName  => _monsterName;
        public int                 StartingHP   => _startingHP;
        public IReadOnlyList<CardData> Deck     => _deck;
        public IBossPassive        BossPassive  => _bossPassive as IBossPassive;
    }
}
```

---

#### T4 — `KeywordResolver`

**`Assets/Scripts/Gameplay/Combat/Keywords/KeywordResolver.cs`**
```csharp
using System.Collections.Generic;
using MonsterCardGame.Gameplay.Cards;

namespace MonsterCardGame.Gameplay.Combat.Keywords
{
    public class KeywordResolver : IKeywordResolver
    {
        public bool CanTarget(AlliedInstance attacker, AlliedInstance target)
        {
            // Un allié au sol ne peut pas cibler un allié Vol adverse sauf s'il a Portée
            bool targetHasVol    = target.Data.HasKeyword(Keyword.Vol);
            bool attackerHasVol  = attacker.Data.HasKeyword(Keyword.Vol);
            bool attackerHasPortee = attacker.Data.HasKeyword(Keyword.Portee);

            if (targetHasVol && !attackerHasVol && !attackerHasPortee)
                return false;

            return true;
        }

        public AlliedInstance GetPriorityTarget(IReadOnlyList<AlliedInstance> allies)
        {
            if (allies == null || allies.Count == 0) return null;

            // Provocation : cibler en priorité
            foreach (var ally in allies)
                if (ally.IsAlive && ally.Data.HasKeyword(Keyword.Provocation))
                    return ally;

            // Sinon premier allié vivant
            foreach (var ally in allies)
                if (ally.IsAlive) return ally;

            return null;
        }

        public bool CanBeBlocked(AlliedInstance source)
        {
            // Les Instantané ne peuvent pas être bloqués
            return !source.Data.HasKeyword(Keyword.Instantane);
        }

        public bool IsEveille(AlliedInstance ally)
            => ally.Data.HasKeyword(Keyword.Eveille);

        public bool IsRampant(AlliedInstance ally)
            => ally.Data.HasKeyword(Keyword.Rampant);

        public int GetRitualCost(CardData card)
        {
            if (!card.HasKeyword(Keyword.Rituel)) return 0;
            return card.RitualValue;
        }
    }
}
```

---

#### T5 — `CombatContext`

**`Assets/Scripts/Gameplay/Combat/CombatContext.cs`**
```csharp
using System.Collections.Generic;
using MonsterCardGame.Gameplay.Cards;
using MonsterCardGame.Gameplay.Combat.Data;

namespace MonsterCardGame.Gameplay.Combat
{
    /// <summary>
    /// État mutable partagé entre tous les états du combat.
    /// Classe C# pure — jamais sérialisée (ADR-001).
    /// </summary>
    public class CombatContext
    {
        // --- Joueur ---
        public List<CardData>      PlayerDeck      { get; } = new();
        public List<CardData>      PlayerHand      { get; } = new();
        public List<CardData>      PlayerCemetery  { get; } = new();
        public List<AlliedInstance> PlayerAllies   { get; } = new();
        public int                 PlayerHP        { get; set; }
        public int                 PlayerMana      { get; set; }

        // --- Monstre ---
        public MonsterData         MonsterData     { get; }
        public List<CardData>      MonsterDeck     { get; } = new();
        public List<AlliedInstance> MonsterAllies  { get; } = new();
        public int                 MonsterHP       { get; set; }

        // --- État tour ---
        public int                 Turn            { get; set; } = 1;
        public CardData            PendingPlayCard { get; set; }   // carte en cours de jeu (drag & drop)
        public bool                IsPlayerTurn    { get; set; } = true;

        // --- Résultat ---
        public CombatResult        Result          { get; set; } = CombatResult.None;

        public CombatContext(MonsterData monsterData, IReadOnlyList<CardData> playerDeckSource)
        {
            MonsterData = monsterData;
            MonsterHP   = monsterData.StartingHP;
            PlayerHP    = Core.GameRules.PlayerStartingHP;
            PlayerMana  = 0;

            // Copie des decks (jamais modifier les assets SO)
            foreach (var card in playerDeckSource) PlayerDeck.Add(card);
            foreach (var card in monsterData.Deck)  MonsterDeck.Add(card);
        }
    }

    public enum CombatResult { None, PlayerWin, PlayerLose }
}
```

---

#### T6 — États du combat

**T6.1 — `Assets/Scripts/Gameplay/Combat/States/DrawState.cs`**
```csharp
using MonsterCardGame.Core;

namespace MonsterCardGame.Gameplay.Combat.States
{
    /// <summary>Pioche 1 carte. Si deck vide, défaite immédiate. Puis → SacrificeState.</summary>
    public class DrawState : ICombatState
    {
        private readonly System.Action<ICombatState> _transitionTo;
        private readonly ICombatState _sacrifice;

        public DrawState(System.Action<ICombatState> transitionTo, ICombatState sacrifice)
        {
            _transitionTo = transitionTo;
            _sacrifice    = sacrifice;
        }

        public void Enter(CombatContext ctx)
        {
            if (ctx.PlayerDeck.Count == 0)
            {
                GameLog.Warning("DrawState", "Deck vide — défaite");
                ctx.Result = CombatResult.PlayerLose;
                return;
            }

            // Pioche (indice 0 = sommet du deck)
            var card = ctx.PlayerDeck[0];
            ctx.PlayerDeck.RemoveAt(0);

            if (ctx.PlayerHand.Count < Core.GameRules.MaxHandSize)
                ctx.PlayerHand.Add(card);
            else
            {
                GameLog.Info("DrawState", $"Main pleine — {card.CardName} envoyée au cimetière");
                ctx.PlayerCemetery.Add(card);
            }

            // Mana +1 chaque tour (plafonné à 10)
            ctx.PlayerMana = System.Math.Min(ctx.Turn, 10);

            GameLog.Info("DrawState", $"Tour {ctx.Turn} — pioche : {card.CardName}. Mana : {ctx.PlayerMana}");
        }

        public void Update(CombatContext ctx)
        {
            if (ctx.Result == CombatResult.PlayerLose) return;
            _transitionTo(_sacrifice);
        }

        public void Exit(CombatContext ctx) { }
    }
}
```

**T6.2 — `Assets/Scripts/Gameplay/Combat/States/SacrificeState.cs`**
```csharp
using MonsterCardGame.Core;

namespace MonsterCardGame.Gameplay.Combat.States
{
    /// <summary>
    /// Optionnel : le joueur peut sacrifier 1 carte de sa main (→ bas du deck, +1 mana).
    /// Déclenché par UI. Passer → PlayState directement.
    /// </summary>
    public class SacrificeState : ICombatState
    {
        private readonly System.Action<ICombatState> _transitionTo;
        private readonly ICombatState _play;
        private bool _sacrificeDone;

        public SacrificeState(System.Action<ICombatState> transitionTo, ICombatState play)
        {
            _transitionTo = transitionTo;
            _play         = play;
        }

        public void Enter(CombatContext ctx)
        {
            _sacrificeDone = false;
            GameLog.Info("SacrificeState", "En attente de sacrifice ou de passage");
        }

        public void Update(CombatContext ctx) { /* l'UI appelle TrySacrifice ou Skip */ }

        public void Exit(CombatContext ctx) { }

        /// <summary>Appelé par l'UI quand le joueur choisit une carte à sacrifier.</summary>
        public void TrySacrifice(CombatContext ctx, Gameplay.Cards.CardData card)
        {
            if (_sacrificeDone)
            {
                GameLog.Warning("SacrificeState", "Sacrifice déjà effectué ce tour");
                return;
            }
            if (!ctx.PlayerHand.Remove(card))
            {
                GameLog.Warning("SacrificeState", $"Carte {card.CardName} introuvable en main");
                return;
            }
            ctx.PlayerDeck.Add(card); // bas du deck
            ctx.PlayerMana++;
            _sacrificeDone = true;
            GameLog.Info("SacrificeState", $"Sacrifice : {card.CardName} → bas du deck. Mana : {ctx.PlayerMana}");
            _transitionTo(_play);
        }

        /// <summary>Appelé par l'UI quand le joueur passe la phase de sacrifice.</summary>
        public void Skip(CombatContext ctx) => _transitionTo(_play);
    }
}
```

**T6.3 — `Assets/Scripts/Gameplay/Combat/States/PlayState.cs`**
```csharp
using MonsterCardGame.Core;
using MonsterCardGame.Gameplay.Cards;
using MonsterCardGame.Gameplay.Combat.Keywords;

namespace MonsterCardGame.Gameplay.Combat.States
{
    /// <summary>
    /// Le joueur joue des cartes depuis sa main (drag & drop).
    /// Fin de phase → ReactiveWindowState.
    /// </summary>
    public class PlayState : ICombatState
    {
        private readonly System.Action<ICombatState> _transitionTo;
        private readonly ICombatState _reactive;
        private IKeywordResolver _resolver;

        public PlayState(System.Action<ICombatState> transitionTo, ICombatState reactive)
        {
            _transitionTo = transitionTo;
            _reactive     = reactive;
        }

        public void Enter(CombatContext ctx)
        {
            _resolver = Core.Services.Services.Get<IKeywordResolver>();
            GameLog.Info("PlayState", "Phase de jeu — en attente d'actions joueur");
        }

        public void Update(CombatContext ctx) { }

        public void Exit(CombatContext ctx) { }

        /// <summary>Tente de jouer une carte depuis la main.</summary>
        public bool TryPlayCard(CombatContext ctx, CardData card)
        {
            if (!ctx.PlayerHand.Contains(card))
            {
                GameLog.Warning("PlayState", $"{card.CardName} introuvable en main");
                return false;
            }
            if (ctx.PlayerMana < card.ManaCost)
            {
                GameLog.Warning("PlayState", $"Mana insuffisant pour {card.CardName} (coût {card.ManaCost}, dispo {ctx.PlayerMana})");
                return false;
            }

            ctx.PlayerMana -= card.ManaCost;
            ctx.PlayerHand.Remove(card);

            switch (card.CardType)
            {
                case CardType.Allie:
                    var ally = new AlliedInstance(card);
                    ctx.PlayerAllies.Add(ally);
                    // Éveillé : peut agir ce tour. Sinon endormi.
                    GameLog.Info("PlayState", $"Allié joué : {card.CardName} (Éveillé={ally.Data.HasKeyword(Keyword.Eveille)})");
                    break;

                case CardType.Action:
                case CardType.Blocage:
                case CardType.Reaction:
                    ctx.PlayerDeck.Add(card); // bas du deck
                    GameLog.Info("PlayState", $"Carte jouée : {card.CardName} → bas du deck");
                    break;

                case CardType.Equipement:
                    ctx.PlayerCemetery.Add(card); // cimetière après usage
                    GameLog.Info("PlayState", $"Équipement joué : {card.CardName} → cimetière");
                    break;
            }

            // Génération de mana de la carte
            if (card.ManaGenerated > 0)
                ctx.PlayerMana += card.ManaGenerated;

            return true;
        }

        /// <summary>Le joueur termine sa phase de jeu.</summary>
        public void EndPlay(CombatContext ctx)
        {
            // Réveil des alliés endormis pour le prochain tour
            foreach (var a in ctx.PlayerAllies)
                if (a.IsSleeping) a.SetSleeping(false);

            _transitionTo(_reactive);
        }
    }
}
```

**T6.4 — `Assets/Scripts/Gameplay/Combat/States/ReactiveWindowState.cs`**
```csharp
using MonsterCardGame.Core;

namespace MonsterCardGame.Gameplay.Combat.States
{
    /// <summary>
    /// Fenêtre de réponse adverse avant le tour monstre.
    /// Pas de timer — bouton "Passer" uniquement.
    /// </summary>
    public class ReactiveWindowState : ICombatState
    {
        private readonly System.Action<ICombatState> _transitionTo;
        private readonly ICombatState _monsterTurn;

        public ReactiveWindowState(System.Action<ICombatState> transitionTo, ICombatState monsterTurn)
        {
            _transitionTo = transitionTo;
            _monsterTurn  = monsterTurn;
        }

        public void Enter(CombatContext ctx)
            => GameLog.Info("ReactiveWindowState", "Fenêtre réactive — cliquer Passer pour continuer");

        public void Update(CombatContext ctx) { }

        public void Exit(CombatContext ctx) { }

        /// <summary>Appelé par l'UI quand le joueur clique "Passer".</summary>
        public void Pass(CombatContext ctx) => _transitionTo(_monsterTurn);
    }
}
```

**T6.5 — `Assets/Scripts/Gameplay/Combat/States/MonsterTurnState.cs`**
```csharp
using MonsterCardGame.Core;
using MonsterCardGame.Gameplay.Combat.MonsterAI;

namespace MonsterCardGame.Gameplay.Combat.States
{
    /// <summary>
    /// Tour du monstre. Délègue à MonsterAIController.
    /// Peut être interrompu par la ReactiveWindowState entre chaque action.
    /// </summary>
    public class MonsterTurnState : ICombatState
    {
        private readonly System.Action<ICombatState> _transitionTo;
        private readonly ICombatState _reactive;
        private readonly ICombatState _draw;
        private readonly ICombatState _end;
        private readonly MonsterAIController _ai;

        public MonsterTurnState(
            System.Action<ICombatState> transitionTo,
            ICombatState reactive,
            ICombatState draw,
            ICombatState end,
            MonsterAIController ai)
        {
            _transitionTo = transitionTo;
            _reactive     = reactive;
            _draw         = draw;
            _end          = end;
            _ai           = ai;
        }

        public void Enter(CombatContext ctx)
        {
            GameLog.Info("MonsterTurnState", "Tour monstre");
            _ai.ExecuteNextAction(ctx, OnActionComplete);
        }

        public void Update(CombatContext ctx) { }

        public void Exit(CombatContext ctx) { }

        private void OnActionComplete(CombatContext ctx, bool hasMoreActions)
        {
            if (ctx.Result != CombatResult.None)
            {
                _transitionTo(_end);
                return;
            }

            if (hasMoreActions)
                _transitionTo(_reactive); // fenêtre réactive entre chaque action monstre
            else
            {
                ctx.Turn++;
                ctx.IsPlayerTurn = true;
                _transitionTo(_draw); // fin du tour monstre → prochain tour joueur
            }
        }
    }
}
```

**T6.6 — `Assets/Scripts/Gameplay/Combat/States/CombatEndState.cs`**
```csharp
using MonsterCardGame.Core;

namespace MonsterCardGame.Gameplay.Combat.States
{
    /// <summary>Affiche victoire ou défaite. Terminal — pas de transition sortante.</summary>
    public class CombatEndState : ICombatState
    {
        public void Enter(CombatContext ctx)
        {
            switch (ctx.Result)
            {
                case CombatResult.PlayerWin:
                    GameLog.Info("CombatEndState", "VICTOIRE");
                    break;
                case CombatResult.PlayerLose:
                    GameLog.Info("CombatEndState", "DÉFAITE");
                    break;
                default:
                    GameLog.Warning("CombatEndState", "CombatEndState atteint sans résultat défini");
                    break;
            }
        }

        public void Update(CombatContext ctx) { }
        public void Exit(CombatContext ctx) { }
    }
}
```

---

#### T7 — `MonsterAIController`

**`Assets/Scripts/Gameplay/Combat/MonsterAI/MonsterAIController.cs`**
```csharp
using System;
using System.Collections.Generic;
using MonsterCardGame.Core;
using MonsterCardGame.Gameplay.Cards;
using MonsterCardGame.Gameplay.Combat.Keywords;

namespace MonsterCardGame.Gameplay.Combat.MonsterAI
{
    /// <summary>
    /// IA scriptée : joue les cartes du deck monstre dans l'ordre.
    /// Respecte Vol/Provocation pour le ciblage.
    /// </summary>
    public class MonsterAIController
    {
        private int _actionIndex = 0;
        private IKeywordResolver _resolver;

        public void Reset() => _actionIndex = 0;

        /// <summary>Exécute la prochaine action et appelle le callback avec (ctx, hasMoreActions).</summary>
        public void ExecuteNextAction(CombatContext ctx, Action<CombatContext, bool> onComplete)
        {
            _resolver ??= Core.Services.Services.Get<IKeywordResolver>();

            if (ctx.MonsterDeck.Count == 0)
            {
                GameLog.Info("MonsterAI", "Deck monstre vide — fin du tour monstre");
                onComplete(ctx, false);
                return;
            }

            if (_actionIndex >= ctx.MonsterDeck.Count)
            {
                GameLog.Info("MonsterAI", "Toutes les actions du tour exécutées");
                _actionIndex = 0;
                onComplete(ctx, false);
                return;
            }

            var card = ctx.MonsterDeck[_actionIndex];
            _actionIndex++;

            ExecuteCard(ctx, card);

            CheckCombatEnd(ctx);

            bool hasMore = _actionIndex < ctx.MonsterDeck.Count;
            onComplete(ctx, hasMore);
        }

        private void ExecuteCard(CombatContext ctx, CardData card)
        {
            GameLog.Info("MonsterAI", $"Monstre joue : {card.CardName}");

            switch (card.CardType)
            {
                case CardType.Allie:
                    var ally = new AlliedInstance(card);
                    ctx.MonsterAllies.Add(ally);
                    break;

                case CardType.Action:
                    // Attaque le joueur ou un allié joueur selon ciblage
                    var target = _resolver.GetPriorityTarget(ctx.PlayerAllies);
                    if (target != null)
                    {
                        target.TakeDamage(card.Attack);
                        GameLog.Info("MonsterAI", $"{card.CardName} attaque {target.Data.CardName} pour {card.Attack}");
                        if (!target.IsAlive)
                        {
                            ctx.PlayerAllies.Remove(target);
                            ctx.PlayerCemetery.Add(target.Data);
                        }
                    }
                    else
                    {
                        ctx.PlayerHP -= card.Attack;
                        GameLog.Info("MonsterAI", $"{card.CardName} attaque directement le joueur pour {card.Attack}. PV joueur : {ctx.PlayerHP}");
                    }
                    ctx.MonsterDeck.Add(card); // bas du deck
                    break;

                default:
                    ctx.MonsterDeck.Add(card);
                    break;
            }
        }

        private static void CheckCombatEnd(CombatContext ctx)
        {
            if (ctx.PlayerHP <= 0)    ctx.Result = CombatResult.PlayerLose;
            if (ctx.MonsterHP <= 0)   ctx.Result = CombatResult.PlayerWin;
        }
    }
}
```

---

#### T8 — `CombatManager`

**`Assets/Scripts/Gameplay/Combat/CombatManager.cs`**
```csharp
using UnityEngine;
using MonsterCardGame.Core;
using MonsterCardGame.Gameplay.Cards;
using MonsterCardGame.Gameplay.Combat.Data;
using MonsterCardGame.Gameplay.Combat.MonsterAI;
using MonsterCardGame.Gameplay.Combat.States;

namespace MonsterCardGame.Gameplay.Combat
{
    /// <summary>
    /// MonoBehaviour orchestrateur. Contient et drive la CombatStateMachine.
    /// Placé sur un GameObject vide "CombatManager" dans Combat.unity.
    /// </summary>
    public class CombatManager : MonoBehaviour
    {
        [SerializeField] private DeckData    _playerDeck;   // deck joueur hardcodé
        [SerializeField] private MonsterData _monsterData;  // monstre de test hardcodé

        private CombatContext      _ctx;
        private ICombatState       _currentState;

        // Instances des états (créées une fois, réutilisées)
        private DrawState           _drawState;
        private SacrificeState      _sacrificeState;
        private PlayState           _playState;
        private ReactiveWindowState _reactiveState;
        private MonsterTurnState    _monsterTurnState;
        private CombatEndState      _endState;

        private MonsterAIController _ai = new();

        // Accesseurs publics pour l'UI
        public CombatContext       Context       => _ctx;
        public SacrificeState      SacrificeState => _sacrificeState;
        public PlayState           PlayState      => _playState;
        public ReactiveWindowState ReactiveState  => _reactiveState;

        private void Start()
        {
            if (_playerDeck == null || _monsterData == null)
            {
                GameLog.Error("CombatManager", "PlayerDeck ou MonsterData non assigné dans l'Inspector");
                return;
            }

            InitStates();
            _ctx = new CombatContext(_monsterData, _playerDeck.Cards);
            TransitionTo(_drawState);
        }

        private void InitStates()
        {
            _endState         = new CombatEndState();
            _monsterTurnState = new MonsterTurnState(TransitionTo, null, null, _endState, _ai);
            _reactiveState    = new ReactiveWindowState(TransitionTo, _monsterTurnState);
            _playState        = new PlayState(TransitionTo, _reactiveState);
            _sacrificeState   = new SacrificeState(TransitionTo, _playState);
            _drawState        = new DrawState(TransitionTo, _sacrificeState);

            // Patch forward refs sur MonsterTurnState
            RewireMonsterTurnState();
        }

        private void RewireMonsterTurnState()
        {
            // MonsterTurnState a besoin de _reactive et _draw créés après lui
            _monsterTurnState = new MonsterTurnState(TransitionTo, _reactiveState, _drawState, _endState, _ai);
            _reactiveState    = new ReactiveWindowState(TransitionTo, _monsterTurnState);
            _playState        = new PlayState(TransitionTo, _reactiveState);
            _sacrificeState   = new SacrificeState(TransitionTo, _playState);
            _drawState        = new DrawState(TransitionTo, _sacrificeState);
        }

        private void Update()
        {
            _currentState?.Update(_ctx);

            if (_ctx?.Result != CombatResult.None && !(_currentState is CombatEndState))
                TransitionTo(_endState);
        }

        public void TransitionTo(ICombatState next)
        {
            _currentState?.Exit(_ctx);
            _currentState = next;
            _currentState?.Enter(_ctx);
            GameLog.Info("CombatManager", $"→ {next?.GetType().Name}");
        }
    }
}
```

---

#### T9 — Mise à jour de `BootLoader`

Dans `Assets/Scripts/Core/Services/BootLoader.cs`, ajouter l'enregistrement du `KeywordResolver` dans `RegisterServices()` :

```csharp
// Ajouter le using en haut :
using MonsterCardGame.Gameplay.Combat.Keywords;

// Dans RegisterServices() :
Services.Register<IKeywordResolver>(new KeywordResolver());
```

---

#### T10 — `CardView` (UI Toolkit)

**`Assets/Scripts/UI/Combat/CardView.cs`**
```csharp
using UnityEngine;
using UnityEngine.UIElements;
using MonsterCardGame.Gameplay.Cards;

namespace MonsterCardGame.UI.Combat
{
    /// <summary>
    /// VisualElement custom représentant une carte dans la main.
    /// Placeholder : Rectangle coloré + Label nom + Label coût mana.
    /// </summary>
    public class CardView : VisualElement
    {
        public CardData Data { get; private set; }

        private readonly Label _nameLabel;
        private readonly Label _costLabel;

        public CardView(CardData data)
        {
            Data = data;

            AddToClassList("card-view");

            _nameLabel = new Label(data.CardName)  { name = "card-name" };
            _costLabel = new Label($"{data.ManaCost}") { name = "card-cost" };

            Add(_nameLabel);
            Add(_costLabel);
        }

        public void Refresh(CardData data)
        {
            Data           = data;
            _nameLabel.text = data.CardName;
            _costLabel.text  = $"{data.ManaCost}";
        }
    }
}
```

---

#### T11 — `Combat.uxml` et `Combat.uss`

**`Assets/UI/Combat.uxml`**
```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements" xmlns:uie="UnityEditor.UIElements">
    <ui:VisualElement name="root" class="combat-root">

        <!-- Zone monstre -->
        <ui:VisualElement name="monster-zone" class="zone monster-zone">
            <ui:Label name="monster-name-label"  text="Monstre"  class="zone-title" />
            <ui:Label name="monster-hp-label"    text="PV: 30"   class="hp-label" />
            <ui:VisualElement name="monster-allies-zone" class="allies-row" />
        </ui:VisualElement>

        <!-- Zone alliés monstre (lecture seule) -->
        <!-- intégrée dans monster-zone ci-dessus -->

        <!-- Zone de jeu centrale (alliés joueur) -->
        <ui:VisualElement name="play-zone" class="zone play-zone">
            <ui:Label text="Zone de jeu" class="zone-title" />
            <ui:VisualElement name="player-allies-zone" class="allies-row" />
        </ui:VisualElement>

        <!-- Zone main joueur -->
        <ui:VisualElement name="hand-zone" class="zone hand-zone">
            <ui:VisualElement name="hand-cards" class="hand-row" />
        </ui:VisualElement>

        <!-- HUD joueur -->
        <ui:VisualElement name="player-hud" class="hud">
            <ui:Label name="player-hp-label"   text="PV: 30"  class="hp-label" />
            <ui:Label name="player-mana-label" text="Mana: 0" class="mana-label" />
            <ui:Label name="turn-label"        text="Tour 1"  class="turn-label" />
        </ui:VisualElement>

        <!-- Boutons d'action -->
        <ui:VisualElement name="action-buttons" class="action-bar">
            <ui:Button name="sacrifice-btn" text="Sacrifier" class="action-btn" />
            <ui:Button name="end-play-btn"  text="Fin de phase" class="action-btn" />
            <ui:Button name="pass-btn"      text="Passer" class="action-btn" />
        </ui:VisualElement>

        <!-- Overlay résultat combat -->
        <ui:VisualElement name="result-overlay" class="result-overlay hidden">
            <ui:Label name="result-label" text="" class="result-label" />
        </ui:VisualElement>

    </ui:VisualElement>
</ui:UXML>
```

**`Assets/UI/Combat.uss`**
```css
.combat-root {
    flex-direction: column;
    flex-grow: 1;
    background-color: rgb(20, 30, 20);
    padding: 8px;
}

.zone {
    border-width: 2px;
    border-color: rgb(80, 120, 80);
    border-radius: 6px;
    padding: 6px;
    margin-bottom: 6px;
}

.monster-zone {
    flex-grow: 2;
    background-color: rgb(40, 20, 20);
}

.play-zone {
    flex-grow: 2;
    background-color: rgb(20, 40, 20);
}

.hand-zone {
    flex-grow: 1;
    background-color: rgb(20, 20, 40);
}

.zone-title {
    font-size: 12px;
    color: rgb(180, 180, 180);
    -unity-font-style: bold;
    margin-bottom: 4px;
}

.hp-label {
    color: rgb(220, 80, 80);
    font-size: 14px;
}

.mana-label {
    color: rgb(80, 140, 220);
    font-size: 14px;
}

.turn-label {
    color: rgb(200, 200, 200);
    font-size: 12px;
}

.allies-row {
    flex-direction: row;
    flex-wrap: wrap;
}

.hand-row {
    flex-direction: row;
    flex-wrap: nowrap;
    overflow: hidden;
}

/* CardView placeholder */
.card-view {
    width: 80px;
    height: 110px;
    background-color: rgb(60, 60, 90);
    border-width: 1px;
    border-color: rgb(120, 120, 160);
    border-radius: 4px;
    margin: 4px;
    padding: 4px;
    align-items: center;
    justify-content: space-between;
}

#card-name {
    font-size: 9px;
    color: rgb(220, 220, 220);
    white-space: normal;
    -unity-text-align: upper-center;
}

#card-cost {
    font-size: 12px;
    color: rgb(80, 180, 255);
    -unity-text-align: lower-right;
}

.hud {
    flex-direction: row;
    justify-content: space-between;
    padding: 4px 8px;
    background-color: rgb(30, 30, 30);
    border-radius: 4px;
    margin-bottom: 6px;
}

.action-bar {
    flex-direction: row;
    justify-content: flex-end;
}

.action-btn {
    margin-left: 8px;
    padding: 6px 12px;
    background-color: rgb(50, 90, 50);
    color: white;
    border-radius: 4px;
    border-width: 1px;
    border-color: rgb(80, 140, 80);
}

.result-overlay {
    position: absolute;
    top: 0; left: 0; right: 0; bottom: 0;
    background-color: rgba(0, 0, 0, 0.75);
    align-items: center;
    justify-content: center;
}

.result-label {
    font-size: 48px;
    color: rgb(255, 220, 50);
    -unity-font-style: bold;
}

.hidden {
    display: none;
}
```

---

#### T12 — `CombatBoardUI`

**`Assets/Scripts/UI/Combat/CombatBoardUI.cs`**
```csharp
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using MonsterCardGame.Core;
using MonsterCardGame.Gameplay.Cards;
using MonsterCardGame.Gameplay.Combat;
using MonsterCardGame.Gameplay.Combat.States;

namespace MonsterCardGame.UI.Combat
{
    /// <summary>
    /// MonoBehaviour sur le même GameObject que UIDocument.
    /// Synchronise le CombatContext vers les éléments UXML.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class CombatBoardUI : MonoBehaviour
    {
        [SerializeField] private CombatManager _combatManager;

        private VisualElement _root;
        private Label         _monsterHPLabel;
        private Label         _playerHPLabel;
        private Label         _playerManaLabel;
        private Label         _turnLabel;
        private VisualElement _handCards;
        private Button        _sacrificeBtn;
        private Button        _endPlayBtn;
        private Button        _passBtn;
        private VisualElement _resultOverlay;
        private Label         _resultLabel;

        private readonly List<CardView> _handViews = new();
        private CardView _selectedCard;

        private void OnEnable()
        {
            var doc = GetComponent<UIDocument>();
            _root = doc.rootVisualElement;

            _monsterHPLabel  = _root.Q<Label>("monster-hp-label");
            _playerHPLabel   = _root.Q<Label>("player-hp-label");
            _playerManaLabel = _root.Q<Label>("player-mana-label");
            _turnLabel       = _root.Q<Label>("turn-label");
            _handCards       = _root.Q<VisualElement>("hand-cards");
            _sacrificeBtn    = _root.Q<Button>("sacrifice-btn");
            _endPlayBtn      = _root.Q<Button>("end-play-btn");
            _passBtn         = _root.Q<Button>("pass-btn");
            _resultOverlay   = _root.Q<VisualElement>("result-overlay");
            _resultLabel     = _root.Q<Label>("result-label");

            _sacrificeBtn.clicked += OnSacrificeClicked;
            _endPlayBtn.clicked   += OnEndPlayClicked;
            _passBtn.clicked      += OnPassClicked;
        }

        private void Update()
        {
            if (_combatManager?.Context == null) return;
            RefreshUI(_combatManager.Context);
        }

        private void RefreshUI(CombatContext ctx)
        {
            _monsterHPLabel.text  = $"PV: {ctx.MonsterHP}";
            _playerHPLabel.text   = $"PV: {ctx.PlayerHP}";
            _playerManaLabel.text = $"Mana: {ctx.PlayerMana}";
            _turnLabel.text       = $"Tour {ctx.Turn}";

            RefreshHand(ctx);
            RefreshResultOverlay(ctx);
        }

        private void RefreshHand(CombatContext ctx)
        {
            // Reconstruire seulement si la main a changé
            if (_handViews.Count == ctx.PlayerHand.Count) return;

            _handCards.Clear();
            _handViews.Clear();

            foreach (var card in ctx.PlayerHand)
            {
                var view = new CardView(card);
                RegisterDragDrop(view);
                _handCards.Add(view);
                _handViews.Add(view);
            }
        }

        private void RegisterDragDrop(CardView view)
        {
            view.RegisterCallback<MouseDownEvent>(evt =>
            {
                _selectedCard = view;
                evt.StopPropagation();
            });

            view.RegisterCallback<MouseUpEvent>(evt =>
            {
                if (_selectedCard == null) return;
                TryPlaySelectedCard();
                _selectedCard = null;
                evt.StopPropagation();
            });
        }

        private void TryPlaySelectedCard()
        {
            if (_selectedCard == null) return;
            _combatManager.PlayState?.TryPlayCard(_combatManager.Context, _selectedCard.Data);
        }

        private void RefreshResultOverlay(CombatContext ctx)
        {
            if (ctx.Result == CombatResult.None)
            {
                _resultOverlay.AddToClassList("hidden");
                return;
            }
            _resultOverlay.RemoveFromClassList("hidden");
            _resultLabel.text = ctx.Result == CombatResult.PlayerWin ? "VICTOIRE" : "DÉFAITE";
        }

        private void OnSacrificeClicked()
        {
            if (_selectedCard == null)
            {
                GameLog.Warning("CombatBoardUI", "Aucune carte sélectionnée pour le sacrifice");
                return;
            }
            _combatManager.SacrificeState?.TrySacrifice(_combatManager.Context, _selectedCard.Data);
            _selectedCard = null;
        }

        private void OnEndPlayClicked()
            => _combatManager.PlayState?.EndPlay(_combatManager.Context);

        private void OnPassClicked()
            => _combatManager.ReactiveState?.Pass(_combatManager.Context);
    }
}
```

---

#### T13 — `KeywordResolverTests`

**`Assets/Tests/EditMode/KeywordResolverTests.cs`**
```csharp
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using MonsterCardGame.Gameplay.Cards;
using MonsterCardGame.Gameplay.Combat;
using MonsterCardGame.Gameplay.Combat.Keywords;

namespace MonsterCardGame.Tests.EditMode
{
    public class KeywordResolverTests
    {
        private KeywordResolver _resolver;

        [SetUp]
        public void SetUp() => _resolver = new KeywordResolver();

        // --- Helpers ---

        private static CardData CreateCard(Keyword keywords = Keyword.None,
                                           int attack = 1, int defense = 1,
                                           int ritualValue = 0)
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            var so   = new SerializedObject(card);
            so.FindProperty("_keywords").enumValueFlag    = (int)keywords;
            so.FindProperty("_attack").intValue           = attack;
            so.FindProperty("_defense").intValue          = defense;
            so.FindProperty("_ritualValue").intValue      = ritualValue;
            so.ApplyModifiedPropertiesWithoutUndo();
            return card;
        }

        private static AlliedInstance CreateAlly(Keyword keywords = Keyword.None,
                                                  int attack = 1, int defense = 1)
            => new AlliedInstance(CreateCard(keywords, attack, defense));

        // --- CanTarget ---

        [Test]
        public void CanTarget_GroundVsGround_ReturnsTrue()
        {
            var attacker = CreateAlly();
            var target   = CreateAlly();
            Assert.IsTrue(_resolver.CanTarget(attacker, target));
        }

        [Test]
        public void CanTarget_GroundVsVol_ReturnsFalse()
        {
            var attacker = CreateAlly();
            var target   = CreateAlly(Keyword.Vol);
            Assert.IsFalse(_resolver.CanTarget(attacker, target));
        }

        [Test]
        public void CanTarget_PorteeVsVol_ReturnsTrue()
        {
            var attacker = CreateAlly(Keyword.Portee);
            var target   = CreateAlly(Keyword.Vol);
            Assert.IsTrue(_resolver.CanTarget(attacker, target));
        }

        [Test]
        public void CanTarget_VolVsVol_ReturnsTrue()
        {
            var attacker = CreateAlly(Keyword.Vol);
            var target   = CreateAlly(Keyword.Vol);
            Assert.IsTrue(_resolver.CanTarget(attacker, target));
        }

        // --- GetPriorityTarget ---

        [Test]
        public void GetPriorityTarget_OnlyProvocation_ReturnsProvocation()
        {
            var normal = CreateAlly();
            var taunt  = CreateAlly(Keyword.Provocation);
            var list   = new List<AlliedInstance> { normal, taunt };
            Assert.AreEqual(taunt, _resolver.GetPriorityTarget(list));
        }

        [Test]
        public void GetPriorityTarget_NoProvocation_ReturnsFirst()
        {
            var a = CreateAlly();
            var b = CreateAlly();
            var list = new List<AlliedInstance> { a, b };
            Assert.AreEqual(a, _resolver.GetPriorityTarget(list));
        }

        [Test]
        public void GetPriorityTarget_EmptyList_ReturnsNull()
        {
            Assert.IsNull(_resolver.GetPriorityTarget(new List<AlliedInstance>()));
        }

        // --- CanBeBlocked ---

        [Test]
        public void CanBeBlocked_NoKeyword_ReturnsTrue()
        {
            var ally = CreateAlly();
            Assert.IsTrue(_resolver.CanBeBlocked(ally));
        }

        [Test]
        public void CanBeBlocked_Instantane_ReturnsFalse()
        {
            var ally = CreateAlly(Keyword.Instantane);
            Assert.IsFalse(_resolver.CanBeBlocked(ally));
        }

        // --- GetRitualCost ---

        [Test]
        public void GetRitualCost_RituelKeyword_ReturnsValue()
        {
            var card = CreateCard(Keyword.Rituel, ritualValue: 3);
            Assert.AreEqual(3, _resolver.GetRitualCost(card));
        }

        [Test]
        public void GetRitualCost_NoRituelKeyword_ReturnsZero()
        {
            var card = CreateCard();
            Assert.AreEqual(0, _resolver.GetRitualCost(card));
        }
    }
}
```

---

#### T14 — Étapes manuelles dans Unity Editor

Ces étapes ne peuvent pas être scriptées — elles nécessitent l'éditeur Unity ouvert.

**T14.1 — Créer les CardData assets (deck joueur hardcodé)**
1. `Assets/Data/Cards/` → clic droit → Create > MonsterCardGame > Card Data
2. Créer 40 cartes minimales (nom, coût mana 1-3, type Action/Allie, rareté Commune)
3. Ou créer 1 carte "Test Allié" + 1 carte "Test Action" et dupliquer jusqu'à 40

**T14.2 — Créer le DeckData asset joueur**
1. `Assets/Data/Decks/` → Create > MonsterCardGame > Deck Data
2. Nommer `PlayerTestDeck`
3. Référencer 40 cartes

**T14.3 — Créer le MonsterData asset**
1. `Assets/Data/Monsters/` → Create > MonsterCardGame > Monster Data
2. Nommer `TestMonster`, HP = 30
3. Ajouter 5-10 cartes Action dans le deck

**T14.4 — Configurer la scène Combat.unity**
1. Ouvrir `Combat.unity`
2. GameObject vide "CombatManager" → ajouter `CombatManager.cs`
3. Assigner `_playerDeck` = PlayerTestDeck, `_monsterData` = TestMonster
4. GameObject "UIDocument" → ajouter composant `UIDocument`
   - Source Asset = `Assets/UI/Combat.uxml`
   - Ajouter `CombatBoardUI.cs`
   - Assigner `_combatManager`

---

### Acceptance Criteria

#### AC-2.1 — Pioche et état main (DrawState)
- [ ] Au début du tour, 1 carte est tirée du deck et ajoutée en main (si < 6 cartes)
- [ ] Si deck vide au début d'un tour → `ctx.Result = PlayerLose`
- [ ] `PlayerMana` = `Math.Min(turn, 10)` à chaque DrawState.Enter

#### AC-2.2 — Sacrifice (SacrificeState)
- [ ] Sélectionner une carte + cliquer "Sacrifier" : carte retirée de la main, ajoutée en bas du deck, `PlayerMana++`
- [ ] Un seul sacrifice par tour autorisé
- [ ] Cliquer "Fin de phase" sans sacrifier passe directement à PlayState

#### AC-2.3 — Jeu de cartes (PlayState)
- [ ] Drag & drop d'une carte Allié → AlliedInstance créée dans `ctx.PlayerAllies`
- [ ] Drag & drop impossible si `PlayerMana < card.ManaCost` (log Warning)
- [ ] Allié sans Éveillé : `IsSleeping = true` à la pose
- [ ] Allié avec Éveillé : `IsSleeping = false` à la pose
- [ ] Carte Action jouée → bas du deck joueur
- [ ] Équipement joué → cimetière joueur
- [ ] "Fin de phase" → transition vers ReactiveWindowState

#### AC-2.4 — Fenêtre réactive (ReactiveWindowState)
- [ ] Bouton "Passer" visible et fonctionnel
- [ ] Pas de timer — le jeu attend indéfiniment le clic

#### AC-2.5 — Tour monstre (MonsterTurnState + MonsterAIController)
- [ ] Monstre joue les cartes de son deck dans l'ordre (`_actionIndex` séquentiel)
- [ ] Carte Action : cible l'allié joueur avec Provocation en priorité, sinon premier allié, sinon attaque directe au joueur
- [ ] Un allié monstre est créé en `ctx.MonsterAllies` si carte Allié
- [ ] Chaque action est séparée par une ReactiveWindowState (bouton "Passer")
- [ ] Fin du tour monstre → `ctx.Turn++`, transition vers DrawState

#### AC-2.6 — Fin de combat (CombatEndState)
- [ ] `ctx.PlayerHP <= 0` → `CombatResult.PlayerLose` → overlay "DÉFAITE" visible
- [ ] `ctx.MonsterHP <= 0` → `CombatResult.PlayerWin` → overlay "VICTOIRE" visible

#### AC-2.7 — KeywordResolver (service pur)
- [ ] `KeywordResolver` enregistré via ServiceLocator dans `BootLoader.RegisterServices()`
- [ ] `CanTarget(ground, vol)` = false
- [ ] `CanTarget(portee, vol)` = true
- [ ] `GetPriorityTarget` : Provocation cible en premier
- [ ] `CanBeBlocked(instantane)` = false
- [ ] `GetRitualCost` = 0 si pas de mot-clé Rituel

#### AC-2.8 — UI Toolkit
- [ ] 6 zones visibles dans Combat.unity : zone monstre, alliés monstre, zone de jeu, main, HUD, boutons
- [ ] Labels PV joueur, PV monstre, mana, numéro de tour se mettent à jour chaque frame
- [ ] CardView affiche nom et coût mana pour chaque carte en main

#### AC-2.9 — Tests Edit Mode
- [ ] `KeywordResolverTests` : 11 tests passent tous en mode Edit
- [ ] Aucun test n'utilise `new CardData()` directement — tous passent par `ScriptableObject.CreateInstance`

#### AC-2.10 — Build sans erreurs
- [ ] `Build > Build Settings > Build` ou Play Mode sans erreur de compilation
- [ ] Aucun `Debug.Log` direct — uniquement `GameLog.*`

## Additional Context

### Dependencies

- **Epic 1 complété** ✅ : `CardData`, `DeckData`, `GameRules`, `ServiceLocator` disponibles
- Aucune dépendance externe supplémentaire — tous les packages présents dans manifest.json

### Testing Strategy

- **Edit Mode** : `KeywordResolverTests` — logique pure sans runtime Unity
- Tests ciblant : `CanTarget` (Vol/Portée), `CanBlock` (Instantané), `GetPriorityTarget` (Provocation), `IsEveillee`, `GetRitualCost`
- Naming : `[Méthode]_[Condition]_[RésultatAttendu]`

### Notes

- `GameRules.PlayerStartingHP = 30` — placeholder confirmé pour Epic 2
- Les enum sans accents (`Portee`, `Instantane`, etc.) sont cohérents avec Epic 1
- `CombatManager` contient une référence `[SerializeField]` vers le deck joueur hardcodé et le `MonsterData` de test — remplacés par la navigation monde en Epic 4
