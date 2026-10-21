using System.Collections.Generic;
using UnityEngine;
using MonsterCardGame.Gameplay.Skills.Effects;

namespace MonsterCardGame.Gameplay.Skills
{
    /// <summary>Fiche d'une compétence. Référence : docs/competences.md.</summary>
    [CreateAssetMenu(menuName = "MonsterCardGame/Skills/Skill Data")]
    public class SkillData : ScriptableObject
    {
        [Header("Identité")]
        [SerializeField, Tooltip("Nom affiché sur la carte")]
        private string _skillName;

        [SerializeField] private SkillType _type;

        [SerializeField, Tooltip("Pour le design seulement, n'est pas affiché sur la carte")]
        private MonsterRole _role;

        [SerializeField] private SkillTarget _target;

        [Header("Active")]
        [SerializeField, Min(0), Tooltip("Temps de recharge, en phases du camp du monstre. Actives seulement")]
        private int _cooldown;

        [Header("Passive")]
        [SerializeField, Tooltip("Moment où l'effet s'applique. Passives seulement")]
        private PassiveTrigger _trigger;

        [Header("Effets")]
        [SerializeReference, Tooltip("Appliqués dans l'ordre, à chaque cible")]
        private List<SkillEffect> _effects = new();

        public string                     SkillName => _skillName;
        public SkillType                  Type      => _type;
        public MonsterRole                Role      => _role;
        public SkillTarget                Target    => _target;
        public int                        Cooldown  => _cooldown;
        public PassiveTrigger             Trigger   => _trigger;
        public IReadOnlyList<SkillEffect> Effects   => _effects;

        public bool IsActive  => _type == SkillType.Active;
        public bool IsPassive => _type == SkillType.Passive;

        /// <summary>Remplit la fiche. Pour les scripts d'éditeur et les tests.</summary>
        public void Init(string skillName, SkillType type, MonsterRole role, SkillTarget target,
                         int cooldown, PassiveTrigger trigger, params SkillEffect[] effects)
        {
            _skillName = skillName;
            _type      = type;
            _role      = role;
            _target    = target;
            _cooldown  = cooldown;
            _trigger   = trigger;
            _effects   = new List<SkillEffect>(effects);
        }

        public static SkillData CreateActive(string skillName, MonsterRole role, SkillTarget target,
                                             int cooldown, params SkillEffect[] effects)
        {
            var skill = CreateInstance<SkillData>();
            skill.Init(skillName, SkillType.Active, role, target, cooldown, PassiveTrigger.Always, effects);
            return skill;
        }

        public static SkillData CreatePassive(string skillName, MonsterRole role, SkillTarget target,
                                              PassiveTrigger trigger, params SkillEffect[] effects)
        {
            var skill = CreateInstance<SkillData>();
            skill.Init(skillName, SkillType.Passive, role, target, 0, trigger, effects);
            return skill;
        }

        /// <summary>Retourne le premier effet du type demandé, ou null.</summary>
        public T GetEffect<T>() where T : SkillEffect
        {
            foreach (var effect in _effects)
                if (effect is T typed) return typed;
            return null;
        }
    }
}
