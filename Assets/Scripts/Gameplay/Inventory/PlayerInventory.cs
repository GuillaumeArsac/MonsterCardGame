using System.Collections.Generic;
using MonsterCardGame.Gameplay.Cards;

namespace MonsterCardGame.Gameplay.Inventory
{
    public class PlayerInventory : IPlayerInventory
    {
        private readonly Dictionary<MaterialData, int> _materials  = new();
        private readonly Dictionary<CardData, int>     _ownedCards = new();
        private readonly List<CardData>                _activeDeck = new();

        public IReadOnlyDictionary<MaterialData, int> Materials  => _materials;
        public IReadOnlyDictionary<CardData, int>     OwnedCards => _ownedCards;
        public IReadOnlyList<CardData>                ActiveDeck => _activeDeck;

        public void AddMaterial(MaterialData material, int quantity = 1)
        {
            if (material == null) return;
            _materials.TryGetValue(material, out int current);
            _materials[material] = current + quantity;
            Core.GameLog.Info("PlayerInventory", $"+{quantity} {material.MaterialName} (total : {_materials[material]})");
        }

        public bool RemoveMaterial(MaterialData material, int quantity = 1)
        {
            if (material == null) return false;
            _materials.TryGetValue(material, out int current);
            if (current < quantity)
            {
                Core.GameLog.Warning("PlayerInventory",
                    $"Stock insuffisant : {material.MaterialName} (possédé {current}, requis {quantity})");
                return false;
            }
            int remaining = current - quantity;
            if (remaining == 0)
                _materials.Remove(material);
            else
                _materials[material] = remaining;
            Core.GameLog.Info("PlayerInventory", $"-{quantity} {material.MaterialName} (restant : {remaining})");
            return true;
        }

        public bool HasMaterials(IReadOnlyList<MaterialCost> costs)
        {
            foreach (var cost in costs)
            {
                _materials.TryGetValue(cost.Material, out int owned);
                if (owned < cost.Quantity) return false;
            }
            return true;
        }

        public void AddCard(CardData card)
        {
            if (card == null) return;
            _ownedCards.TryGetValue(card, out int current);
            _ownedCards[card] = current + 1;
            Core.GameLog.Info("PlayerInventory", $"Carte ajoutée : {card.CardName} (total : {_ownedCards[card]})");
        }

        public int GetCardCount(CardData card)
        {
            _ownedCards.TryGetValue(card, out int count);
            return count;
        }

        public void AddCardToDeck(CardData card)
        {
            if (card == null) return;
            _activeDeck.Add(card);
        }

        public bool RemoveCardFromDeck(CardData card)
        {
            if (card == null) return false;
            return _activeDeck.Remove(card);
        }

        public int GetDeckCardCount(CardData card)
        {
            int count = 0;
            foreach (var c in _activeDeck)
                if (c == card) count++;
            return count;
        }

        public void Clear()
        {
            _materials.Clear();
            _ownedCards.Clear();
            _activeDeck.Clear();
        }
    }
}
