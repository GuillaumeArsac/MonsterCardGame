using System.Collections.Generic;
using System.Linq;
using MonsterCardGame.Gameplay.Content;
using MonsterCardGame.Gameplay.Monsters;

namespace MonsterCardGame.Gameplay.World
{
    /// <summary>Passe la collection du joueur à ses données de sauvegarde, et inversement.</summary>
    public static class CollectionSaveMapper
    {
        public static void Write(MonsterCollection collection, SaveData data)
        {
            data.Collection = collection.Monsters.Select(m => new OwnedMonsterSave
            {
                Card             = m.Origin.name,
                Skills           = m.Skills.Select(s => s.name).ToList(),
                BoostHp          = m.Boost.Hp,
                LockedSkillIndex = m.LockedSkillIndex
            }).ToList();
            data.Team = collection.TeamIndices.ToList();
        }

        /// <summary>
        /// Recharge la collection. Une carte introuvable retire le monstre ; une compétence introuvable est retirée.
        /// Retourne la liste des éléments ignorés, pour les signaler.
        /// </summary>
        public static List<string> Read(SaveData data, ContentDatabase content, MonsterCollection collection)
        {
            var ignored  = new List<string>();
            var monsters = new List<OwnedMonster>();
            var indexMap = new Dictionary<int, int>();

            for (int i = 0; i < data.Collection.Count; i++)
            {
                var saved = data.Collection[i];
                var card  = content.FindMonster(saved.Card);
                if (card == null) { ignored.Add($"monstre {saved.Card}"); continue; }

                var skills = new List<Skills.SkillData>();
                int locked = OwnedMonster.NoLockedSkill;
                for (int s = 0; s < saved.Skills.Count; s++)
                {
                    var skill = content.FindSkill(saved.Skills[s]);
                    if (skill == null) { ignored.Add($"compétence {saved.Skills[s]} de {saved.Card}"); continue; }
                    if (s == saved.LockedSkillIndex) locked = skills.Count;
                    skills.Add(skill);
                }

                indexMap[i] = monsters.Count;
                monsters.Add(new OwnedMonster(card, skills.Take(Core.GameRules.MaxSkillsPerMonster), new StatBoost(saved.BoostHp),
                                              locked < Core.GameRules.MaxSkillsPerMonster ? locked : OwnedMonster.NoLockedSkill));
            }

            var team = data.Team.Where(indexMap.ContainsKey).Select(i => indexMap[i]);
            collection.Load(monsters, team);
            return ignored;
        }
    }
}
