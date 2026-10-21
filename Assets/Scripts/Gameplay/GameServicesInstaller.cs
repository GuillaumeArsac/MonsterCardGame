using UnityEngine;
using MonsterCardGame.Core.Services;
using MonsterCardGame.Gameplay.Content;
using MonsterCardGame.Gameplay.Encounters;
using MonsterCardGame.Gameplay.Monsters;
using MonsterCardGame.Gameplay.World;

namespace MonsterCardGame.Gameplay
{
    internal static class GameServicesInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterGameplayServices()
        {
            var worldData = Resources.Load<WorldData>("WorldData");
            var content   = Resources.Load<ContentDatabase>(ContentDatabase.ResourcePath);

            if (worldData == null)
                Core.GameLog.Warning("GameServicesInstaller", "WorldData introuvable dans Resources/ — créer Assets/Resources/WorldData.asset");
            if (content == null)
                Core.GameLog.Warning("GameServicesInstaller", "ContentDatabase introuvable dans Resources/ — lancer MonsterCardGame > Contenu > Générer les assets depuis les docs");

            var progression = new WorldProgressionService(worldData);
            var collection  = new MonsterCollection();
            var saveSvc     = new SaveService(progression, collection, content);

            Services.Register<IWorldProgressionService>(progression);
            Services.Register(collection);
            Services.Register(new CombatSession());
            Services.Register<ISaveService>(saveSvc);
            if (content != null) Services.Register(content);

            saveSvc.Load();

            Application.quitting += saveSvc.Save;

            Core.GameLog.Info("GameServicesInstaller", "Services Gameplay enregistrés");
        }
    }
}
