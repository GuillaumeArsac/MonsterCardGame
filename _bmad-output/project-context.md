---
project_name: 'MonsterCardGame'
user_name: 'Guillaume'
date: '2026-04-05'
sections_completed: ['technology_stack', 'engine_rules', 'performance', 'organization', 'testing', 'platform', 'antipatterns']
status: 'complete'
rule_count: 35
optimized_for_llm: true
---

# Project Context for AI Agents

_Règles critiques et non-évidentes que les agents IA doivent suivre lors de l'implémentation du code de MonsterCardGame. Lire avant d'écrire tout code._

---

## Technology Stack & Versions

- **Engine :** Unity 6000.3.12f1 (Unity 6.3 LTS)
- **Render Pipeline :** URP 17.3.0 (`com.unity.render-pipelines.universal`)
- **Input :** New Input System 1.19.0 (`com.unity.inputsystem`) — legacy Input Manager désactivé
- **UI :** UI Toolkit (built-in Unity 6, `com.unity.modules.uielements`) — **PAS uGUI**
- **Tests :** Unity Test Framework 1.6.0
- **MCP Unity :** `com.gamelovers.mcp-unity` (déjà installé)
- **À installer :** DOTween (Asset Store), Steamworks.NET (GitHub), Unity Localization (Package Manager)
- **Note :** `com.unity.ugui` est présent dans le manifest comme dépendance par défaut Unity mais **N'EST PAS utilisé** pour l'UI du jeu — toute l'UI est en UI Toolkit

---

## Critical Implementation Rules

### Engine-Specific Rules (Unity)

**Lifecycle :**
- `Awake()` = auto-initialisation (références internes uniquement)
- `Start()` = initialisation cross-objets (après tous les Awake)
- Ne jamais dépendre de l'ordre d'exécution entre MonoBehaviours sans configurer explicitement `Script Execution Order`
- `Awaitable` est disponible nativement en Unity 6 — pas besoin de UniTask

**ScriptableObjects :**
- Les données de jeu (CardData, MonsterData, RecipeData, ZoneData) existent uniquement comme assets dans `Assets/Data/`
- **JAMAIS** `new CardData()` en runtime — toujours référencer des assets SO existants
- `[SerializeField]` préféré à `public` pour toute sérialisation Inspector
- `[Header("...")]` et `[Tooltip("...")]` obligatoires pour les groupes de champs SO

**Composants :**
- `GetComponent<T>()` se cache dans `Awake()`, **jamais dans Update()**
- `TryGetComponent<T>()` pour les dépendances optionnelles
- `CompareTag("Tag")` et non `gameObject.tag == "Tag"` (évite l'allocation)

**Async :**
- Unity 6 : utiliser `async Awaitable` pour l'I/O (chargement de scène, sauvegarde)
- Coroutines : pour les séquences frame-based (animations, délais)

**Assembly Definitions (4 assemblies) :**
```
Core.asmdef           → Scripts/Core/
Gameplay.asmdef       → Scripts/Combat/, Cards/, Craft/, World/  [ref: Core]
UI.asmdef             → Scripts/UI/                              [ref: Core, Gameplay]
Infrastructure.asmdef → Scripts/Save/, Audio/, Infrastructure/  [ref: Core]
```
Dépendances strictes : `Core ← Gameplay ← UI`, `Core ← Infrastructure`. Jamais l'inverse.

### Performance Rules

- **Cible :** 60fps / 16.7ms par frame
- **Update() :** Zéro allocation — pas de `new`, pas de LINQ, pas de concatenation de strings, pas de `foreach` sur collections non-List
- **Pas de `GetComponent` dans Update** — toujours mis en cache dans Awake
- **Pas de `FindObjectOfType`** dans le code gameplay — utiliser `Services.Get<>()`
- **CardView pooling :** Les visuels de cartes en main peuvent être poolés (CardViewFactory) pour éviter Instantiate/Destroy répétitifs
- **Sprites :** Utiliser des Sprite Atlases par zone pour réduire les draw calls

### Code Organization Rules

**Localisation des systèmes :**
- Toute donnée de jeu → `Assets/Data/[Système]/` (ScriptableObjects)
- Tout event inter-système → `Assets/Events/[Système]/` (SO Event Channels)
- Tout service global → enregistré via `Services.Register<IService>()` au démarrage
- Debug tools → dans `#if UNITY_EDITOR` uniquement, jamais en release

**Naming :**
- Classes/Méthodes : `PascalCase`
- Champs privés : `_camelCase`
- Interfaces : `IPascalCase`
- Assets SO : `[Type]_[Zone]_[Nom].asset` — ex. `Card_Plaines_ChargeBrutale.asset`
- Events SO : `Event_On[Sujet][Verbe].asset` — ex. `Event_OnCardPlayed.asset`
- Audio : `kebab-case` — ex. `zone-plaines-theme.ogg`

### Testing Rules

- **Edit Mode** (`Tests/EditMode/`) : logique pure sans runtime Unity
  → KeywordResolver, DeckValidator, CraftSystem, JsonSaveSystem, GameRules
- **Play Mode** (`Tests/PlayMode/`) : intégration avec ScriptableObjects réels
  → CombatStateMachine, ForgeSystem
- **Pas de mocks pour les ScriptableObjects** — créer des assets de test dédiés dans `Assets/Tests/TestData/`
- **Naming des tests :** `[Méthode]_[Condition]_[RésultatAttendu]`
  ex. `CanTarget_VolWithoutPortee_ReturnsFalse()`

### Platform & Build Rules

- **PC uniquement** (Steam) — pas de code mobile, pas de code console
- **Save :** `Application.persistentDataPath + "/save.json"` — jamais PlayerPrefs pour la progression principale
- **Steam :** Steamworks.NET pour achievements et Cloud Save
- **Debug tools :** `#if UNITY_EDITOR` — retirés automatiquement du build release
- **Localisation :** Unity Localization Package — FR langue par défaut, EN secondaire

### Critical Don't-Miss Rules (Anti-patterns)

```
❌ new CardData()              → ✅ Référencer un CardData asset SO existant
❌ Debug.Log(...)              → ✅ GameLog.Info("[Système]", "message")
❌ FindObjectOfType<T>()       → ✅ Services.Get<IService>()
❌ Sauvegarder mid-combat      → ✅ Sauvegarder uniquement post-combat (ADR-001)
❌ Valeur hardcodée en code    → ✅ GameRules.Constante ou GameBalanceConfig SO
❌ Resources.Load<CardData>()  → ✅ [SerializeField] CardData _card ou ref Data/
❌ Canvas uGUI pour l'UI jeu   → ✅ UI Toolkit (UXML/USS) uniquement
❌ KeywordResolver avec état   → ✅ Service pur (questions booléennes uniquement)
❌ GetComponent dans Update    → ✅ Cache dans Awake()
❌ Systèmes Gameplay se ref.   → ✅ SO Event Channels ou Services.Get<>()
```

**Systèmes spécifiques :**
- `KeywordResolver` est un service pur C# (pas MonoBehaviour). Ne modifie jamais l'état du combat — répond uniquement à des questions bool (`CanTarget`, `CanBlock`, etc.)
- `CraftSystem.TryCraft()` cherche dans TOUTES les recettes (découvertes ou non). L'état "découvert" est uniquement dans `SaveData` pour l'affichage UI.
- La `CombatStateMachine` interroge le `KeywordResolver` et prend les décisions — le resolver ne prend jamais de décisions de son propre chef.

---

## Usage Guidelines

**Pour les agents IA :**
- Lire ce fichier avant d'implémenter tout code du projet
- Suivre TOUTES les règles exactement telles que documentées
- En cas de doute, préférer l'option la plus restrictive
- Mettre à jour ce fichier si de nouveaux patterns émergent

**Pour Guillaume :**
- Garder ce fichier lean et focalisé sur les besoins des agents
- Mettre à jour lors de changements du stack technologique
- Supprimer les règles qui deviennent évidentes avec le temps

_Last Updated: 2026-04-05_
