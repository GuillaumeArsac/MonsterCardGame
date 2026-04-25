using MonsterCardGame.Gameplay.Cards.Effects;
using MonsterCardGame.Gameplay.Combat;
using UnityEngine;

namespace MonsterCardGame.Gameplay
{
    [CreateAssetMenu(menuName = "MonsterCardGame/Cards/Effects/Clear Board")]
    public class ClearBoardEffect : CardEffect
    {
        [SerializeField] private bool _opponent;
        [SerializeField] private bool _self;

        public override void Apply(CardEffectContext ctx)
        {
            if ((_opponent && ctx.IsPlayer) || (_self && !ctx.IsPlayer))
            {
                foreach (var a in ctx.Combat.MonsterAllies)
                {
                    ctx.Combat.MonsterCemetery.Add(a.Data);
                }
                ctx.Combat.MonsterAllies.Clear();
            }

            if ((_opponent && !ctx.IsPlayer) || (_self && ctx.IsPlayer))
            {
                foreach (var a in ctx.Combat.PlayerAllies)
                {
                    ctx.Combat.PlayerCemetery.Add(a.Data);
                }
                ctx.Combat.PlayerAllies.Clear();
            }
        }
    }
}