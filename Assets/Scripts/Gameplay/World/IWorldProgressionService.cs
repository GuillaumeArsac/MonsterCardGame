using System.Collections.Generic;
using MonsterCardGame.Core.Services;
using MonsterCardGame.Gameplay.Combat.Data;

namespace MonsterCardGame.Gameplay.World
{
    public interface IWorldProgressionService : IService
    {
        IReadOnlyList<ZoneData> AllZones { get; }

        bool IsZoneUnlocked(ZoneData zone);
        bool IsMonsterDefeated(MonsterData monster);
        bool IsBossDefeated(ZoneData zone);
        void RecordVictory(MonsterData monster);

        IReadOnlyCollection<string> DefeatedMonsterNames { get; }
        void LoadDefeated(IEnumerable<string> names);
    }
}
