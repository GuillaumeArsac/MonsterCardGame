using System.Collections.Generic;
using MonsterCardGame.Gameplay.Encounters;

namespace MonsterCardGame.Gameplay.World
{
    public class WorldProgressionService : IWorldProgressionService
    {
        private readonly WorldData       _worldData;
        private readonly HashSet<string> _completedEncounters = new();

        public WorldProgressionService(WorldData worldData) => _worldData = worldData;

        public IReadOnlyList<ZoneData>     AllZones              => _worldData?.Zones ?? new List<ZoneData>();
        public IReadOnlyCollection<string> CompletedEncounterIds => _completedEncounters;

        /// <summary>Une zone qui l'exige (les Ruines) reste verrouillée tant que le boss de chaque autre zone n'est pas vaincu.</summary>
        public bool IsZoneUnlocked(ZoneData zone)
        {
            if (zone == null) return false;
            if (!zone.RequiresAllOtherBossesDefeated) return true;

            foreach (var other in AllZones)
                if (other != zone && !IsBossDefeated(other)) return false;
            return true;
        }

        /// <summary>Faux tant que la zone n'a pas de boss défini.</summary>
        public bool IsBossDefeated(ZoneData zone)
            => zone?.BossEncounter != null && IsEncounterCompleted(zone.BossEncounter.EncounterId);

        public EncounterState GetEncounterState(ZoneData zone, EncounterData encounter)
        {
            if (encounter == null || !IsZoneUnlocked(zone)) return EncounterState.Locked;
            if (encounter.IsRepeatable) return EncounterState.Available;
            if (IsEncounterCompleted(encounter.EncounterId)) return EncounterState.Completed;

            // Combats du scénario dans l'ordre : chacun attend la victoire des précédents
            foreach (var previous in zone.ScenarioEncounters)
            {
                if (previous == encounter) return EncounterState.Available;
                if (previous != null && !IsEncounterCompleted(previous.EncounterId)) return EncounterState.Locked;
            }
            return EncounterState.Available;
        }

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
