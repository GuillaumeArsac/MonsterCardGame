using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MonsterCardGame.Core.Services;
using MonsterCardGame.Gameplay.Encounters;
using MonsterCardGame.Gameplay.Monsters;
using MonsterCardGame.Gameplay.Skills;

namespace MonsterCardGame.Gameplay.Content
{
    /// <summary>
    /// Liste de tout le contenu du jeu, pour retrouver un asset à partir de l'identifiant stocké dans la sauvegarde.
    /// Générée par le menu MonsterCardGame > Contenu, dans Resources/ContentDatabase.asset.
    /// L'identifiant d'une compétence ou d'un monstre est le nom de son asset : le renommer casse les sauvegardes.
    /// </summary>
    [CreateAssetMenu(menuName = "MonsterCardGame/Content Database")]
    public class ContentDatabase : ScriptableObject, IService
    {
        public const string ResourcePath = "ContentDatabase";

        [SerializeField] private List<SkillData>       _skills   = new();
        [SerializeField] private List<MonsterCardData> _monsters = new();
        [SerializeField, Tooltip("Collection donnée au joueur en début de partie")]
        private List<MonsterCardData> _starterMonsters = new();
        [SerializeField] private List<EncounterData>   _encounters = new();

        public IReadOnlyList<SkillData>       Skills          => _skills;
        public IReadOnlyList<MonsterCardData> Monsters        => _monsters;
        public IReadOnlyList<MonsterCardData> StarterMonsters => _starterMonsters;
        public IReadOnlyList<EncounterData>   Encounters      => _encounters;

        public SkillData       FindSkill(string id)     => _skills.FirstOrDefault(s => s != null && s.name == id);
        public MonsterCardData FindMonster(string id)   => _monsters.FirstOrDefault(m => m != null && m.name == id);
        public EncounterData   FindEncounter(string id) => _encounters.FirstOrDefault(e => e != null && e.EncounterId == id);

        /// <summary>Remplit la base. Pour les scripts d'éditeur et les tests.</summary>
        public void Init(IEnumerable<SkillData> skills, IEnumerable<MonsterCardData> monsters,
                         IEnumerable<MonsterCardData> starterMonsters, IEnumerable<EncounterData> encounters)
        {
            _skills          = skills.ToList();
            _monsters        = monsters.ToList();
            _starterMonsters = starterMonsters.ToList();
            _encounters      = encounters.ToList();
        }
    }
}
