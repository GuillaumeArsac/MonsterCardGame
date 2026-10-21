using System.Collections.Generic;
using UnityEngine;

namespace MonsterCardGame.Gameplay.World
{
    [CreateAssetMenu(menuName = "MonsterCardGame/World/World Data")]
    public class WorldData : ScriptableObject
    {
        [SerializeField, Tooltip("Toutes les zones du jeu, dans l'ordre d'affichage")]
        private List<ZoneData> _zones = new();

        public IReadOnlyList<ZoneData> Zones => _zones;

        /// <summary>Remplace la liste des zones. Pour les scripts d'éditeur et les tests.</summary>
        public void SetZones(IEnumerable<ZoneData> zones) => _zones = new List<ZoneData>(zones);
    }
}
