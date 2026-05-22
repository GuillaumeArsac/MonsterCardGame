using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MonsterCardGame.UI.Combat.Board
{
    /// <summary>
    /// Façade UI 2D du combat : labels (HP/mana/tour/compteurs), boutons d'action,
    /// bannières (phase, ciblage, action en attente) et overlays (résultat, cimetière).
    /// Le binder (CombatBoardUI) lit/écrit l'état via cette API et s'abonne aux events.
    /// </summary>
    public class CombatHUD : MonoBehaviour
    {
        [Header("Labels — joueur")]
        [SerializeField] private TMP_Text _playerHPLabel;
        [SerializeField] private TMP_Text _playerManaLabel;
        [SerializeField] private TMP_Text _turnLabel;
        [SerializeField] private TMP_Text _playerHandCountLabel;

        [Header("Labels — monstre")]
        [SerializeField] private TMP_Text _monsterHPLabel;
        [SerializeField] private TMP_Text _monsterDeckCountLabel;

        [Header("Boutons d'action")]
        [SerializeField] private Button _sacrificeBtn;
        [SerializeField] private Button _endPlayBtn;
        [SerializeField] private Button _blockBtn;
        [SerializeField] private Button _passBtn;
        [SerializeField] private Button _cemeteryBtn;
        [SerializeField] private TMP_Text _cemeteryBtnLabel;

        [Header("Bannière de phase")]
        [SerializeField] private GameObject _phaseBanner;
        [SerializeField] private TMP_Text _activePlayerLabel;
        [SerializeField] private TMP_Text _phaseNameLabel;
        [SerializeField] private Image _phaseBannerBackground;
        [SerializeField] private Color _phaseColorPlayer  = new(0.20f, 0.40f, 0.70f, 0.85f);
        [SerializeField] private Color _phaseColorMonster = new(0.70f, 0.20f, 0.20f, 0.85f);

        [Header("Bannière de ciblage")]
        [SerializeField] private GameObject _targetingBanner;
        [SerializeField] private TMP_Text   _targetingLabel;
        [SerializeField] private Button     _cancelTargetBtn;

        [Header("Action monstre en attente")]
        [SerializeField] private GameObject _pendingActionBanner;
        [SerializeField] private TMP_Text   _pendingActionLabel;

        [Header("Overlay résultat")]
        [SerializeField] private GameObject _resultOverlay;
        [SerializeField] private TMP_Text   _resultLabel;
        [SerializeField] private Transform  _lootListRoot;
        [SerializeField] private Button     _forgeBtn;

        [Header("Panneau cimetière")]
        [SerializeField] private GameObject _cemeteryPanel;
        [SerializeField] private Button     _cemeteryCloseBtn;
        [SerializeField] private Transform  _cemeteryListRoot;

        [Header("Zone monstre (clic direct / ciblage)")]
        [SerializeField] private Button _monsterZoneButton;
        [SerializeField] private GameObject _monsterZoneHighlight;

        // ── Events ────────────────────────────────────────────────────────

        public event Action SacrificeClicked;
        public event Action EndPlayClicked;
        public event Action BlockClicked;
        public event Action PassClicked;
        public event Action CemeteryOpenClicked;
        public event Action CemeteryCloseClicked;
        public event Action CancelTargetingClicked;
        public event Action ForgeClicked;
        public event Action MonsterZoneClicked;

        // ── Exposés au binder pour remplir des listes ─────────────────────

        public Transform LootListRoot     => _lootListRoot;
        public Transform CemeteryListRoot => _cemeteryListRoot;
        public bool IsCemeteryPanelOpen   => _cemeteryPanel != null && _cemeteryPanel.activeSelf;

        // ── Lifecycle ─────────────────────────────────────────────────────

        private void Awake()
        {
            if (_sacrificeBtn     != null) _sacrificeBtn.onClick.AddListener(() => SacrificeClicked?.Invoke());
            if (_endPlayBtn       != null) _endPlayBtn.onClick.AddListener(() => EndPlayClicked?.Invoke());
            if (_blockBtn         != null) _blockBtn.onClick.AddListener(() => BlockClicked?.Invoke());
            if (_passBtn          != null) _passBtn.onClick.AddListener(() => PassClicked?.Invoke());
            if (_cemeteryBtn      != null) _cemeteryBtn.onClick.AddListener(() => CemeteryOpenClicked?.Invoke());
            if (_cemeteryCloseBtn != null) _cemeteryCloseBtn.onClick.AddListener(() => CemeteryCloseClicked?.Invoke());
            if (_cancelTargetBtn  != null) _cancelTargetBtn.onClick.AddListener(() => CancelTargetingClicked?.Invoke());
            if (_forgeBtn         != null) _forgeBtn.onClick.AddListener(() => ForgeClicked?.Invoke());
            if (_monsterZoneButton != null) _monsterZoneButton.onClick.AddListener(() => MonsterZoneClicked?.Invoke());
        }

        // ── Labels ────────────────────────────────────────────────────────

        public void SetMonsterHP(int hp)         { if (_monsterHPLabel        != null) _monsterHPLabel.text        = $"PV: {hp}"; }
        public void SetPlayerHP(int hp)          { if (_playerHPLabel         != null) _playerHPLabel.text         = $"PV: {hp}"; }
        public void SetPlayerMana(int mana)      { if (_playerManaLabel       != null) _playerManaLabel.text       = $"Mana: {mana}"; }
        public void SetTurn(int turn)            { if (_turnLabel             != null) _turnLabel.text             = $"Tour {turn}"; }
        public void SetMonsterDeckCount(int n)   { if (_monsterDeckCountLabel != null) _monsterDeckCountLabel.text = $"Deck: {n}"; }
        public void SetPlayerHandCount(int n)    { if (_playerHandCountLabel  != null) _playerHandCountLabel.text  = $"{n} carte(s)"; }
        public void SetCemeteryButtonText(int n) { if (_cemeteryBtnLabel      != null) _cemeteryBtnLabel.text      = $"Cimetière ({n})"; }

        // ── Boutons (activation) ─────────────────────────────────────────

        public void SetSacrificeEnabled(bool on) { if (_sacrificeBtn != null) _sacrificeBtn.interactable = on; }
        public void SetEndPlayEnabled(bool on)   { if (_endPlayBtn   != null) _endPlayBtn.interactable   = on; }
        public void SetBlockEnabled(bool on)     { if (_blockBtn     != null) _blockBtn.interactable     = on; }
        public void SetPassEnabled(bool on)      { if (_passBtn      != null) _passBtn.interactable      = on; }

        // ── Bannière de phase ─────────────────────────────────────────────

        public void SetPhase(string activePlayer, string phaseName, bool isPlayerTurn)
        {
            if (_phaseBanner != null) _phaseBanner.SetActive(true);
            if (_activePlayerLabel != null) _activePlayerLabel.text = activePlayer;
            if (_phaseNameLabel    != null) _phaseNameLabel.text    = phaseName;
            if (_phaseBannerBackground != null)
                _phaseBannerBackground.color = isPlayerTurn ? _phaseColorPlayer : _phaseColorMonster;
        }

        // ── Bannière de ciblage ──────────────────────────────────────────

        public void ShowTargetingBanner(string text)
        {
            if (_targetingBanner != null) _targetingBanner.SetActive(true);
            if (_targetingLabel  != null) _targetingLabel.text = text;
        }

        public void HideTargetingBanner()
        {
            if (_targetingBanner != null) _targetingBanner.SetActive(false);
        }

        // ── Bannière action en attente ────────────────────────────────────

        public void ShowPendingActionBanner(string text)
        {
            if (_pendingActionBanner != null) _pendingActionBanner.SetActive(true);
            if (_pendingActionLabel  != null) _pendingActionLabel.text = text;
        }

        public void HidePendingActionBanner()
        {
            if (_pendingActionBanner != null) _pendingActionBanner.SetActive(false);
        }

        // ── Overlay résultat ──────────────────────────────────────────────

        public void ShowResultOverlay(string text)
        {
            if (_resultOverlay != null) _resultOverlay.SetActive(true);
            if (_resultLabel   != null) _resultLabel.text = text;
        }

        public void HideResultOverlay()
        {
            if (_resultOverlay != null) _resultOverlay.SetActive(false);
        }

        // ── Cimetière ─────────────────────────────────────────────────────

        public void OpenCemeteryPanel()  { if (_cemeteryPanel != null) _cemeteryPanel.SetActive(true);  }
        public void CloseCemeteryPanel() { if (_cemeteryPanel != null) _cemeteryPanel.SetActive(false); }

        // ── Zone monstre ──────────────────────────────────────────────────

        public void SetMonsterZoneHighlight(bool on)
        {
            if (_monsterZoneHighlight != null) _monsterZoneHighlight.SetActive(on);
        }
    }
}
