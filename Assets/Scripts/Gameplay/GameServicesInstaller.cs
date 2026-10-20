using UnityEngine;
using MonsterCardGame.Core.Services;
using MonsterCardGame.Gameplay.World;

namespace MonsterCardGame.Gameplay
{
    internal static class GameServicesInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterGameplayServices()
        {
            var worldData = Resources.Load<WorldData>("WorldData");

            if (worldData == null)
                Core.GameLog.Warning("GameServicesInstaller", "WorldData introuvable dans Resources/ — créer Assets/Resources/WorldData.asset");

            var progression = new WorldProgressionService(worldData);
            var saveSvc     = new SaveService(progression);

            Services.Register<IWorldProgressionService>(progression);
            Services.Register<ISaveService>(saveSvc);

            saveSvc.Load();

            Application.quitting += saveSvc.Save;

            Core.GameLog.Info("GameServicesInstaller", "Services Gameplay enregistrés");
        }
    }
}
