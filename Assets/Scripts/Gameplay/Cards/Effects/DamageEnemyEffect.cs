using MonsterCardGame.Gameplay.Cards.Effects;
using MonsterCardGame.Gameplay.Combat;
using UnityEngine;

namespace MonsterCardGame.Gameplay
{
    [CreateAssetMenu(menuName = "MonsterCardGame/Cards/Effects/Damage Enemy")]
    public class DamageEnemyEffect : CardEffect
    {
        [SerializeField] private int _quantity;

        public override void Apply(CardEffectContext ctx)
        {
            if (ctx.IsPlayer)
            {
                ctx.Combat.MonsterHP -= _quantity;
                Core.GameLog.Info("DamageEnemyEffect", $"Monstre subit {_quantity} dégats");
            }
            else
            {
                ctx.Combat.PlayerHP -= _quantity;
                Core.GameLog.Info("DamageEnemyEffect", $"Joueur subit {_quantity} dégats");
            }
        }
    }
}