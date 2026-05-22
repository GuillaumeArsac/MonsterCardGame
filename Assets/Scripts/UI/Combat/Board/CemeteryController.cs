using System;
using System.Collections.Generic;
using UnityEngine;
using MonsterCardGame.Gameplay.Cards;

namespace MonsterCardGame.UI.Combat.Board
{
    /// <summary>
    /// Construit la liste du panneau cimetière (lignes uGUI sous un conteneur).
    /// Aucune règle de jeu : publie <see cref="PlayFromCemeteryRequested"/> quand le
    /// joueur clique une carte Rampant rejouable ; le binder exécute l'action.
    /// </summary>
    public class CemeteryController : MonoBehaviour
    {
        [SerializeField] private Transform   _listRoot;
        [SerializeField] private CemeteryRow _rowPrefab;
        [SerializeField, Tooltip("Affiché quand le cimetière est vide")]
        private GameObject _emptyLabel;

        public event Action<CardData> PlayFromCemeteryRequested;

        private readonly List<CemeteryRow> _rows = new();

        public void Rebuild(IReadOnlyList<CardData> cemetery, bool inPlay, int playerMana)
        {
            foreach (var row in _rows)
                if (row != null) Destroy(row.gameObject);
            _rows.Clear();

            bool empty = cemetery.Count == 0;
            if (_emptyLabel != null) _emptyLabel.SetActive(empty);
            if (empty) return;

            foreach (var card in cemetery)
            {
                bool rampant = card.HasKeyword(Keyword.Rampant) && card.CardType == CardType.Allie;
                bool canPlay = rampant && inPlay && playerMana >= card.ManaCost;

                var row = Instantiate(_rowPrefab, _listRoot);
                row.Bind(card, rampant, canPlay, card.ManaCost);
                row.Clicked += OnRowClicked;
                _rows.Add(row);
            }
        }

        private void OnRowClicked(CardData card) => PlayFromCemeteryRequested?.Invoke(card);
    }
}
