using System;
using System.Collections.Generic;
using MonsterCardGame.Core;
using MonsterCardGame.Gameplay.Skills;

namespace MonsterCardGame.Gameplay.Monsters
{
    /// <summary>
    /// Monstre de la collection du joueur : sa carte d'origine, modifiée par les dépeçages.
    /// Référence : docs/progression.md.
    /// </summary>
    public class OwnedMonster
    {
        /// <summary>Valeur de LockedSkillIndex quand le monstre n'a pas de compétence unique.</summary>
        public const int NoLockedSkill = -1;

        private readonly List<SkillData> _skills;

        public MonsterCardData          Origin           { get; }
        public IReadOnlyList<SkillData> Skills           => _skills;
        public StatBoost                Boost            { get; private set; }
        /// <summary>Index de la compétence unique (monstres de quête), qui ne peut jamais être remplacée.</summary>
        public int                      LockedSkillIndex { get; }

        public string Name             => Origin.MonsterName;
        public int    MaxHp            => Origin.MaxHp + Boost.Hp;
        public bool   HasFreeSkillSlot => _skills.Count < GameRules.MaxSkillsPerMonster;

        /// <summary>Monstre tout juste obtenu : les compétences et la compétence unique de sa carte, sans boost.</summary>
        public OwnedMonster(MonsterCardData origin)
            : this(origin, origin != null ? origin.Skills : null, StatBoost.None, FindLockedIndex(origin))
        {
        }

        /// <summary>Monstre déjà modifié, par exemple rechargé depuis la sauvegarde.</summary>
        public OwnedMonster(MonsterCardData origin, IEnumerable<SkillData> skills, StatBoost boost, int lockedSkillIndex)
        {
            Origin = origin ?? throw new ArgumentNullException(nameof(origin));
            _skills = new List<SkillData>(skills ?? Array.Empty<SkillData>());

            if (_skills.Count > GameRules.MaxSkillsPerMonster)
                throw new ArgumentException($"{origin.MonsterName} : {_skills.Count} compétences, maximum {GameRules.MaxSkillsPerMonster}", nameof(skills));
            if (lockedSkillIndex != NoLockedSkill && (lockedSkillIndex < 0 || lockedSkillIndex >= _skills.Count))
                throw new ArgumentOutOfRangeException(nameof(lockedSkillIndex));

            Boost            = boost;
            LockedSkillIndex = lockedSkillIndex;
        }

        public bool IsLocked(int skillIndex) => skillIndex == LockedSkillIndex;

        /// <summary>Ajoute une compétence dans un emplacement libre. Un doublon est permis.</summary>
        public void AddSkill(SkillData skill)
        {
            if (skill == null) throw new ArgumentNullException(nameof(skill));
            if (!HasFreeSkillSlot)
                throw new InvalidOperationException($"{Name} a déjà {GameRules.MaxSkillsPerMonster} compétences : il faut en remplacer une");

            _skills.Add(skill);
        }

        /// <summary>Remplace une compétence. La compétence remplacée est perdue.</summary>
        public void ReplaceSkill(int skillIndex, SkillData skill)
        {
            if (skill == null) throw new ArgumentNullException(nameof(skill));
            if (skillIndex < 0 || skillIndex >= _skills.Count) throw new ArgumentOutOfRangeException(nameof(skillIndex));
            if (IsLocked(skillIndex))
                throw new InvalidOperationException($"La compétence unique {_skills[skillIndex].SkillName} de {Name} ne peut pas être remplacée");

            _skills[skillIndex] = skill;
        }

        /// <summary>Donne un boost au monstre. Il n'en porte qu'un : le nouveau remplace le précédent.</summary>
        public void SetBoost(StatBoost boost) => Boost = boost;

        private static int FindLockedIndex(MonsterCardData origin)
        {
            if (origin == null || origin.UniqueSkill == null) return NoLockedSkill;

            for (int i = 0; i < origin.Skills.Count; i++)
                if (origin.Skills[i] == origin.UniqueSkill) return i;

            return NoLockedSkill;
        }
    }
}
