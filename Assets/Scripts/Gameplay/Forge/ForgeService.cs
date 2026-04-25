using System.Collections.Generic;
using MonsterCardGame.Gameplay.Inventory;

namespace MonsterCardGame.Gameplay.Forge
{
    public class ForgeService : IForgeService
    {
        private readonly List<CraftRecipe>  _allRecipes       = new();
        private readonly HashSet<CraftRecipe> _discovered     = new();
        private readonly List<CraftRecipe>  _discoveredList   = new();

        public IReadOnlyList<CraftRecipe> AllRecipes        => _allRecipes;
        public IReadOnlyList<CraftRecipe> DiscoveredRecipes => _discoveredList;

        public void InitializeCatalog(ForgeData data)
        {
            _allRecipes.Clear();
            _allRecipes.AddRange(data.Recipes);

            foreach (var recipe in _allRecipes)
            {
                if (recipe != null && recipe.IsDiscoveredByDefault)
                    Discover(recipe);
            }

            Core.GameLog.Info("ForgeService",
                $"Catalogue chargé : {_allRecipes.Count} recette(s), {_discoveredList.Count} découverte(s)");
        }

        public bool IsDiscovered(CraftRecipe recipe) => _discovered.Contains(recipe);

        public void CheckEmpiricalDiscovery(IPlayerInventory inventory)
        {
            foreach (var recipe in _allRecipes)
            {
                if (recipe == null || _discovered.Contains(recipe)) continue;
                if (inventory.HasMaterials(recipe.Ingredients))
                {
                    Discover(recipe);
                    Core.GameLog.Info("ForgeService",
                        $"Recette découverte empiriquement : {recipe.Result?.CardName ?? "???"}");
                }
            }
        }

        public bool TryCraft(CraftRecipe recipe, IPlayerInventory inventory)
        {
            if (!_discovered.Contains(recipe))
            {
                Core.GameLog.Warning("ForgeService", "Craft refusé : recette non découverte");
                return false;
            }
            if (!inventory.HasMaterials(recipe.Ingredients))
            {
                Core.GameLog.Warning("ForgeService",
                    $"Craft refusé : matériaux insuffisants pour {recipe.Result?.CardName}");
                return false;
            }

            foreach (var cost in recipe.Ingredients)
                inventory.RemoveMaterial(cost.Material, cost.Quantity);

            inventory.AddCard(recipe.Result);
            Core.GameLog.Info("ForgeService", $"Craft réussi : {recipe.Result?.CardName}");
            return true;
        }

        private void Discover(CraftRecipe recipe)
        {
            if (_discovered.Add(recipe))
                _discoveredList.Add(recipe);
        }
    }
}
