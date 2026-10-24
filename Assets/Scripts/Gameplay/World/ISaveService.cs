using MonsterCardGame.Core.Services;

namespace MonsterCardGame.Gameplay.World
{
    public interface ISaveService : IService
    {
        void Save();
        void Load();

        /// <summary>Nouvelle partie : oublie les combats gagnés et rend la collection de départ, puis sauvegarde.</summary>
        void ResetGame();
    }
}
