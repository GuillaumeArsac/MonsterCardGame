using MonsterCardGame.Core.Services;

namespace MonsterCardGame.Gameplay.World
{
    public interface ISaveService : IService
    {
        void Save();
        void Load();
    }
}
