using System;
using System.Collections.Generic;

namespace MonsterCardGame.Gameplay.World
{
    [Serializable]
    public class SaveData
    {
        public List<string>            OwnedCardNames    = new();
        public List<MaterialSaveEntry> OwnedMaterials    = new();
        public List<string>            ActiveDeckNames   = new();
        public List<string>            DefeatedMonsters  = new();
    }

    [Serializable]
    public class MaterialSaveEntry
    {
        public string MaterialName;
        public int    Count;
    }
}
