using UnityEngine;

namespace MonsterCardGame.Gameplay.Cards.Effects
{
    [CreateAssetMenu(menuName = "MonsterCardGame/Cards/Effects/Heal Player")]
    public class HealPlayerEffect : CardEffect
    {
        [SerializeField, Tooltip("PV rendus au joueur")]
        private int _amount;

        public override void Apply(CardEffectContext ctx)
        {
            if (ctx.IsPlayer)
            {
                ctx.Combat.PlayerHP += _amount;
                Core.GameLog.Info("HealPlayerEffect", $"Joueur soigné de {_amount} PV → {ctx.Combat.PlayerHP} PV");
            }
            else
            {
                ctx.Combat.MonsterHP += _amount;
                Core.GameLog.Info("HealPlayerEffect", $"Monstre soigné de {_amount} PV → {ctx.Combat.MonsterHP} PV");
            }
        }
    }
}