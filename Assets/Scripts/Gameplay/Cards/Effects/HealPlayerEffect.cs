using UnityEngine;

namespace MonsterCardGame.Gameplay.Cards.Effects
{
    [CreateAssetMenu(menuName = "MonsterCardGame/Cards/Effects/Heal Player")]
    public class HealPlayerEffect : CardEffect
    {
        [SerializeField, Tooltip("PV rendus au joueur")]
        private int _amount = 1;

        public override void Apply(CardEffectContext ctx)
        {
            ctx.Combat.PlayerHP += _amount;
            Core.GameLog.Info("HealPlayerEffect", $"Joueur soigné de {_amount} PV → {ctx.Combat.PlayerHP} PV");
        }
    }
}
