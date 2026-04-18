using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using MonsterCardGame.Core;
using MonsterCardGame.Gameplay.Cards;
using MonsterCardGame.Gameplay.Combat;
using MonsterCardGame.Gameplay.Combat.States;

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
        private Button        _attackBtn;
        private Button        _blockBtn;
        private Button        _passBtn;
        private VisualElement _pendingActionBanner;
        private Label         _pendingActionLabel;
        private VisualElement _resultOverlay;
        private Label         _resultLabel;

        private readonly List<CardView> _handViews            = new();
        private readonly List<CardView> _playerAllyViews      = new();
        private readonly List<CardView> _monsterAllyViews     = new();
        private CardView                _selectedCard         = null;
        private AlliedInstance          _selectedAttacker     = null;
        private CardView                _selectedAttackerView = null;
        private int                     _lastHandCount        = -1;
        private int                     _lastPlayerAllyCount  = -1;
        private int                     _lastMonsterAllyCount = -1;

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
            _attackBtn             = root.Q<Button>("attack-btn");
            _blockBtn              = root.Q<Button>("block-btn");
            _passBtn               = root.Q<Button>("pass-btn");
            _pendingActionBanner   = root.Q<VisualElement>("pending-action-banner");
            _pendingActionLabel    = root.Q<Label>("pending-action-label");
            _resultOverlay         = root.Q<VisualElement>("result-overlay");
            _resultLabel           = root.Q<Label>("result-label");

            _sacrificeBtn.clicked += OnSacrificeClicked;
            _endPlayBtn.clicked   += OnEndPlayClicked;
            _attackBtn.clicked    += OnAttackClicked;
            _blockBtn.clicked     += OnBlockClicked;
            _passBtn.clicked      += OnPassClicked;

            _cardDetailPopup = new CardDetailPopup();
            root.Add(_cardDetailPopup);
        }

        private void OnDisable()
        {
            if (_sacrificeBtn != null) _sacrificeBtn.clicked -= OnSacrificeClicked;
            if (_endPlayBtn   != null) _endPlayBtn.clicked   -= OnEndPlayClicked;
            if (_attackBtn    != null) _attackBtn.clicked    -= OnAttackClicked;
            if (_blockBtn     != null) _blockBtn.clicked     -= OnBlockClicked;
            if (_passBtn      != null) _passBtn.clicked      -= OnPassClicked;
        }

        private void Update()
        {
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
                        if (evt.button == 1) { _cardDetailPopup.Show(instance.Data); evt.StopPropagation(); return; }
                        if (evt.button == 0) { SelectAttacker(instance, view); evt.StopPropagation(); }
                    });

                    _playerAlliesZone.Add(view);
                    _playerAllyViews.Add(view);
                }
            }

            for (int i = 0; i < ctx.PlayerAllies.Count && i < _playerAllyViews.Count; i++)
            {
                if (ctx.PlayerAllies[i].IsSleeping)
                    _playerAllyViews[i].AddToClassList("ally-sleeping");
                else
                    _playerAllyViews[i].RemoveFromClassList("ally-sleeping");
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
                    if (evt.button == 0 && _selectedAttacker != null)
                    {
                        _combatManager.PlayState?.TryAttackWithAlly(
                            _combatManager.Context, _selectedAttacker, instance);
                        ClearAttackerSelection();
                        evt.StopPropagation();
                    }
                });

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
        }

        private void ClearAttackerSelection()
        {
            _selectedAttackerView?.RemoveFromClassList("attacker-selected");
            _selectedAttacker     = null;
            _selectedAttackerView = null;
        }

        private void TryPlaySelectedCard()
        {
            if (_selectedCard == null) return;
            if (_combatManager.CurrentState is not PlayState) return;
            _combatManager.PlayState.TryPlayCard(_combatManager.Context, _selectedCard.Data);
            ClearSelection();
        }

        private void ClearSelection()
        {
            _selectedCard?.RemoveFromClassList("selected");
            _selectedCard = null;
        }

        private void OnAttackClicked()
        {
            if (_selectedAttacker == null) return;
            _combatManager.PlayState?.TryAttackMonsterDirectly(_combatManager.Context, _selectedAttacker);
            ClearAttackerSelection();
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
            bool hasAttacker  = _selectedAttacker != null;

            _sacrificeBtn.SetEnabled(inSacrifice);
            _endPlayBtn.SetEnabled(inSacrifice || inPlay);
            _attackBtn.SetEnabled(inPlay && hasAttacker);
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
                _resultOverlay.AddToClassList("hidden");
            else
            {
                _resultOverlay.RemoveFromClassList("hidden");
                _resultLabel.text = ctx.Result == CombatResult.PlayerWin ? "VICTOIRE" : "DÉFAITE";
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
