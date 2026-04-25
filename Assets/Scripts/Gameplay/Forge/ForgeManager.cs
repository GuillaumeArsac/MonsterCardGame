using UnityEngine;
using MonsterCardGame.Core.Services;
using MonsterCardGame.Gameplay.Inventory;

namespace MonsterCardGame.Gameplay.Forge
{
    /// <summary>
    /// MonoBehaviour dans Forge.unity. Initialise le catalogue de recettes
    /// et déclenche la découverte empirique à l'ouverture de la Forge.
    /// </summary>
    public class ForgeManager : MonoBehaviour
    {
        [SerializeField, Tooltip("Catalogue de recettes disponibles à ce niveau de Forge")]
        private ForgeData _forgeData;

        private void Awake()
        {
            if (_forgeData == null)
            {
                Core.GameLog.Error("ForgeManager", "ForgeData non assignée dans l'Inspector");
                return;
            }

            var forge     = Services.Get<IForgeService>();
            var inventory = Services.Get<IPlayerInventory>();

            forge.InitializeCatalog(_forgeData);
            forge.CheckEmpiricalDiscovery(inventory);
        }
    }
}
