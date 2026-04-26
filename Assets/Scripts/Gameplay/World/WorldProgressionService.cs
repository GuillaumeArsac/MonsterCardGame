using System.Collections.Generic;
using MonsterCardGame.Gameplay.Combat.Data;

namespace MonsterCardGame.Gameplay.World
{
    public class WorldProgressionService : IWorldProgressionService
    {
        private readonly WorldData          _worldData;
        private readonly HashSet<string>    _defeatedMonsters = new();

        public WorldProgressionService(WorldData worldData) => _worldData = worldData;

        public IReadOnlyList<ZoneData>      AllZones             => _worldData?.Zones ?? new List<ZoneData>();
        public IReadOnlyCollection<string>  DefeatedMonsterNames => _defeatedMonsters;

        public bool IsZoneUnlocked(ZoneData zone)
        {
            if (zone == null || !zone.RequiresAllOtherBossesDefeated) return true;
            if (_worldData == null) return false;

            foreach (var z in _worldData.Zones)
            {
                if (z == zone) continue;
                if (!IsBossDefeated(z)) return false;
            }
            return true;
        }

        public bool IsMonsterDefeated(MonsterData monster)
            => monster != null && _defeatedMonsters.Contains(monster.name);

        public bool IsBossDefeated(ZoneData zone)
            => zone?.Boss != null && IsMonsterDefeated(zone.Boss);

        public void RecordVictory(MonsterData monster)
        {
            if (monster == null) return;
            _defeatedMonsters.Add(monster.name);
            Core.GameLog.Info("WorldProgression", $"Monstre vaincu enregistré : {monster.MonsterName}");
        }

        public void LoadDefeated(IEnumerable<string> names)
        {
            _defeatedMonsters.Clear();
            foreach (var n in names) _defeatedMonsters.Add(n);
        }
    }
}
