using System;
using System.Collections.Generic;

namespace MonsterCardGame.Gameplay.World
{
    [Serializable]
    public class SaveData
    {
        public List<string> CompletedEncounters = new();

        /// <summary>Monstres du joueur. Vide dans une sauvegarde d'avant la collection : nouvelle collection de départ.</summary>
        public List<OwnedMonsterSave> Collection = new();

        /// <summary>Index des monstres de l'équipe dans Collection.</summary>
        public List<int> Team = new();
    }

    /// <summary>Un monstre de la collection. Les compétences et la carte sont désignées par le nom de leur asset.</summary>
    [Serializable]
    public class OwnedMonsterSave
    {
        public string       Card;
        public List<string> Skills = new();
        public int          BoostHp;
        public int          LockedSkillIndex = -1;
    }
}
