using System;
using MonsterCardGame.Core.Services;
using MonsterCardGame.Gameplay.Cards;
using MonsterCardGame.Gameplay.Cards.Effects;
using MonsterCardGame.Gameplay.Combat.Keywords;

namespace MonsterCardGame.Gameplay.Combat.MonsterAI
{
    /// <summary>
    /// IA scriptée : joue la première carte du deck monstre chaque tour.
    /// Allie → zone alliés (consommée). Action → bas du deck (rotation).
    /// </summary>
    public class MonsterAIController
    {
        private IKeywordResolver _resolver;

        public void ExecuteNextAction(CombatContext ctx, Action<CombatContext, bool> onComplete)
        {
            _resolver ??= Services.Get<IKeywordResolver>();

            // Phase 1 : attaques des alliés éveillés (un par appel — ReactiveWindow entre chaque)
            var attacker = ctx.MonsterAllies.Find(a => !a.IsSleeping);
            if (attacker != null)
            {
                AllyAttack(ctx, attacker);
                CheckCombatEnd(ctx);
                onComplete(ctx, false);
                return;
            }

            // Phase 2 : jouer une carte de la main
            if (ctx.MonsterHand.Count == 0)
            {
                Core.GameLog.Info("MonsterAI", "Main monstre vide — fin du tour monstre");
                onComplete(ctx, false);
                return;
            }

            var card = ctx.MonsterHand[0];
            ctx.MonsterHand.RemoveAt(0);

            ExecuteCard(ctx, card);
            CheckCombatEnd(ctx);

            onComplete(ctx, false);
        }

        private void AllyAttack(CombatContext ctx, AlliedInstance attacker)
        {
            attacker.SetSleeping(true);

            var target = _resolver.GetPriorityTarget(ctx.PlayerAllies);
            ctx.PendingMonsterAction = attacker.Data;
            ctx.PendingMonsterTarget = target;

            foreach (var effect in attacker.Data.OnAttackEffects)
                effect.Apply(new CardEffectContext(ctx, attacker, isPlayer: false));

            var targetName = target != null ? target.Data.CardName : "le joueur";
            Core.GameLog.Info("MonsterAI", $"Allié {attacker.Data.CardName} attaque {targetName}");
        }

        private void ExecuteCard(CombatContext ctx, CardData card)
        {
            Core.GameLog.Info("MonsterAI", $"Monstre joue : {card.CardName}");

            AlliedInstance source = null;

            switch (card.CardType)
            {
                case CardType.Allie:
                    source = new AlliedInstance(card);
                    ctx.MonsterAllies.Add(source);
                    break;

                case CardType.Action:
                    AnnounceAction(ctx, card);
                    ctx.MonsterDeck.Add(card);
                    break;

                default:
                    Core.GameLog.Info("MonsterAI", $"{card.CardName} (type {card.CardType}) — ignorée pour MVP");
                    break;
            }

            foreach (var effect in card.OnPlayEffects)
                effect.Apply(new CardEffectContext(ctx, source, isPlayer: false));
        }

        private void AnnounceAction(CombatContext ctx, CardData card)
        {
            _resolver ??= Services.Get<IKeywordResolver>();
            var target = _resolver.GetPriorityTarget(ctx.PlayerAllies);
            ctx.PendingMonsterAction = card;
            ctx.PendingMonsterTarget = target;

            var targetName = target != null ? target.Data.CardName : "le joueur";
            Core.GameLog.Info("MonsterAI", $"{card.CardName} annonce une attaque contre {targetName} — le joueur peut bloquer");
        }

        private static void CheckCombatEnd(CombatContext ctx)
        {
            if (ctx.PlayerHP  <= 0) ctx.Result = CombatResult.PlayerLose;
            if (ctx.MonsterHP <= 0) ctx.Result = CombatResult.PlayerWin;
        }
    }
}
