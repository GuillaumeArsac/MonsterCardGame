using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using MonsterCardGame.Gameplay.Cards;
using MonsterCardGame.Gameplay.Combat;

namespace MonsterCardGame.UI.Combat.Board
{
    /// <summary>
    /// Gère la main du joueur sous forme de prefabs 2D, disposée en éventail.
    /// Synchronise par diff avec <see cref="CombatContext.PlayerHand"/> (les cartes
    /// existantes glissent à leur nouvelle place, seules les nouvelles sont instanciées
    /// et seules celles défaussées sont détruites).
    /// </summary>
    public class PlayerHandController : MonoBehaviour
    {
        [Header("Références")]
        [SerializeField] private CombatManager  _combatManager;
        [SerializeField] private CardController _cardPrefab;
        [SerializeField] private Transform      _handAnchor;
        [SerializeField] private Camera         _camera;

        [Header("Layout en éventail")]
        [SerializeField, Tooltip("Rayon de l'arc de l'éventail (unités world). Plus grand = arc plus plat.")]
        private float _fanRadius = 8f;

        [SerializeField, Tooltip("Ouverture angulaire totale de l'éventail (degrés). 0 = ligne droite.")]
        private float _maxFanAngle = 35f;

        [SerializeField, Tooltip("Espacement angulaire max entre deux cartes (degrés). Limite le serrage quand peu de cartes.")]
        private float _maxAnglePerCard = 8f;

        [SerializeField, Tooltip("Échelle locale appliquée à chaque carte au repos")]
        private Vector3 _cardScale = Vector3.one;

        [SerializeField, Tooltip("Position de départ des cartes piochées (offset local par rapport à l'anchor)")]
        private Vector3 _drawFromOffset = new(-6f, 2f, 0f);

        // ── Événements ────────────────────────────────────────────────────

        /// <summary>Sélection d'une carte (clic gauche). null = désélection.</summary>
        public event Action<CardData> SelectionChanged;

        /// <summary>Le joueur active une carte (clic gauche sur la carte déjà sélectionnée).</summary>
        public event Action<CardData> CardActivated;

        /// <summary>Clic droit sur une carte — demande l'ouverture de la popup détail.</summary>
        public event Action<CardData> CardRightClicked;

        // ── État interne ──────────────────────────────────────────────────

        private readonly List<CardController> _cards = new();
        private CardController _selected;
        private CardController _hovered;
        private int _lastHandHash = -1;

        // ── Cycle de vie ──────────────────────────────────────────────────

        private void Awake()
        {
            if (_camera == null) _camera = Camera.main;
        }

        private void Update()
        {
            var ctx = _combatManager?.Context;
            if (ctx == null) return;

            SyncHand(ctx);
            UpdateHover();
            HandleInput();
        }

        // ── Synchronisation avec CombatContext (diff) ─────────────────────

        private void SyncHand(CombatContext ctx)
        {
            int hash = ComputeHandHash(ctx.PlayerHand);
            if (hash == _lastHandHash) return;
            _lastHandHash = hash;

            // Pool des cartes existantes par CardData (queue pour gérer les doublons).
            var pool = new Dictionary<CardData, Queue<CardController>>();
            foreach (var c in _cards)
            {
                if (c == null) continue;
                if (!pool.TryGetValue(c.Data, out var q))
                    pool[c.Data] = q = new Queue<CardController>();
                q.Enqueue(c);
            }

            var ordered = new List<CardController>(ctx.PlayerHand.Count);
            var freshlyDealt = new List<CardController>();

            // Apparie la main demandée avec les cartes existantes ; instancie ce qui manque.
            for (int i = 0; i < ctx.PlayerHand.Count; i++)
            {
                var data = ctx.PlayerHand[i];
                if (pool.TryGetValue(data, out var q) && q.Count > 0)
                {
                    ordered.Add(q.Dequeue());
                }
                else
                {
                    var card = Instantiate(_cardPrefab, _handAnchor);
                    card.Bind(data);
                    ordered.Add(card);
                    freshlyDealt.Add(card);
                }
            }

            // Tout ce qui reste dans le pool a été retiré de la main — défausse + destroy.
            foreach (var q in pool.Values)
            {
                while (q.Count > 0)
                {
                    var c = q.Dequeue();
                    if (_selected == c)
                    {
                        _selected = null;
                        SelectionChanged?.Invoke(null);
                    }
                    if (_hovered == c) _hovered = null;

                    var captured = c;
                    var tween = captured.PlayDiscard();
                    tween.OnComplete(() => { if (captured != null) Destroy(captured.gameObject); });
                }
            }

            _cards.Clear();
            _cards.AddRange(ordered);

            LayoutCards(freshlyDealt);
        }

        private static int ComputeHandHash(IReadOnlyList<CardData> hand)
        {
            unchecked
            {
                int h = 17;
                for (int i = 0; i < hand.Count; i++)
                    h = h * 31 + (hand[i]?.GetInstanceID() ?? 0);
                return h;
            }
        }

        // ── Layout en éventail ────────────────────────────────────────────

        private void LayoutCards(List<CardController> freshlyDealt)
        {
            int count = _cards.Count;
            if (count == 0) return;

            // Angle total occupé, plafonné pour ne pas trop écarter quand on a peu de cartes.
            float totalAngle = count == 1
                ? 0f
                : Mathf.Min(_maxFanAngle, _maxAnglePerCard * (count - 1));

            float startAngle = totalAngle * 0.5f; // carte de gauche penche vers la gauche (Z positif)
            float angleStep  = count == 1 ? 0f : totalAngle / (count - 1);

            for (int i = 0; i < count; i++)
            {
                float angleDeg = startAngle - angleStep * i;
                float rad      = angleDeg * Mathf.Deg2Rad;

                // Centre de l'arc en (0, -R) → carte centrale à l'origine.
                Vector3 target = new(
                    _fanRadius * Mathf.Sin(rad),
                    _fanRadius * (Mathf.Cos(rad) - 1f),
                    i * 0.01f); // petit z pour stabiliser le raycast quand les cartes se chevauchent

                var card = _cards[i];
                if (freshlyDealt.Contains(card))
                    card.PlayDeal(_drawFromOffset, target, _cardScale, angleDeg, delay: i * 0.05f);
                else
                    card.SetBasePose(target, _cardScale, angleDeg, snap: false);
            }
        }

        // ── Hover (survol souris) ────────────────────────────────────────

        private void UpdateHover()
        {
            if (_camera == null) return;

            if (Mouse.current == null) return;
            Vector3 world = _camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            var hit = Physics2D.Raycast(world, Vector2.zero);

            CardController under = null;
            if (hit.collider != null)
            {
                var c = hit.collider.GetComponentInParent<CardController>();
                if (c != null && _cards.Contains(c)) under = c;
            }

            if (under == _hovered) return;

            if (_hovered != null) _hovered.SetHovered(false);
            _hovered = under;
            if (_hovered != null) _hovered.SetHovered(true);
        }

        // ── Input ─────────────────────────────────────────────────────────

        private void HandleInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;
            if (mouse.leftButton.wasPressedThisFrame) HandleClick(button: 0);
            if (mouse.rightButton.wasPressedThisFrame) HandleClick(button: 1);
        }

        private void HandleClick(int button)
        {
            // Clic sur la carte actuellement survolée (déjà résolue par UpdateHover).
            if (_hovered == null) return;

            if (button == 1)
            {
                CardRightClicked?.Invoke(_hovered.Data);
                return;
            }

            if (_selected == _hovered)
                CardActivated?.Invoke(_hovered.Data);
            else
                SetSelected(_hovered);
        }

        // ── Sélection (API publique pour CombatBoardUI) ───────────────────

        public CardData SelectedCard => _selected?.Data;

        public void ClearSelection()
        {
            if (_selected == null) return;
            _selected.SetSelected(false);
            _selected = null;
            SelectionChanged?.Invoke(null);
        }

        private void SetSelected(CardController card)
        {
            if (_selected != null) _selected.SetSelected(false);
            _selected = card;
            _selected.SetSelected(true);
            SelectionChanged?.Invoke(card.Data);
        }
    }
}
