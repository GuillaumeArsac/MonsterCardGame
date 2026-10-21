using System;
using UnityEngine;

namespace MonsterCardGame.Gameplay.Monsters
{
    /// <summary>
    /// Boost de stats qu'un monstre adverse donne au dépeçage. Ne concerne que les PV pour l'instant.
    /// Référence : docs/progression.md (échelle des boosts).
    /// </summary>
    [Serializable]
    public struct StatBoost
    {
        [SerializeField, Min(0), Tooltip("PV ajoutés aux PV de départ du monstre qui porte le boost")]
        private int _hp;

        public int  Hp      => _hp;
        public bool IsEmpty => _hp == 0;

        public static readonly StatBoost None = default;

        public StatBoost(int hp) => _hp = hp;

        public override string ToString() => IsEmpty ? "aucun boost" : $"+{_hp} PV";
    }
}
