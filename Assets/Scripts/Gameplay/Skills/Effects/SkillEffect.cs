using System;
using UnityEngine;

namespace MonsterCardGame.Gameplay.Skills.Effects
{
    /// <summary>À qui s'applique une brique d'effet.</summary>
    public enum EffectTarget
    {
        /// <summary>Les cibles de la compétence.</summary>
        SkillTargets,
        /// <summary>Le monstre qui utilise la compétence, quelle que soit sa cible (Essence de l'ombre).</summary>
        Self,
        /// <summary>Tous les alliés sur le terrain, y compris ce monstre (Essence de vie).</summary>
        AllAllies
    }

    /// <summary>
    /// Brique d'effet d'une compétence — une sous-classe par type d'effet.
    /// Classe C# pure, sérialisée dans SkillData avec [SerializeReference] : le moteur de combat
    /// peut l'utiliser sans dépendre de Unity.
    /// </summary>
    [Serializable]
    public abstract class SkillEffect
    {
        [SerializeField, Tooltip("À qui s'applique cet effet : les cibles de la compétence, ce monstre, ou tous les alliés")]
        private EffectTarget _appliesTo;

        public EffectTarget AppliesTo => _appliesTo;

        /// <summary>Applique cet effet à d'autres monstres que les cibles de la compétence.</summary>
        public SkillEffect On(EffectTarget target)
        {
            _appliesTo = target;
            return this;
        }
    }
}
