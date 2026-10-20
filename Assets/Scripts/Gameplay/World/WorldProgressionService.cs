using System.Collections.Generic;

namespace MonsterCardGame.Gameplay.World
{
    public class WorldProgressionService : IWorldProgressionService
    {
        private readonly WorldData       _worldData;
        private readonly HashSet<string> _completedEncounters = new();

        public WorldProgressionService(WorldData worldData) => _worldData = worldData;

        public IReadOnlyList<ZoneData>     AllZones              => _worldData?.Zones ?? new List<ZoneData>();
        public IReadOnlyCollection<string> CompletedEncounterIds => _completedEncounters;

        // Les combats de zone (et donc les boss) seront branchés à l'étape 8 du plan :
        // d'ici là, une zone qui exige les autres boss reste verrouillée.
        public bool IsZoneUnlocked(ZoneData zone)
            => zone != null && !zone.RequiresAllOtherBossesDefeated;

        public bool IsEncounterCompleted(string encounterId)
            => !string.IsNullOrEmpty(encounterId) && _completedEncounters.Contains(encounterId);

        public void RecordVictory(string encounterId)
        {
            if (string.IsNullOrEmpty(encounterId)) return;
            _completedEncounters.Add(encounterId);
            Core.GameLog.Info("WorldProgression", $"Combat gagné enregistré : {encounterId}");
        }

        public void LoadCompleted(IEnumerable<string> encounterIds)
        {
            _completedEncounters.Clear();
            foreach (var id in encounterIds) _completedEncounters.Add(id);
        }
    }
}
