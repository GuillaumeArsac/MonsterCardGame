using UnityEngine;

namespace MonsterCardGame.Gameplay.Cards.Effects
{
    /// <summary>Effet de carte — ScriptableObject abstrait. Créer une sous-classe par type d'effet.</summary>
    public abstract class CardEffect : ScriptableObject
    {
        public abstract void Apply(CardEffectContext ctx);
    }
}
