using System.Collections.Generic;
using UnityEngine;
using MonsterCardGame.Gameplay.Cards;

namespace MonsterCardGame.Gameplay.Inventory
{
    [CreateAssetMenu(fileName = "CraftRecipe", menuName = "MonsterCardGame/Inventory/Craft Recipe")]
    public class CraftRecipe : ScriptableObject
    {
        [Header("Ingrédients")]
        [SerializeField, Tooltip("Liste des matériaux requis et leurs quantités")]
        private List<MaterialCost> _ingredients = new();

        [Header("Résultat")]
        [SerializeField, Tooltip("Carte obtenue après craft")]
        private CardData _result;

        [Header("Découverte")]
        [SerializeField, Tooltip("True = visible dès le départ. False = cachée jusqu'à découverte empirique ou PNJ.")]
        private bool _isDiscoveredByDefault = false;

        public IReadOnlyList<MaterialCost> Ingredients          => _ingredients;
        public CardData                    Result               => _result;
        public bool                        IsDiscoveredByDefault => _isDiscoveredByDefault;
    }
}
