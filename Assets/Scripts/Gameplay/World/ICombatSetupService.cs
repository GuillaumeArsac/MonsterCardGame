using MonsterCardGame.Core.Services;
using MonsterCardGame.Gameplay.Combat.Data;

namespace MonsterCardGame.Gameplay.World
{
    public interface ICombatSetupService : IService
    {
        void Setup(ZoneData zone, MonsterData monster);
        ZoneData    PendingZone    { get; }
        MonsterData PendingMonster { get; }
    }
}
