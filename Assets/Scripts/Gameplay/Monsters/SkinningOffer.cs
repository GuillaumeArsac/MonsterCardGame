using System;
using System.Collections.Generic;
using System.Linq;
using MonsterCardGame.Gameplay.Encounters;
using MonsterCardGame.Gameplay.Skills;

namespace MonsterCardGame.Gameplay.Monsters
{
    /// <summary>
    /// Récompense du dépeçage après une victoire : un monstre adverse tiré au hasard, dont le joueur prend
    /// une compétence ou le boost de stats, pour l'un de ses monstres. Référence : docs/progression.md (Dépeçage).
    /// </summary>
    public class SkinningOffer
    {
        /// <summary>Monstre adverse dépecé, tiré parmi toute l'équipe adverse, vaincu ou non.</summary>
        public MonsterCardData Monster { get; }
        public bool IsClaimed { get; private set; }

        /// <summary>Compétences proposées : celles du monstre, sans doublon.</summary>
        public IReadOnlyList<SkillData> Skills { get; }
        public StatBoost Boost    => Monster.StatBoost;
        public bool      HasBoost => !Monster.StatBoost.IsEmpty;

        public SkinningOffer(MonsterCardData monster)
        {
            Monster = monster ?? throw new ArgumentNullException(nameof(monster));
            Skills  = monster.Skills.Where(s => s != null).Distinct().ToList();
        }

        public static SkinningOffer Draw(EncounterData encounter, Random random)
        {
            var candidates = encounter.Monsters.Where(m => m != null).ToList();
            if (candidates.Count == 0) throw new ArgumentException($"{encounter.name} n'a aucun monstre");
            return new SkinningOffer(candidates[random.Next(candidates.Count)]);
        }

        /// <summary>Vrai si <paramref name="target"/> a déjà 3 compétences : il faudra en remplacer une.</summary>
        public static bool NeedsReplacement(OwnedMonster target) => !target.HasFreeSkillSlot;

        /// <summary>
        /// Donne une compétence du monstre dépecé. Si <paramref name="target"/> a déjà 3 compétences,
        /// <paramref name="replacedIndex"/> désigne celle qui est remplacée (et perdue).
        /// </summary>
        public void GiveSkill(SkillData skill, OwnedMonster target, int replacedIndex = -1)
        {
            EnsureNotClaimed();
            if (!Skills.Contains(skill)) throw new ArgumentException($"{skill?.SkillName} n'est pas une compétence de {Monster.MonsterName}");

            if (target.HasFreeSkillSlot) target.AddSkill(skill);
            else target.ReplaceSkill(replacedIndex, skill);

            IsClaimed = true;
        }

        /// <summary>Donne le boost du monstre dépecé. Il remplace le boost que <paramref name="target"/> portait.</summary>
        public void GiveBoost(OwnedMonster target)
        {
            EnsureNotClaimed();
            if (!HasBoost) throw new InvalidOperationException($"{Monster.MonsterName} n'a pas de boost de stats");

            target.SetBoost(Boost);
            IsClaimed = true;
        }

        private void EnsureNotClaimed()
        {
            if (IsClaimed) throw new InvalidOperationException("La récompense du dépeçage a déjà été prise");
        }
    }
}
