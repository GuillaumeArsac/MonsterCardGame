using System.Collections.Generic;
using UnityEngine;
using MonsterCardGame.Gameplay.Cards;

namespace MonsterCardGame.Gameplay.Deck
{
    [CreateAssetMenu(menuName = "MonsterCardGame/Deck/Deck Data")]
    public class DeckData : ScriptableObject
    {
        [Header("Cartes")]
        [SerializeField, Tooltip("Liste des cartes (doublons autorisés, représentent des copies)")]
        private List<CardData> _cards = new();

        public IReadOnlyList<CardData> Cards => _cards;

        public void AddCard(CardData card)
        {
            if (card != null) _cards.Add(card);
        }

        public void RemoveCard(CardData card)
            => _cards.Remove(card);

        public void Clear()
            => _cards.Clear();
    }
}
