using UnityEngine;
using UnityEngine.SceneManagement;

namespace MonsterCardGame.Core.Services
{
    public class BootLoader : MonoBehaviour
    {
        [Header("Boot Configuration")]
        [SerializeField, Tooltip("Nom de la scène à charger après l'initialisation")]
        private string _nextScene = "MainMenu";

        private void Awake()
        {
            RegisterServices();
        }

        private void Start()
        {
            GameLog.Info("BootLoader", $"Boot terminé — chargement de {_nextScene}");
            SceneManager.LoadScene(_nextScene);
        }

        private void RegisterServices()
        {
            // Les services Gameplay sont enregistrés par GameServicesInstaller (Gameplay.asmdef)
            // via [RuntimeInitializeOnLoadMethod] — avant tout Awake().
            GameLog.Info("BootLoader", "Services Core initialisés");
        }
    }
}
