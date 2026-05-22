using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MonsterCardGame.Gameplay.Cards;

namespace MonsterCardGame.UI.Combat.Board
{
    /// <summary>Ligne uGUI du panneau cimetière : nom, badge/coût Rampant, clic pour rejouer.</summary>
    public class CemeteryRow : MonoBehaviour
    {
        [SerializeField] private TMP_Text   _nameLabel;
        [SerializeField] private GameObject _rampantBadge;
        [SerializeField] private TMP_Text   _costLabel;
        [SerializeField] private Button     _button;

        public event Action<CardData> Clicked;

        private CardData _data;

        private void Awake()
        {
            if (_button != null) _button.onClick.AddListener(() => Clicked?.Invoke(_data));
        }

        /// <param name="rampant">Carte Rampant rejouable depuis le cimetière (Allié + mot-clé).</param>
        /// <param name="canPlay">Rejouable maintenant (phase Action + mana suffisant).</param>
        public void Bind(CardData data, bool rampant, bool canPlay, int cost)
        {
            _data = data;

            if (_nameLabel != null) _nameLabel.text = data.CardName;

            if (_rampantBadge != null) _rampantBadge.SetActive(rampant);

            if (_costLabel != null)
            {
                _costLabel.gameObject.SetActive(rampant);
                if (rampant)
                {
                    _costLabel.text = $"{cost}m";
                    var c = _costLabel.color;
                    c.a = canPlay ? 1f : 0.45f;
                    _costLabel.color = c;
                }
            }

            if (_button != null) _button.interactable = canPlay;
        }
    }
}
