using System.Collections.Generic;
using UnityEngine;
using MonsterCardGame.Gameplay.Combat.Data;

namespace MonsterCardGame.Gameplay.Inventory
{
    public static class LootResolver
    {
        /// <summary>
        /// Tire chaque entrée de la loot table indépendamment.
        /// Retourne la liste des matériaux droppés (peut contenir des doublons si même matériau présent plusieurs fois).
        /// </summary>
        public static List<MaterialData> Resolve(MonsterData monster)
        {
            var drops = new List<MaterialData>();
            if (monster == null) return drops;

            foreach (var entry in monster.LootTable)
            {
                if (entry.Material == null) continue;
                if (Random.value <= entry.DropChance)
                    drops.Add(entry.Material);
            }

            Core.GameLog.Info("LootResolver", $"{drops.Count} matériau(x) obtenu(s) sur {monster.LootTable.Count} entrée(s)");
            return drops;
        }
    }
}
