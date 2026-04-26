using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using MonsterCardGame.Core;
using MonsterCardGame.Gameplay.Cards;
using MonsterCardGame.Gameplay.Combat;
using MonsterCardGame.Gameplay.Combat.States;
using MonsterCardGame.Gameplay.Inventory;

namespace MonsterCardGame.UI.Combat
{
    [RequireComponent(typeof(UIDocument))]
    public class CombatBoardUI : MonoBehaviour
    {
        [SerializeField, Tooltip("CombatManager de la scène")]
        private CombatManager _combatManager;

        private Label         _monsterHPLabel;
        private Label         _playerHPLabel;
        private Label         _playerManaLabel;
        private Label         _turnLabel;
        private VisualElement _phaseBanner;
        private Label         _activePlayerLabel;
        private Label         _phaseNameLabel;
        private VisualElement _monsterAlliesZone;
        private VisualElement _playerAlliesZone;
        private Label         _monsterDeckCountLabel;
        private Label         _playerHandCountLabel;
        private VisualElement _handCards;
        private Button        _sacrificeBtn;
        private Button        _endPlayBtn;
        private Button        _blockBtn;
        private Button        _passBtn;
        private VisualElement _pendingActionBanner;
        private Label         _pendingActionLabel;
        private VisualElement _resultOverlay;
        private Label         _resultLabel;
        private VisualElement _lootList;
        private bool          _lootDisplayed;

        private readonly List<CardView> _handViews            = new();
        private readonly List<CardView> _playerAllyViews      = new();
        private readonly List<CardView> _monsterAllyViews     = new();
        private CardView                _selectedCard         = null;
        private AlliedInstance          _selectedAttacker     = null;
        private CardView                _selectedAttackerView = null;
        private int                     _lastHandCount        = -1;
        private int                     _lastPlayerAllyCount  = -1;
        private int                     _lastMonsterAllyCount = -1;

        private VisualElement   _monsterZone;
        private VisualElement   _targetingBanner;
        private Label           _targetingLabel;
        private bool            _isTargeting;
        private CardData        _pendingActionCard;

        private Button          _cemeteryBtn;
        private VisualElement   _cemeteryPanel;
        private VisualElement   _cemeteryList;
        private int             _lastCemeteryCount = -1;

        private CardDetailPopup _cardDetailPopup;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            _monsterHPLabel        = root.Q<Label>("monster-hp-label");
            _playerHPLabel         = root.Q<Label>("player-hp-label");
            _playerManaLabel       = root.Q<Label>("player-mana-label");
            _turnLabel             = root.Q<Label>("turn-label");
            _phaseBanner           = root.Q<VisualElement>("phase-banner");
            _activePlayerLabel     = root.Q<Label>("active-player-label");
            _phaseNameLabel        = root.Q<Label>("phase-name-label");
            _monsterAlliesZone     = root.Q<VisualElement>("monster-allies-zone");
            _playerAlliesZone      = root.Q<VisualElement>("player-allies-zone");
            _monsterDeckCountLabel = root.Q<Label>("monster-deck-count-label");
            _playerHandCountLabel  = root.Q<Label>("player-hand-count-label");
            _handCards             = root.Q<VisualElement>("hand-cards");
            _sacrificeBtn          = root.Q<Button>("sacrifice-btn");
            _endPlayBtn            = root.Q<Button>("end-play-btn");
            _blockBtn              = root.Q<Button>("block-btn");
            _passBtn               = root.Q<Button>("pass-btn");
            _pendingActionBanner   = root.Q<VisualElement>("pending-action-banner");
            _pendingActionLabel    = root.Q<Label>("pending-action-label");
            _resultOverlay         = root.Q<VisualElement>("result-overlay");
            _resultLabel           = root.Q<Label>("result-label");
            _lootList              = root.Q<VisualElement>("loot-list");

            _monsterZone     = root.Q<VisualElement>("monster-zone");
            _targetingBanner = root.Q<VisualElement>("targeting-banner");
            _targetingLabel  = root.Q<Label>("targeting-label");
            _cemeteryBtn     = root.Q<Button>("cemetery-btn");
            _cemeteryPanel   = root.Q<VisualElement>("cemetery-panel");
            _cemeteryList    = root.Q<VisualElement>("cemetery-list");

            _sacrificeBtn.clicked += OnSacrificeClicked;
            _endPlayBtn.clicked   += OnEndPlayClicked;
            _blockBtn.clicked     += OnBlockClicked;
            _passBtn.clicked      += OnPassClicked;
            root.Q<Button>("cancel-target-btn").clicked += OnCancelTargeting;
            root.Q<Button>("forge-btn").clicked         += () => SceneManager.LoadScene("Forge");
            _cemeteryBtn.clicked                        += ToggleCemeteryPanel;
            root.Q<Button>("cemetery-close-btn").clicked += CloseCemeteryPanel;

            _monsterZone.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button != 0) return;
                if (_isTargeting)
                {
                    ResolveTargeting(CombatTarget.Monster);
                    evt.StopPropagation();
                }
                else if (_selectedAttacker != null)
                {
                    _combatManager.PlayState?.TryAttackMonsterDirectly(_combatManager.Context, _selectedAttacker);
                    ClearAttackerSelection();
                    evt.StopPropagation();
                }
            });

            _cardDetailPopup = new CardDetailPopup();
            root.Add(_cardDetailPopup);
        }

        private void OnDisable()
        {
            if (_sacrificeBtn != null) _sacrificeBtn.clicked -= OnSacrificeClicked;
            if (_endPlayBtn   != null) _endPlayBtn.clicked   -= OnEndPlayClicked;
            if (_blockBtn     != null) _blockBtn.clicked     -= OnBlockClicked;
            if (_passBtn      != null) _passBtn.clicked      -= OnPassClicked;
        }

        private void Update()
        {
            if (_isTargeting && _combatManager.CurrentState is not PlayState)
                ExitTargetingMode();

            if (_selectedAttacker != null && _selectedAttacker.IsSleeping)
                ClearAttackerSelection();

            var ctx = _combatManager?.Context;
            if (ctx == null) return;

            RefreshLabels(ctx);
            RefreshPhaseBanner(ctx, _combatManager.CurrentState);
            RefreshHand(ctx);
            RefreshPlayerAllies(ctx);
            RefreshMonsterAllies(ctx);
            RefreshButtons(ctx, _combatManager.CurrentState);
            RefreshPendingActionBanner(ctx);
            RefreshResultOverlay(ctx);
            RefreshCemeteryButton(ctx);
        }

        private void RefreshLabels(CombatContext ctx)
        {
            _monsterHPLabel.text        = $"PV: {ctx.MonsterHP}";
            _playerHPLabel.text         = $"PV: {ctx.PlayerHP}";
            _playerManaLabel.text       = $"Mana: {ctx.PlayerMana}";
            _turnLabel.text             = $"Tour {ctx.Turn}";
            _monsterDeckCountLabel.text = $"Deck: {ctx.MonsterDeck.Count}";
            _playerHandCountLabel.text  = $"{ctx.PlayerHand.Count} carte(s)";
        }

        private void RefreshHand(CombatContext ctx)
        {
            if (ctx.PlayerHand.Count == _lastHandCount) return;

            _handCards.Clear();
            _handViews.Clear();
            _selectedCard  = null;
            _lastHandCount = ctx.PlayerHand.Count;

            foreach (var card in ctx.PlayerHand)
            {
                var view = new CardView(card);
                RegisterCardInteraction(view);
                _handCards.Add(view);
                _handViews.Add(view);
            }
        }

        private void RefreshPlayerAllies(CombatContext ctx)
        {
            if (ctx.PlayerAllies.Count != _lastPlayerAllyCount)
            {
                ClearAttackerSelection();
                _playerAlliesZone.Clear();
                _playerAllyViews.Clear();
                _lastPlayerAllyCount = ctx.PlayerAllies.Count;

                foreach (var ally in ctx.PlayerAllies)
                {
                    var instance = ally;
                    var view     = new CardView(instance.Data);
                    view.AddToClassList("ally-view");

                    view.RegisterCallback<PointerDownEvent>(evt =>
                    {
                        if (evt.button == 1) { _cardDetailPopup.Show(instance); evt.StopPropagation(); return; }
                        if (evt.button == 0)
                        {
                            if (_isTargeting) { ResolveTargeting(CombatTarget.ForAlly(instance)); evt.StopPropagation(); return; }
                            SelectAttacker(instance, view);
                            evt.StopPropagation();
                        }
                    });

                    _playerAlliesZone.Add(view);
                    _playerAllyViews.Add(view);
                }
            }

            for (int i = 0; i < ctx.PlayerAllies.Count && i < _playerAllyViews.Count; i++)
            {
                var ally = ctx.PlayerAllies[i];
                var view = _playerAllyViews[i];

                if (ally.IsSleeping)
                    view.AddToClassList("ally-sleeping");
                else
                    view.RemoveFromClassList("ally-sleeping");

                view.RefreshInstance(ally);
            }
        }

        private void RefreshMonsterAllies(CombatContext ctx)
        {
            if (ctx.MonsterAllies.Count == _lastMonsterAllyCount) return;

            _monsterAlliesZone.Clear();
            _monsterAllyViews.Clear();
            _lastMonsterAllyCount = ctx.MonsterAllies.Count;

            foreach (var ally in ctx.MonsterAllies)
            {
                var instance = ally;
                var view     = new CardView(instance.Data);
                view.AddToClassList("ally-view");
                view.AddToClassList("ally-view--enemy");

                view.RegisterCallback<PointerDownEvent>(evt =>
                {
                    if (evt.button == 1) { _cardDetailPopup.Show(instance.Data); evt.StopPropagation(); return; }
                    if (evt.button == 0)
                    {
                        if (_isTargeting) { ResolveTargeting(CombatTarget.ForAlly(instance)); evt.StopPropagation(); return; }
                        if (_selectedAttacker != null)
                        {
                            _combatManager.PlayState?.TryAttackWithAlly(_combatManager.Context, _selectedAttacker, instance);
                            ClearAttackerSelection();
                            evt.StopPropagation();
                        }
                    }
                });

                if (_selectedAttacker != null || _isTargeting)
                    view.AddToClassList("targeting-highlight");

                _monsterAlliesZone.Add(view);
                _monsterAllyViews.Add(view);
            }
        }

        private void RegisterCardInteraction(CardView view)
        {
            view.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button == 1)
                {
                    _cardDetailPopup.Show(view.Data);
                    evt.StopPropagation();
                    return;
                }

                ClearSelection();
                ClearAttackerSelection();
                _selectedCard = view;
                view.AddToClassList("selected");
                evt.StopPropagation();
            });

            view.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (evt.button == 0 && _selectedCard == view)
                    TryPlaySelectedCard();
                evt.StopPropagation();
            });
        }

        private void SelectAttacker(AlliedInstance ally, CardView view)
        {
            ClearSelection();
            ClearAttackerSelection();
            _selectedAttacker     = ally;
            _selectedAttackerView = view;
            view.AddToClassList("attacker-selected");
            ApplyAttackHighlights();
        }

        private void ClearAttackerSelection()
        {
            _selectedAttackerView?.RemoveFromClassList("attacker-selected");
            _selectedAttacker     = null;
            _selectedAttackerView = null;
            RemoveAttackHighlights();
            if (!_isTargeting) HideTargetingBanner();
        }

        private void ApplyAttackHighlights()
        {
            var ctx = _combatManager.Context;

            _targetingBanner.RemoveFromClassList("hidden");
            _targetingLabel.text = $"{_selectedAttacker.Data.CardName} ({_selectedAttacker.ATK} ATK) — choisissez une cible";

            bool canGoDirectly = !HasProvocationBlocker(ctx);
            if (canGoDirectly) _monsterZone.AddToClassList("targeting-highlight");

            foreach (var v in _monsterAllyViews) v.AddToClassList("targeting-highlight");
        }

        private void RemoveAttackHighlights()
        {
            _monsterZone.RemoveFromClassList("targeting-highlight");
            foreach (var v in _monsterAllyViews) v.RemoveFromClassList("targeting-highlight");
        }

        private void HideTargetingBanner() => _targetingBanner.AddToClassList("hidden");

        private static bool HasProvocationBlocker(CombatContext ctx)
        {
            if (ctx == null) return false;
            foreach (var ally in ctx.MonsterAllies)
                if (ally.Data.HasKeyword(Keyword.Provocation)) return true;
            return false;
        }

        private void TryPlaySelectedCard()
        {
            if (_selectedCard == null) return;
            if (_combatManager.CurrentState is not PlayState) return;
            var card = _selectedCard.Data;
            ClearSelection();
            if (card.CardType == CardType.Action)
                EnterTargetingMode(card);
            else
                _combatManager.PlayState.TryPlayCard(_combatManager.Context, card);
        }

        private void EnterTargetingMode(CardData card)
        {
            _isTargeting       = true;
            _pendingActionCard = card;
            _targetingBanner.RemoveFromClassList("hidden");
            _targetingLabel.text = $"{card.CardName} ({card.Attack} ATK) — choisissez une cible";
            _monsterZone.AddToClassList("targeting-highlight");
            foreach (var v in _playerAllyViews)  v.AddToClassList("targeting-highlight");
            foreach (var v in _monsterAllyViews) v.AddToClassList("targeting-highlight");
        }

        private void ExitTargetingMode()
        {
            _isTargeting       = false;
            _pendingActionCard = null;
            _monsterZone.RemoveFromClassList("targeting-highlight");
            foreach (var v in _playerAllyViews)  v.RemoveFromClassList("targeting-highlight");
            foreach (var v in _monsterAllyViews) v.RemoveFromClassList("targeting-highlight");
            if (_selectedAttacker == null) HideTargetingBanner();
        }

        private void OnCancelTargeting()
        {
            if (_isTargeting) ExitTargetingMode();
            ClearAttackerSelection();
        }

        private void ResolveTargeting(CombatTarget target)
        {
            var card = _pendingActionCard;
            ExitTargetingMode();
            _combatManager.PlayState?.TryPlayCard(_combatManager.Context, card, target);
        }

        private void ClearSelection()
        {
            _selectedCard?.RemoveFromClassList("selected");
            _selectedCard = null;
        }


        private void RefreshPhaseBanner(CombatContext ctx, ICombatState state)
        {
            bool isPlayerTurn = state is DrawState or SacrificeState or PlayState or ReactiveWindowState;

            _activePlayerLabel.text = isPlayerTurn ? "Joueur" : "Monstre";

            _phaseBanner.RemoveFromClassList("phase-banner--player");
            _phaseBanner.RemoveFromClassList("phase-banner--monster");
            _phaseBanner.AddToClassList(isPlayerTurn ? "phase-banner--player" : "phase-banner--monster");

            _phaseNameLabel.text = state switch
            {
                DrawState           => "Pioche",
                SacrificeState      => "Sacrifice",
                PlayState           => "Action",
                ReactiveWindowState => "Réaction",
                MonsterTurnState    => "Attaque",
                CombatEndState      => ctx.Result == CombatResult.PlayerWin ? "Victoire" : "Défaite",
                _                   => "—"
            };
        }

        private void RefreshButtons(CombatContext ctx, ICombatState state)
        {
            bool inSacrifice  = state is SacrificeState;
            bool inPlay       = state is PlayState;
            bool inReactive   = state is ReactiveWindowState;
            bool hasPending   = ctx.PendingMonsterAction != null;
            bool hasBlockCard = _selectedCard?.Data.CardType == CardType.Blocage;
            _sacrificeBtn.SetEnabled(inSacrifice);
            _endPlayBtn.SetEnabled(inSacrifice || inPlay);
            _blockBtn.SetEnabled(inReactive && hasPending && hasBlockCard);
            _passBtn.SetEnabled(inReactive);
        }

        private void RefreshPendingActionBanner(CombatContext ctx)
        {
            if (ctx.PendingMonsterAction == null)
            {
                _pendingActionBanner.AddToClassList("hidden");
                return;
            }

            _pendingActionBanner.RemoveFromClassList("hidden");
            var targetName = ctx.PendingMonsterTarget != null
                ? ctx.PendingMonsterTarget.Data.CardName
                : "le joueur";
            _pendingActionLabel.text =
                $"Le monstre joue {ctx.PendingMonsterAction.CardName} ({ctx.PendingMonsterAction.Attack} ATK) → {targetName}";
        }

        private void RefreshResultOverlay(CombatContext ctx)
        {
            if (ctx.Result == CombatResult.None)
            {
                _resultOverlay.AddToClassList("hidden");
                _lootDisplayed = false;
                return;
            }

            _resultOverlay.RemoveFromClassList("hidden");
            _resultLabel.text = ctx.Result == CombatResult.PlayerWin ? "VICTOIRE !" : "DÉFAITE";

            if (!_lootDisplayed && ctx.Result == CombatResult.PlayerWin
                && _combatManager.CurrentState is CombatEndState)
            {
                _lootDisplayed = true;
                BuildLootList(ctx);
            }
        }

        private void BuildLootList(CombatContext ctx)
        {
            _lootList.Clear();

            if (ctx.DroppedMaterials.Count == 0)
            {
                var noneLabel = new Label("Aucun matériau obtenu");
                noneLabel.AddToClassList("loot-item");
                noneLabel.AddToClassList("loot-item--commun");
                _lootList.Add(noneLabel);
                return;
            }

            var header = new Label("Matériaux obtenus :");
            header.AddToClassList("loot-header");
            _lootList.Add(header);

            // Regrouper les doublons
            var grouped = new Dictionary<MaterialData, int>();
            foreach (var mat in ctx.DroppedMaterials)
            {
                grouped.TryGetValue(mat, out int count);
                grouped[mat] = count + 1;
            }

            foreach (var (mat, count) in grouped)
            {
                var label = new Label($"× {count}  {mat.MaterialName}");
                label.AddToClassList("loot-item");
                label.AddToClassList(mat.Rarity == MaterialRarity.Rare ? "loot-item--rare" : "loot-item--commun");
                _lootList.Add(label);
            }
        }

        // ── Cimetière ─────────────────────────────────────────────────────

        private void RefreshCemeteryButton(CombatContext ctx)
        {
            int count = ctx.PlayerCemetery.Count;
            _cemeteryBtn.text = $"Cimetière ({count})";

            if (count == _lastCemeteryCount) return;
            _lastCemeteryCount = count;

            if (_cemeteryPanel != null && !_cemeteryPanel.ClassListContains("hidden"))
                BuildCemeteryList(ctx);
        }

        private void ToggleCemeteryPanel()
        {
            if (_cemeteryPanel.ClassListContains("hidden"))
            {
                BuildCemeteryList(_combatManager.Context);
                _cemeteryPanel.RemoveFromClassList("hidden");
            }
            else
            {
                CloseCemeteryPanel();
            }
        }

        private void CloseCemeteryPanel() => _cemeteryPanel.AddToClassList("hidden");

        private void BuildCemeteryList(CombatContext ctx)
        {
            _cemeteryList.Clear();

            if (ctx.PlayerCemetery.Count == 0)
            {
                var empty = new Label("Cimetière vide.");
                empty.AddToClassList("cemetery-card-name");
                _cemeteryList.Add(empty);
                return;
            }

            bool inPlay = _combatManager.CurrentState is PlayState;

            foreach (var card in ctx.PlayerCemetery)
            {
                var c        = card;
                bool rampant = card.HasKeyword(Keyword.Rampant) && card.CardType == CardType.Allie;
                bool canPlay = rampant && inPlay && ctx.PlayerMana >= card.ManaCost;

                var row = new VisualElement();
                row.AddToClassList("cemetery-row");
                if (rampant) row.AddToClassList("cemetery-row--rampant");

                var nameLabel = new Label(card.CardName);
                nameLabel.AddToClassList("cemetery-card-name");
                row.Add(nameLabel);

                if (rampant)
                {
                    var badge = new Label("Rampant");
                    badge.AddToClassList("cemetery-rampant-badge");
                    row.Add(badge);

                    var costLabel = new Label($"{card.ManaCost}m");
                    costLabel.AddToClassList("cemetery-cost-label");
                    if (!canPlay) costLabel.style.opacity = 0.45f;
                    row.Add(costLabel);
                }

                if (canPlay)
                {
                    row.RegisterCallback<PointerDownEvent>(evt =>
                    {
                        if (evt.button != 0) return;
                        _combatManager.PlayState.TryPlayFromCemetery(_combatManager.Context, c);
                        CloseCemeteryPanel();
                        evt.StopPropagation();
                    });
                }

                _cemeteryList.Add(row);
            }
        }

        private void OnSacrificeClicked()
        {
            if (_selectedCard == null)
            {
                GameLog.Warning("CombatBoardUI", "Aucune carte sélectionnée pour le sacrifice");
                return;
            }
            _combatManager.SacrificeState?.TrySacrifice(_combatManager.Context, _selectedCard.Data);
            ClearSelection();
        }

        private void OnEndPlayClicked()
        {
            ClearSelection();
            var state = _combatManager.CurrentState;
            if (state is SacrificeState ss)
                ss.Skip(_combatManager.Context);
            else
                _combatManager.PlayState?.EndPlay(_combatManager.Context);
        }

        private void OnBlockClicked()
        {
            if (_selectedCard == null)
            {
                GameLog.Warning("CombatBoardUI", "Aucune carte Blocage sélectionnée");
                return;
            }
            _combatManager.ReactiveState?.TryBlock(_combatManager.Context, _selectedCard.Data);
            ClearSelection();
        }

        private void OnPassClicked()
            => _combatManager.ReactiveState?.Pass(_combatManager.Context);
    }
}
