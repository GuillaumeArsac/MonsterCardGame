using System.Collections.Generic;
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
                DestroyAll(ctx, ctx.Combat.MonsterAllies, ctx.Combat.MonsterCemetery, isPlayer: false);

            if ((_opponent && !ctx.IsPlayer) || (_self && ctx.IsPlayer))
                DestroyAll(ctx, ctx.Combat.PlayerAllies, ctx.Combat.PlayerCemetery, isPlayer: true);
        }

        private static void DestroyAll(
            CardEffectContext ctx,
            List<AlliedInstance> zone,
            List<Cards.CardData> cemetery,
            bool isPlayer)
        {
            // Copie pour éviter une modification de la liste pendant l'itération
            var snapshot = new List<AlliedInstance>(zone);
            foreach (var ally in snapshot)
                if (ally.Data.CardType == Cards.CardType.Allie)
                    CombatHelper.DestroyAlly(ctx.Combat, zone, cemetery, ally, isPlayer);
        }
    }
}