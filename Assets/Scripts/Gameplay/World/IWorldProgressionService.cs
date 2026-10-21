using System.Collections.Generic;
using MonsterCardGame.Core.Services;
using MonsterCardGame.Gameplay.Encounters;

namespace MonsterCardGame.Gameplay.World
{
    /// <summary>État d'un combat sur la carte du monde.</summary>
    public enum EncounterState
    {
        /// <summary>Peut être lancé.</summary>
        Available,
        /// <summary>Combat du scénario déjà gagné : il ne se rejoue pas.</summary>
        Completed,
        /// <summary>Combat du scénario dont les combats précédents ne sont pas encore gagnés.</summary>
        Locked
    }

    public interface IWorldProgressionService : IService
    {
        IReadOnlyList<ZoneData> AllZones { get; }

        bool IsZoneUnlocked(ZoneData zone);
        bool IsBossDefeated(ZoneData zone);
        EncounterState GetEncounterState(ZoneData zone, EncounterData encounter);

        /// <summary>True si le combat identifié par <paramref name="encounterId"/> a déjà été gagné.</summary>
        bool IsEncounterCompleted(string encounterId);
        void RecordVictory(string encounterId);

        IReadOnlyCollection<string> CompletedEncounterIds { get; }
        void LoadCompleted(IEnumerable<string> encounterIds);
    }
}
