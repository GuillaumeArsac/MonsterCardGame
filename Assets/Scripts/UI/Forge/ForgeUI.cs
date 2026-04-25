using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using MonsterCardGame.Core.Services;
using MonsterCardGame.Gameplay.Forge;
using MonsterCardGame.Gameplay.Inventory;

namespace MonsterCardGame.UI.Forge
{
    [RequireComponent(typeof(UIDocument))]
    public class ForgeUI : MonoBehaviour
    {
        private IForgeService    _forge;
        private IPlayerInventory _inventory;

        private VisualElement _recipeList;
        private Label         _detailName;
        private Label         _detailType;
        private VisualElement _ingredientsList;
        private Button        _craftBtn;

        private CraftRecipe   _selectedRecipe;

        private void OnEnable()
        {
            _forge     = Services.Get<IForgeService>();
            _inventory = Services.Get<IPlayerInventory>();

            var root = GetComponent<UIDocument>().rootVisualElement;

            _recipeList      = root.Q<VisualElement>("recipe-list");
            _detailName      = root.Q<Label>("detail-name");
            _detailType      = root.Q<Label>("detail-type");
            _ingredientsList = root.Q<VisualElement>("ingredients-list");
            _craftBtn        = root.Q<Button>("craft-btn");

            root.Q<Button>("back-btn").clicked        += () => SceneManager.LoadScene("Combat");
            root.Q<Button>("deckbuilder-btn").clicked += () => SceneManager.LoadScene("DeckBuilder");
            _craftBtn.clicked += OnCraftClicked;
        }

        private void Start()
        {
            BuildRecipeList();
            ClearDetail();
        }

        private void BuildRecipeList()
        {
            _recipeList.Clear();

            foreach (var recipe in _forge.AllRecipes)
            {
                bool known = _forge.IsDiscovered(recipe);

                var entry = new Button();
                entry.AddToClassList("recipe-entry");

                if (known)
                {
                    entry.text = !string.IsNullOrEmpty(recipe.RecipeName) ? recipe.RecipeName : "Sans nom";
                    bool canCraft = _inventory.HasMaterials(recipe.Ingredients);
                    if (canCraft) entry.AddToClassList("recipe-entry--craftable");
                }
                else
                {
                    entry.text = "???";
                    entry.AddToClassList("recipe-entry--hidden");
                }

                var captured = recipe;
                entry.clicked += () => SelectRecipe(captured);
                _recipeList.Add(entry);
            }
        }

        private void SelectRecipe(CraftRecipe recipe)
        {
            _selectedRecipe = recipe;
            RefreshDetail();
        }

        private void RefreshDetail()
        {
            if (_selectedRecipe == null) { ClearDetail(); return; }

            bool known = _forge.IsDiscovered(_selectedRecipe);

            if (!known)
            {
                _detailName.text = "Recette inconnue";
                _detailType.text = "Combine les ingrédients pour découvrir cette recette.";
                _ingredientsList.Clear();
                _craftBtn.SetEnabled(false);
                return;
            }

            var result = _selectedRecipe.Result;
            _detailName.text = result != null ? result.CardName : "???";
            _detailType.text = result != null ? $"{result.CardType}  —  {result.Rarity}" : "";

            _ingredientsList.Clear();
            foreach (var cost in _selectedRecipe.Ingredients)
            {
                _inventory.Materials.TryGetValue(cost.Material, out int owned);
                bool enough = owned >= cost.Quantity;

                var row = new VisualElement();
                row.AddToClassList("ingredient-row");

                var nameLabel = new Label($"{cost.Material?.MaterialName ?? "???"} × {cost.Quantity}");
                nameLabel.AddToClassList("ingredient-name");

                var stockLabel = new Label($"({owned} possédé{(owned > 1 ? "s" : "")})");
                stockLabel.AddToClassList("ingredient-stock");
                stockLabel.AddToClassList(enough ? "ingredient-stock--ok" : "ingredient-stock--missing");

                row.Add(nameLabel);
                row.Add(stockLabel);
                _ingredientsList.Add(row);
            }

            _craftBtn.SetEnabled(_inventory.HasMaterials(_selectedRecipe.Ingredients));
        }

        private void ClearDetail()
        {
            _detailName.text = "Sélectionne une recette";
            _detailType.text = "";
            _ingredientsList.Clear();
            _craftBtn.SetEnabled(false);
        }

        private void OnCraftClicked()
        {
            if (_selectedRecipe == null) return;

            if (_forge.TryCraft(_selectedRecipe, _inventory))
            {
                _forge.CheckEmpiricalDiscovery(_inventory);
                BuildRecipeList();
                RefreshDetail();
            }
        }
    }
}
