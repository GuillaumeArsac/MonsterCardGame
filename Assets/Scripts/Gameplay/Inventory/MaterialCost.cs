using System;
using UnityEngine;

namespace MonsterCardGame.Gameplay.Inventory
{
    [Serializable]
    public struct MaterialCost
    {
        [SerializeField]
        public MaterialData Material;

        [SerializeField, Min(1)]
        public int Quantity;
    }
}
