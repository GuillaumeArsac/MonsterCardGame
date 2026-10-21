using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MonsterCardGame.Gameplay.Combat;

namespace MonsterCardGame.UI.Combat
{
    /// <summary>Un emplacement du terrain (gauche, centre, droite). Affiche la marque du Chasseur de trophées.</summary>
    public class SlotView : MonoBehaviour
    {
        [SerializeField] private Side      _side;
        [SerializeField] private int       _index;
        [SerializeField] private Button    _button;
        [SerializeField] private Image     _background;
        [SerializeField] private TMP_Text  _markLabel;
        [SerializeField] private RectTransform _cardAnchor;

        [Header("Couleurs")]
        [SerializeField] private Color _emptyColor     = new(1f, 1f, 1f, 0.06f);
        [SerializeField] private Color _placeableColor = new(1f, 0.82f, 0.25f, 0.35f);

        public Side          Side       => _side;
        public int           Index      => _index;
        public RectTransform CardAnchor => _cardAnchor;

        public event Action<SlotView> Clicked;

        private void Awake() => _button.onClick.AddListener(() => Clicked?.Invoke(this));

        /// <param name="bonusPoints">Points en plus si un monstre de ce camp est vaincu ici (emplacement marqué).</param>
        public void Refresh(bool placeable, int bonusPoints)
        {
            _background.color = placeable ? _placeableColor : _emptyColor;
            _markLabel.text   = bonusPoints > 0 ? $"Marqué : +{bonusPoints} point{(bonusPoints > 1 ? "s" : "")}" : "";
        }
    }
}
