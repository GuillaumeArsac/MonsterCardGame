using System.Collections.Generic;
using MonsterCardGame.Core.Services;
using MonsterCardGame.Gameplay.Inventory;

namespace MonsterCardGame.Gameplay.Forge
{
    public interface IForgeService : IService
    {
        IReadOnlyList<CraftRecipe> AllRecipes       { get; }
        IReadOnlyList<CraftRecipe> DiscoveredRecipes { get; }

        void InitializeCatalog(ForgeData data);

        bool IsDiscovered(CraftRecipe recipe);

        /// <summary>Révèle toute recette cachée dont le joueur possède déjà tous les ingrédients.</summary>
        void CheckEmpiricalDiscovery(IPlayerInventory inventory);

        /// <summary>Tente de crafter la recette. Retourne true si réussi (matériaux consommés, carte ajoutée).</summary>
        bool TryCraft(CraftRecipe recipe, IPlayerInventory inventory);
    }
}
