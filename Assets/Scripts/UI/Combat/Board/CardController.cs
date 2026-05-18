using DG.Tweening;
using TMPro;
using UnityEngine;
using MonsterCardGame.Gameplay.Cards;

namespace MonsterCardGame.UI.Combat.Board
{
    /// <summary>
    /// Contrôleur d'une carte du board (prefab 2D).
    /// Gère l'affichage des données et les animations (deal, hover, sélection, défausse).
    /// L'input est géré par <see cref="PlayerHandController"/> via raycast.
    /// </summary>
    public class CardController : MonoBehaviour
    {
        private enum PoseState { Idle, Hovered, Selected }

        [Header("Visuel")]
        [SerializeField] private SpriteRenderer _frameRenderer;

        [SerializeField] private SpriteRenderer _artworkRenderer;
        [SerializeField] private TMP_Text _nameLabel;
        [SerializeField] private TMP_Text _costLabel;
        [SerializeField] private TMP_Text _manaGenLabel;
        [SerializeField] private GameObject _selectionHighlight;

        [Header("Couleurs par région")]
        [SerializeField] private Color _colorPlaines = new(0.55f, 0.75f, 0.45f);

        [SerializeField] private Color _colorMontagne = new(0.65f, 0.55f, 0.45f);
        [SerializeField] private Color _colorMarais = new(0.45f, 0.55f, 0.40f);
        [SerializeField] private Color _colorRuines = new(0.60f, 0.55f, 0.50f);
        [SerializeField] private Color _colorForet = new(0.35f, 0.60f, 0.40f);
        [SerializeField] private Color _colorOcean = new(0.40f, 0.55f, 0.70f);
        [SerializeField] private Color _colorAucune = new(0.55f, 0.55f, 0.55f);

        [Header("Animations")]
        [SerializeField] private float _hoveredYOffset = 0.35f;
        [SerializeField] private float _hoveredScale = 1.08f;
        [SerializeField] private float _selectedYOffset = 0.6f;

        [SerializeField] private float _selectedScale = 1.15f;
        [SerializeField] private float _poseAnimTime = 0.18f;
        [SerializeField] private float _dealAnimTime = 0.30f;
        [SerializeField] private float _discardAnimTime = 0.25f;

        public CardData Data { get; private set; }
        public bool IsSelected => _state == PoseState.Selected;

        private Vector3   _basePosition;
        private Vector3   _baseScale = Vector3.one;
        private float     _baseZRotation;
        private PoseState _state = PoseState.Idle;

        // ── Binding ───────────────────────────────────────────────────────

        public void Bind(CardData data)
        {
            Data = data;

            if (_nameLabel != null) _nameLabel.text = data.CardName;
            if (_costLabel != null) _costLabel.text = data.ManaCost.ToString();

            if (_manaGenLabel != null)
            {
                bool hasMana = data.ManaGenerated > 0;
                _manaGenLabel.gameObject.SetActive(hasMana);
                if (hasMana) _manaGenLabel.text = $"+{data.ManaGenerated}";
            }

            if (_artworkRenderer != null) _artworkRenderer.sprite = data.Artwork;
            if (_frameRenderer != null) _frameRenderer.color = ColorForRegion(data.Region);

            if (_selectionHighlight != null) _selectionHighlight.SetActive(false);
            _state = PoseState.Idle;
        }

        // ── Layout : pose de repos ────────────────────────────────────────

        public void SetBasePose(Vector3 localPosition, Vector3 localScale, float zRotation, bool snap)
        {
            _basePosition  = localPosition;
            _baseScale     = localScale;
            _baseZRotation = zRotation;

            if (snap)
            {
                transform.DOKill();
                _state = PoseState.Idle;
                if (_selectionHighlight != null) _selectionHighlight.SetActive(false);
                transform.localPosition       = localPosition;
                transform.localScale          = localScale;
                transform.localEulerAngles    = new Vector3(0f, 0f, zRotation);
            }
            else
            {
                ApplyPose();
            }
        }

        // ── Animations ────────────────────────────────────────────────────

        public void PlayDeal(Vector3 fromLocalPosition, Vector3 toLocalPosition, Vector3 targetScale, float zRotation, float delay)
        {
            _basePosition  = toLocalPosition;
            _baseScale     = targetScale;
            _baseZRotation = zRotation;
            _state         = PoseState.Idle;
            if (_selectionHighlight != null) _selectionHighlight.SetActive(false);

            transform.DOKill();
            transform.localPosition    = fromLocalPosition;
            transform.localScale       = Vector3.zero;
            transform.localEulerAngles = Vector3.zero;

            var seq = DOTween.Sequence().SetDelay(delay);
            seq.Append(transform.DOLocalMove(toLocalPosition, _dealAnimTime).SetEase(Ease.OutCubic));
            seq.Join(transform.DOScale(targetScale, _dealAnimTime).SetEase(Ease.OutBack));
            seq.Join(transform.DOLocalRotate(new Vector3(0f, 0f, zRotation), _dealAnimTime).SetEase(Ease.OutCubic));
        }

        public void SetHovered(bool hovered)
        {
            // Le hover ne touche pas l'état "Selected" (la sélection prime).
            if (_state == PoseState.Selected) return;

            var target = hovered ? PoseState.Hovered : PoseState.Idle;
            if (_state == target) return;
            _state = target;
            ApplyPose();
        }

        public void SetSelected(bool selected)
        {
            var target = selected ? PoseState.Selected : PoseState.Idle;
            if (_state == target) return;
            _state = target;

            if (_selectionHighlight != null) _selectionHighlight.SetActive(selected);
            ApplyPose();
        }

        public Tween PlayDiscard()
        {
            transform.DOKill();
            _state = PoseState.Idle;
            if (_selectionHighlight != null) _selectionHighlight.SetActive(false);

            var seq = DOTween.Sequence();
            seq.Append(transform.DOScale(_baseScale * 0.1f, _discardAnimTime).SetEase(Ease.InCubic));
            seq.Join(transform.DOLocalMoveY(transform.localPosition.y - 1f, _discardAnimTime).SetEase(Ease.InCubic));
            return seq;
        }

        // ── Application de la pose courante ──────────────────────────────

        private void ApplyPose()
        {
            Vector3 pos    = _basePosition;
            Vector3 scale  = _baseScale;
            float   zAngle = _baseZRotation;

            switch (_state)
            {
                case PoseState.Hovered:
                    pos   += Vector3.up * _hoveredYOffset;
                    scale *= _hoveredScale;
                    zAngle = 0f;
                    break;
                case PoseState.Selected:
                    pos   += Vector3.up * _selectedYOffset;
                    scale *= _selectedScale;
                    zAngle = 0f;
                    break;
            }

            transform.DOKill();
            transform.DOLocalMove(pos, _poseAnimTime).SetEase(Ease.OutCubic);
            transform.DOScale(scale, _poseAnimTime).SetEase(Ease.OutCubic);
            transform.DOLocalRotate(new Vector3(0f, 0f, zAngle), _poseAnimTime).SetEase(Ease.OutCubic);
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private Color ColorForRegion(Region region) => region switch
        {
            Region.Plaines => _colorPlaines,
            Region.Montagne => _colorMontagne,
            Region.Marais => _colorMarais,
            Region.Ruines => _colorRuines,
            Region.Foret => _colorForet,
            Region.Ocean => _colorOcean,
            _ => _colorAucune
        };

        private void OnDestroy() => transform.DOKill();
    }
}
