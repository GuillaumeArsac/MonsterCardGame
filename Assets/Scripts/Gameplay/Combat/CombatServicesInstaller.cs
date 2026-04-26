using UnityEngine;
using MonsterCardGame.Core.Services;
using MonsterCardGame.Gameplay.Combat.Keywords;
using MonsterCardGame.Gameplay.Forge;
using MonsterCardGame.Gameplay.Inventory;
using MonsterCardGame.Gameplay.World;

namespace MonsterCardGame.Gameplay.Combat
{
    internal static class CombatServicesInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterGameplayServices()
        {
            var worldData = Resources.Load<WorldData>("WorldData");
            var registry  = Resources.Load<GameRegistry>("GameRegistry");

            if (worldData == null)
                Core.GameLog.Warning("CombatServicesInstaller", "WorldData introuvable dans Resources/ — créer Assets/Resources/WorldData.asset");
            if (registry == null)
                Core.GameLog.Warning("CombatServicesInstaller", "GameRegistry introuvable dans Resources/ — créer Assets/Resources/GameRegistry.asset");

            var inventory   = new PlayerInventory();
            var progression = new WorldProgressionService(worldData);
            var setupSvc    = new CombatSetupService();
            var saveSvc     = new SaveService(registry, inventory, progression);

            Services.Register<IKeywordResolver>(new KeywordResolver());
            Services.Register<IPlayerInventory>(inventory);
            Services.Register<IForgeService>(new ForgeService());
            Services.Register<IWorldProgressionService>(progression);
            Services.Register<ICombatSetupService>(setupSvc);
            Services.Register<ISaveService>(saveSvc);

            saveSvc.Load();

            Application.quitting += saveSvc.Save;

            Core.GameLog.Info("CombatServicesInstaller", "Services Gameplay enregistrés");
        }
    }
}
