using DG.Tweening;
using UnityEngine;
using MonsterCardGame.Gameplay.Cards;

namespace MonsterCardGame.UI.Combat.Board
{
    /// <summary>
    /// Contrôleur d'une carte de la main (prefab 2D).
    /// Délègue l'affichage à <see cref="CardView2D"/> et gère les poses/animations
    /// spécifiques à la main (deal, hover, sélection, défausse).
    /// L'input est géré par <see cref="PlayerHandController"/> via raycast.
    /// </summary>
    [RequireComponent(typeof(CardView2D))]
    public class CardController : MonoBehaviour
    {
        private enum PoseState { Idle, Hovered, Selected }

        [SerializeField] private CardView2D _view;

        [Header("Animations")]
        [SerializeField] private float _hoveredYOffset   = 0.35f;
        [SerializeField] private float _hoveredScale    = 1.08f;
        [SerializeField] private float _selectedYOffset = 0.6f;
        [SerializeField] private float _selectedScale   = 1.15f;
        [SerializeField] private float _poseAnimTime    = 0.18f;
        [SerializeField] private float _dealAnimTime    = 0.30f;
        [SerializeField] private float _discardAnimTime = 0.25f;

        public CardData Data => _view != null ? _view.Data : null;
        public bool IsSelected => _state == PoseState.Selected;

        private Vector3   _basePosition;
        private Vector3   _baseScale = Vector3.one;
        private float     _baseZRotation;
        private PoseState _state = PoseState.Idle;

        private void Reset() => _view = GetComponent<CardView2D>();
        private void Awake()
        {
            if (_view == null) _view = GetComponent<CardView2D>();
        }

        // ── Binding ───────────────────────────────────────────────────────

        public void Bind(CardData data)
        {
            _view.Bind(data);
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
                _view.SetHighlight(false);
                transform.localPosition    = localPosition;
                transform.localScale       = localScale;
                transform.localEulerAngles = new Vector3(0f, 0f, zRotation);
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
            _view.SetHighlight(false);

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

            _view.SetHighlight(selected);
            ApplyPose();
        }

        public Tween PlayDiscard()
        {
            transform.DOKill();
            _state = PoseState.Idle;
            _view.SetHighlight(false);

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

        private void OnDestroy() => transform.DOKill();
    }
}
