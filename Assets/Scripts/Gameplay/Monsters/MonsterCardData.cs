using System.Collections.Generic;
using UnityEngine;
using MonsterCardGame.Core;
using MonsterCardGame.Gameplay.Skills;

namespace MonsterCardGame.Gameplay.Monsters
{
    /// <summary>Carte monstre. Référence : docs/monstres.md.</summary>
    [CreateAssetMenu(menuName = "MonsterCardGame/Monsters/Monster Card Data")]
    public class MonsterCardData : ScriptableObject
    {
        [Header("Identité")]
        [SerializeField, Tooltip("Nom affiché sur la carte")]
        private string _monsterName;

        [SerializeField] private Sprite _artwork;

        [SerializeField, Tooltip("Pour le design et l'IA seulement, n'est pas affiché sur la carte")]
        private MonsterRole _role;

        [Header("Stats")]
        [SerializeField, Min(1), Tooltip("PV de départ")]
        private int _maxHp = 1;

        [Header("Compétences")]
        [SerializeField, Tooltip("Jusqu'à 3 compétences, actives et passives comprises. Une compétence peut apparaître plusieurs fois")]
        private List<SkillData> _skills = new();

        [SerializeField, Tooltip("Monstres de quête : compétence unique, qui ne peut jamais être remplacée. Doit faire partie des compétences")]
        private SkillData _uniqueSkill;

        [Header("Dépeçage")]
        [SerializeField, Tooltip("Boost donné au joueur s'il choisit le boost lors du dépeçage de ce monstre")]
        private StatBoost _statBoost;

        public string                   MonsterName => _monsterName;
        public Sprite                   Artwork     => _artwork;
        public MonsterRole              Role        => _role;
        public int                      MaxHp       => _maxHp;
        public IReadOnlyList<SkillData> Skills      => _skills;
        public SkillData                UniqueSkill => _uniqueSkill;
        public StatBoost                StatBoost   => _statBoost;

        /// <summary>Remplit la carte. Pour les scripts d'éditeur et les tests.</summary>
        public void Init(string monsterName, MonsterRole role, int maxHp, IEnumerable<SkillData> skills,
                         StatBoost statBoost = default, SkillData uniqueSkill = null, Sprite artwork = null)
        {
            _monsterName = monsterName;
            _role        = role;
            _maxHp       = maxHp;
            _skills      = new List<SkillData>(skills);
            _statBoost   = statBoost;
            _uniqueSkill = uniqueSkill;
            _artwork     = artwork;
            Validate();
        }

        public static MonsterCardData Create(string monsterName, MonsterRole role, int maxHp,
                                             StatBoost statBoost, params SkillData[] skills)
        {
            var card = CreateInstance<MonsterCardData>();
            card.Init(monsterName, role, maxHp, skills, statBoost);
            return card;
        }

        private void OnValidate() => Validate();

        private void Validate()
        {
            if (_skills.Count > GameRules.MaxSkillsPerMonster)
                GameLog.Warning("MonsterCardData", $"{name} : {_skills.Count} compétences, maximum {GameRules.MaxSkillsPerMonster}");

            if (_uniqueSkill != null && !_skills.Contains(_uniqueSkill))
                GameLog.Warning("MonsterCardData", $"{name} : la compétence unique {_uniqueSkill.name} ne fait pas partie de ses compétences");
        }
    }
}
