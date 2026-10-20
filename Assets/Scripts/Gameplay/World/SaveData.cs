using System;
using System.Collections.Generic;

namespace MonsterCardGame.Gameplay.World
{
    [Serializable]
    public class SaveData
    {
        public List<string> CompletedEncounters = new();
    }
}
