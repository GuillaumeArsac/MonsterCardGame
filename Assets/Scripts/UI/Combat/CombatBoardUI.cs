using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using MonsterCardGame.Core;
using MonsterCardGame.Core.Services;
using MonsterCardGame.Gameplay.Cards;
using MonsterCardGame.Gameplay.Combat;
using MonsterCardGame.Gameplay.Combat.Keywords;
using MonsterCardGame.Gameplay.Combat.States;
using MonsterCardGame.Gameplay.Inventory;
using MonsterCardGame.UI.Combat.Board;

namespace MonsterCardGame.UI.Combat
{
    /// <summary>
    /// Binder du combat : relie le <see cref="CombatManager"/> aux composants 2D
    /// (<see cref="CombatHUD"/>, <see cref="PlayerHandController"/>, <see cref="AlliesController"/>,
    /// <see cref="CemeteryController"/>, <see cref="CardDetailPopup2D"/>). Lit l'état du
    /// contexte chaque frame et pilote l'affichage ; les composants ne contiennent
    /// aucune règle de jeu.
    /// </summary>
    public class CombatBoardUI : MonoBehaviour
    {
        [Header("Logique")]
        [SerializeField] private CombatManager _combatManager;

        [Header("Vues")]
        [SerializeField] private CombatHUD           _hud;
        [SerializeField] private PlayerHandController _playerHand;
        [SerializeField] private AlliesController     _playerAllies;
        [SerializeField] private AlliesController     _monsterAllies;
        [SerializeField] private CemeteryController   _cemetery;
        [SerializeField] private CardDetailPopup2D    _detailPopup;

        [Header("Loot (overlay résultat)")]
        [SerializeField] private TMP_Text _lootEntryPrefab;
        [SerializeField] private Color    _lootCommonColor = new(0.85f, 0.85f, 0.85f);
        [SerializeField] private Color    _lootRareColor   = new(0.95f, 0.80f, 0.30f);

        // ── État ──────────────────────────────────────────────────────────

        private CardData         _selectedHandData;
        private AlliedInstance   _selectedAttacker;
        private bool             _isTargeting;
        private CardData         _pendingActionCard;
        private IKeywordResolver _resolver;
        private bool             _lootDisplayed;
        private int              _lastCemeteryCount = -1;

        // ── Cycle de vie ──────────────────────────────────────────────────

        private void OnEnable()
        {
            _playerHand.SelectionChanged += OnHandSelectionChanged;
            _playerHand.CardActivated    += OnHandCardActivated;
            _playerHand.CardRightClicked += OnHandCardRightClicked;

            _playerAllies.AllyClicked       += OnPlayerAllyClicked;
            _playerAllies.AllyRightClicked  += OnPlayerAllyRightClicked;
            _monsterAllies.AllyClicked      += OnMonsterAllyClicked;
            _monsterAllies.AllyRightClicked += OnMonsterAllyRightClicked;

            _hud.SacrificeClicked       += OnSacrificeClicked;
            _hud.EndPlayClicked         += OnEndPlayClicked;
            _hud.BlockClicked           += OnBlockClicked;
            _hud.PassClicked            += OnPassClicked;
            _hud.CemeteryOpenClicked    += ToggleCemeteryPanel;
            _hud.CemeteryCloseClicked   += CloseCemeteryPanel;
            _hud.CancelTargetingClicked += OnCancelTargeting;
            _hud.ForgeClicked           += OnForgeClicked;
            _hud.MonsterZoneClicked     += OnMonsterZoneClicked;

            _cemetery.PlayFromCemeteryRequested += OnPlayFromCemetery;
        }

        private void OnDisable()
        {
            _playerHand.SelectionChanged -= OnHandSelectionChanged;
            _playerHand.CardActivated    -= OnHandCardActivated;
            _playerHand.CardRightClicked -= OnHandCardRightClicked;

            _playerAllies.AllyClicked       -= OnPlayerAllyClicked;
            _playerAllies.AllyRightClicked  -= OnPlayerAllyRightClicked;
            _monsterAllies.AllyClicked      -= OnMonsterAllyClicked;
            _monsterAllies.AllyRightClicked -= OnMonsterAllyRightClicked;

            _hud.SacrificeClicked       -= OnSacrificeClicked;
            _hud.EndPlayClicked         -= OnEndPlayClicked;
            _hud.BlockClicked           -= OnBlockClicked;
            _hud.PassClicked            -= OnPassClicked;
            _hud.CemeteryOpenClicked    -= ToggleCemeteryPanel;
            _hud.CemeteryCloseClicked   -= CloseCemeteryPanel;
            _hud.CancelTargetingClicked -= OnCancelTargeting;
            _hud.ForgeClicked           -= OnForgeClicked;
            _hud.MonsterZoneClicked     -= OnMonsterZoneClicked;

            _cemetery.PlayFromCemeteryRequested -= OnPlayFromCemetery;
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
            _playerAllies.Sync(ctx.PlayerAllies);
            _monsterAllies.Sync(ctx.MonsterAllies);
            RefreshHighlights(ctx);
            RefreshButtons(ctx, _combatManager.CurrentState);
            RefreshPendingActionBanner(ctx);
            RefreshResultOverlay(ctx);
            RefreshCemeteryButton(ctx);
        }

        // ── Main joueur ───────────────────────────────────────────────────

        private void OnHandSelectionChanged(CardData data)
        {
            _selectedHandData = data;
            if (data != null) ClearAttackerSelection();
        }

        private void OnHandCardActivated(CardData data)
        {
            if (_combatManager.CurrentState is not PlayState) return;

            if (data.CardType == CardType.Action)
                EnterTargetingMode(data);
            else
                _combatManager.PlayState.TryPlayCard(_combatManager.Context, data);

            _playerHand.ClearSelection();
        }

        private void OnHandCardRightClicked(CardData data) => _detailPopup.Show(data);

        // ── Alliés ────────────────────────────────────────────────────────

        private void OnPlayerAllyClicked(AlliedInstance instance)
        {
            if (_isTargeting) { ResolveTargeting(CombatTarget.ForAlly(instance)); return; }
            SelectAttacker(instance);
        }

        private void OnPlayerAllyRightClicked(AlliedInstance instance) => _detailPopup.Show(instance);

        private void OnMonsterAllyClicked(AlliedInstance instance)
        {
            if (_isTargeting) { ResolveTargeting(CombatTarget.ForAlly(instance)); return; }

            if (_selectedAttacker != null)
            {
                _combatManager.PlayState?.TryAttackWithAlly(_combatManager.Context, _selectedAttacker, instance);
                ClearAttackerSelection();
            }
        }

        private void OnMonsterAllyRightClicked(AlliedInstance instance) => _detailPopup.Show(instance.Data);

        private void OnMonsterZoneClicked()
        {
            if (_isTargeting) { ResolveTargeting(CombatTarget.Monster); return; }

            if (_selectedAttacker != null)
            {
                _combatManager.PlayState?.TryAttackMonsterDirectly(_combatManager.Context, _selectedAttacker);
                ClearAttackerSelection();
            }
        }

        // ── Sélection attaquant / ciblage ─────────────────────────────────

        private void SelectAttacker(AlliedInstance ally)
        {
            _playerHand.ClearSelection();
            _selectedAttacker = ally;
            _hud.ShowTargetingBanner($"{ally.Data.CardName} ({ally.ATK} ATK) — choisissez une cible");
        }

        private void ClearAttackerSelection()
        {
            _selectedAttacker = null;
            _playerAllies.ClearAttacker();
            if (!_isTargeting) _hud.HideTargetingBanner();
        }

        private void EnterTargetingMode(CardData card)
        {
            _isTargeting       = true;
            _pendingActionCard = card;
            _hud.ShowTargetingBanner($"{card.CardName} ({card.Attack} ATK) — choisissez une cible");
        }

        private void ExitTargetingMode()
        {
            _isTargeting       = false;
            _pendingActionCard = null;
            if (_selectedAttacker == null) _hud.HideTargetingBanner();
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

        /// <summary>Réapplique chaque frame les surbrillances (les alliés sont reconstruits par les contrôleurs).</summary>
        private void RefreshHighlights(CombatContext ctx)
        {
            _playerAllies.ClearTargetable();
            _monsterAllies.ClearTargetable();
            _hud.SetMonsterZoneHighlight(false);

            if (_isTargeting)
            {
                _hud.SetMonsterZoneHighlight(true);
                foreach (var a in ctx.PlayerAllies)  _playerAllies.SetTargetable(a, true);
                foreach (var a in ctx.MonsterAllies) _monsterAllies.SetTargetable(a, true);
            }
            else if (_selectedAttacker != null)
            {
                _playerAllies.SetAttacker(_selectedAttacker);

                _resolver ??= Services.Get<IKeywordResolver>();
                if (!HasProvocationBlocker(ctx, _selectedAttacker))
                    _hud.SetMonsterZoneHighlight(true);

                foreach (var enemy in ctx.MonsterAllies)
                    if (_resolver.CanTarget(_selectedAttacker, enemy))
                        _monsterAllies.SetTargetable(enemy, true);
            }
            else
            {
                _playerAllies.ClearAttacker();
            }
        }

        private bool HasProvocationBlocker(CombatContext ctx, AlliedInstance attacker)
        {
            _resolver ??= Services.Get<IKeywordResolver>();
            if (ctx == null) return false;
            foreach (var ally in ctx.MonsterAllies)
                if (ally.Data.HasKeyword(Keyword.Provocation) && _resolver.CanTarget(attacker, ally))
                    return true;
            return false;
        }

        // ── Labels / bannières ────────────────────────────────────────────

        private void RefreshLabels(CombatContext ctx)
        {
            _hud.SetMonsterHP(ctx.MonsterHP);
            _hud.SetPlayerHP(ctx.PlayerHP);
            _hud.SetPlayerMana(ctx.PlayerMana);
            _hud.SetTurn(ctx.Turn);
            _hud.SetMonsterDeckCount(ctx.MonsterDeck.Count);
            _hud.SetPlayerHandCount(ctx.PlayerHand.Count);
        }

        private void RefreshPhaseBanner(CombatContext ctx, ICombatState state)
        {
            bool isPlayerTurn = state is DrawState or SacrificeState or PlayState or ReactiveWindowState;

            string phaseName = state switch
            {
                DrawState           => "Pioche",
                SacrificeState      => "Sacrifice",
                PlayState           => "Action",
                ReactiveWindowState => "Réaction",
                MonsterTurnState    => "Attaque",
                CombatEndState      => ctx.Result == CombatResult.PlayerWin ? "Victoire" : "Défaite",
                _                   => "—"
            };

            _hud.SetPhase(isPlayerTurn ? "Joueur" : "Monstre", phaseName, isPlayerTurn);
        }

        private void RefreshButtons(CombatContext ctx, ICombatState state)
        {
            bool inSacrifice  = state is SacrificeState;
            bool inPlay       = state is PlayState;
            bool inReactive   = state is ReactiveWindowState;
            bool hasPending   = ctx.PendingMonsterAction != null;
            bool hasBlockCard = _selectedHandData?.CardType == CardType.Blocage;

            _hud.SetSacrificeEnabled(inSacrifice);
            _hud.SetEndPlayEnabled(inSacrifice || inPlay);
            _hud.SetBlockEnabled(inReactive && hasPending && hasBlockCard);
            _hud.SetPassEnabled(inReactive);
        }

        private void RefreshPendingActionBanner(CombatContext ctx)
        {
            if (ctx.PendingMonsterAction == null)
            {
                _hud.HidePendingActionBanner();
                return;
            }

            var targetName = ctx.PendingMonsterTarget != null
                ? ctx.PendingMonsterTarget.Data.CardName
                : "le joueur";
            _hud.ShowPendingActionBanner(
                $"Le monstre joue {ctx.PendingMonsterAction.CardName} ({ctx.PendingMonsterAction.Attack} ATK) → {targetName}");
        }

        // ── Overlay résultat / loot ───────────────────────────────────────

        private void RefreshResultOverlay(CombatContext ctx)
        {
            if (ctx.Result == CombatResult.None)
            {
                _hud.HideResultOverlay();
                _lootDisplayed = false;
                return;
            }

            _hud.ShowResultOverlay(ctx.Result == CombatResult.PlayerWin ? "VICTOIRE !" : "DÉFAITE");

            if (!_lootDisplayed && ctx.Result == CombatResult.PlayerWin
                && _combatManager.CurrentState is CombatEndState)
            {
                _lootDisplayed = true;
                BuildLootList(ctx);
            }
        }

        private void BuildLootList(CombatContext ctx)
        {
            var root = _hud.LootListRoot;
            if (root == null || _lootEntryPrefab == null) return;

            for (int i = root.childCount - 1; i >= 0; i--)
                Destroy(root.GetChild(i).gameObject);

            if (ctx.DroppedMaterials.Count == 0)
            {
                AddLootEntry(root, "Aucun matériau obtenu", _lootCommonColor);
                return;
            }

            AddLootEntry(root, "Matériaux obtenus :", Color.white);

            var grouped = new Dictionary<MaterialData, int>();
            foreach (var mat in ctx.DroppedMaterials)
            {
                grouped.TryGetValue(mat, out int count);
                grouped[mat] = count + 1;
            }

            foreach (var (mat, count) in grouped)
            {
                var color = mat.Rarity == MaterialRarity.Rare ? _lootRareColor : _lootCommonColor;
                AddLootEntry(root, $"× {count}  {mat.MaterialName}", color);
            }
        }

        private void AddLootEntry(Transform root, string text, Color color)
        {
            var entry = Instantiate(_lootEntryPrefab, root);
            entry.text  = text;
            entry.color = color;
        }

        // ── Cimetière ─────────────────────────────────────────────────────

        private void RefreshCemeteryButton(CombatContext ctx)
        {
            int count = ctx.PlayerCemetery.Count;
            _hud.SetCemeteryButtonText(count);

            if (count == _lastCemeteryCount) return;
            _lastCemeteryCount = count;

            if (_hud.IsCemeteryPanelOpen) RebuildCemetery(ctx);
        }

        private void ToggleCemeteryPanel()
        {
            if (_hud.IsCemeteryPanelOpen)
            {
                CloseCemeteryPanel();
            }
            else
            {
                RebuildCemetery(_combatManager.Context);
                _hud.OpenCemeteryPanel();
            }
        }

        private void CloseCemeteryPanel() => _hud.CloseCemeteryPanel();

        private void RebuildCemetery(CombatContext ctx)
        {
            bool inPlay = _combatManager.CurrentState is PlayState;
            _cemetery.Rebuild(ctx.PlayerCemetery, inPlay, ctx.PlayerMana);
        }

        private void OnPlayFromCemetery(CardData card)
        {
            _combatManager.PlayState?.TryPlayFromCemetery(_combatManager.Context, card);
            CloseCemeteryPanel();
        }

        // ── Boutons d'action ──────────────────────────────────────────────

        private void OnSacrificeClicked()
        {
            if (_selectedHandData == null)
            {
                GameLog.Warning("CombatBoardUI", "Aucune carte sélectionnée pour le sacrifice");
                return;
            }
            _combatManager.SacrificeState?.TrySacrifice(_combatManager.Context, _selectedHandData);
            _playerHand.ClearSelection();
        }

        private void OnEndPlayClicked()
        {
            _playerHand.ClearSelection();
            var state = _combatManager.CurrentState;
            if (state is SacrificeState ss)
                ss.Skip(_combatManager.Context);
            else
                _combatManager.PlayState?.EndPlay(_combatManager.Context);
        }

        private void OnBlockClicked()
        {
            if (_selectedHandData == null)
            {
                GameLog.Warning("CombatBoardUI", "Aucune carte Blocage sélectionnée");
                return;
            }
            _combatManager.ReactiveState?.TryBlock(_combatManager.Context, _selectedHandData);
            _playerHand.ClearSelection();
        }

        private void OnPassClicked()
            => _combatManager.ReactiveState?.Pass(_combatManager.Context);

        private void OnForgeClicked() => SceneManager.LoadScene("Forge");
    }
}
