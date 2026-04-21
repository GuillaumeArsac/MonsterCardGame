using System;
using UnityEngine;

namespace MonsterCardGame.Gameplay.Inventory
{
    [Serializable]
    public struct LootEntry
    {
        [SerializeField]
        public MaterialData Material;

        [SerializeField, Range(0f, 1f), Tooltip("Probabilité de drop (0.0 = jamais, 1.0 = toujours)")]
        public float DropChance;
    }
}
