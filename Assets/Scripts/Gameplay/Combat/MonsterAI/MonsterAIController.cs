using System;
using MonsterCardGame.Core.Services;
using MonsterCardGame.Gameplay.Cards;
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

            if (ctx.MonsterDeck.Count == 0)
            {
                Core.GameLog.Info("MonsterAI", "Deck monstre vide — fin du tour monstre");
                onComplete(ctx, false);
                return;
            }

            var card = ctx.MonsterDeck[0];
            ctx.MonsterDeck.RemoveAt(0);

            ExecuteCard(ctx, card);
            CheckCombatEnd(ctx);

            onComplete(ctx, false);
        }

        private void ExecuteCard(CombatContext ctx, CardData card)
        {
            Core.GameLog.Info("MonsterAI", $"Monstre joue : {card.CardName}");

            switch (card.CardType)
            {
                case CardType.Allie:
                    ctx.MonsterAllies.Add(new AlliedInstance(card));
                    break;

                case CardType.Action:
                    AnnounceAction(ctx, card);
                    ctx.MonsterDeck.Add(card);
                    break;

                default:
                    Core.GameLog.Info("MonsterAI", $"{card.CardName} (type {card.CardType}) — ignorée pour MVP");
                    break;
            }
        }

        private void AnnounceAction(CombatContext ctx, CardData card)
        {
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
