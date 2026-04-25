using System.Collections.Generic;
using UnityEngine;
using MonsterCardGame.Gameplay.Inventory;

namespace MonsterCardGame.Gameplay.Forge
{
    [CreateAssetMenu(fileName = "ForgeData", menuName = "MonsterCardGame/Forge/Forge Data")]
    public class ForgeData : ScriptableObject
    {
        [SerializeField, Tooltip("Toutes les recettes disponibles à ce niveau de Forge")]
        private List<CraftRecipe> _recipes = new();

        public IReadOnlyList<CraftRecipe> Recipes => _recipes;
    }
}
