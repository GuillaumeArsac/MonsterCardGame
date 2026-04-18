using UnityEngine;
using MonsterCardGame.Core.Services;
using MonsterCardGame.Gameplay.Combat.Keywords;

namespace MonsterCardGame.Gameplay.Combat
{
    /// <summary>
    /// Enregistre les services Gameplay au démarrage de l'application via [RuntimeInitializeOnLoadMethod].
    /// S'exécute avant tout Awake() — aucun GameObject requis dans la scène.
    /// Séparé de BootLoader (Core.asmdef) pour respecter la dépendance Core ← Gameplay.
    /// </summary>
    internal static class CombatServicesInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterGameplayServices()
        {
            Services.Register<IKeywordResolver>(new KeywordResolver());
            Core.GameLog.Info("CombatServicesInstaller", "KeywordResolver enregistré");
        }
    }
}
