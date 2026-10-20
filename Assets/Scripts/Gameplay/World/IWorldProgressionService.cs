using System.Collections.Generic;
using MonsterCardGame.Core.Services;

namespace MonsterCardGame.Gameplay.World
{
    public interface IWorldProgressionService : IService
    {
        IReadOnlyList<ZoneData> AllZones { get; }

        bool IsZoneUnlocked(ZoneData zone);

        /// <summary>True si le combat du scénario identifié par <paramref name="encounterId"/> a déjà été gagné.</summary>
        bool IsEncounterCompleted(string encounterId);
        void RecordVictory(string encounterId);

        IReadOnlyCollection<string> CompletedEncounterIds { get; }
        void LoadCompleted(IEnumerable<string> encounterIds);
    }
}
