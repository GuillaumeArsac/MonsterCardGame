using System;

namespace MonsterCardGame.Gameplay.Skills.Effects
{
    /// <summary>
    /// Brique d'effet d'une compétence — une sous-classe par type d'effet.
    /// Classe C# pure, sérialisée dans SkillData avec [SerializeReference] : le moteur de combat
    /// peut l'utiliser sans dépendre de Unity.
    /// </summary>
    [Serializable]
    public abstract class SkillEffect
    {
    }
}
