using System;

namespace MonsterCardGame.Gameplay.Combat.States
{
    /// <summary>Pioche 1 carte. Si deck vide → défaite immédiate. Puis → SacrificeState.</summary>
    public class DrawState : ICombatState
    {
        private readonly Action<ICombatState> _transitionTo;
        private readonly ICombatState _sacrifice;

        public DrawState(Action<ICombatState> transitionTo, ICombatState sacrifice)
        {
            _transitionTo = transitionTo;
            _sacrifice = sacrifice;
        }

        public void Enter(CombatContext ctx)
        {
            do
            {
                if (ctx.PlayerDeck.Count == 0)
                {
                    Core.GameLog.Warning("DrawState", "Deck vide — défaite");
                    ctx.Result = CombatResult.PlayerLose;
                    return;
                }

                var card = ctx.PlayerDeck[0];
                ctx.PlayerDeck.RemoveAt(0);

                if (ctx.PlayerHand.Count < Core.GameRules.MaxHandSize)
                    ctx.PlayerHand.Add(card);
                else
                {
                    Core.GameLog.Info("DrawState", $"Main pleine — {card.CardName} envoyée au cimetière");
                    ctx.PlayerCemetery.Add(card);
                }
            } while (ctx.PlayerHand.Count < Core.GameRules.StartOfTurnHandSize);

            // Mana = 0 au début du tour
            ctx.PlayerMana = 0;

            Core.GameLog.Info("DrawState", $"Tour {ctx.Turn}");
        }

        public void Update(CombatContext ctx)
        {
            if (ctx.Result == CombatResult.PlayerLose) return;
            _transitionTo(_sacrifice);
        }

        public void Exit(CombatContext ctx)
        { }
    }
}