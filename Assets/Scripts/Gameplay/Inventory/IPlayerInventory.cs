using System.Collections.Generic;
using MonsterCardGame.Core.Services;
using MonsterCardGame.Gameplay.Cards;

namespace MonsterCardGame.Gameplay.Inventory
{
    public interface IPlayerInventory : IService
    {
        IReadOnlyDictionary<MaterialData, int> Materials  { get; }
        IReadOnlyDictionary<CardData, int>     OwnedCards { get; }

        void AddMaterial(MaterialData material, int quantity = 1);

        /// <summary>Retire la quantité demandée. Retourne false si le stock est insuffisant (aucune modification).</summary>
        bool RemoveMaterial(MaterialData material, int quantity = 1);

        /// <summary>Vérifie que tous les coûts sont satisfaits sans modifier l'inventaire.</summary>
        bool HasMaterials(IReadOnlyList<MaterialCost> costs);

        void AddCard(CardData card);

        int GetCardCount(CardData card);

        // --- Deck actif ---
        IReadOnlyList<CardData> ActiveDeck { get; }

        void AddCardToDeck(CardData card);

        /// <summary>Retire une copie du deck actif. Retourne false si la carte n'y est pas.</summary>
        bool RemoveCardFromDeck(CardData card);

        int GetDeckCardCount(CardData card);

        /// <summary>Remet l'inventaire à zéro — utilisé par SaveService.Load() avant de repeupler.</summary>
        void Clear();
    }
}
