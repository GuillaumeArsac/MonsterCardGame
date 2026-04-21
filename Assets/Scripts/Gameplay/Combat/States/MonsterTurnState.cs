using System;
using MonsterCardGame.Gameplay.Combat.MonsterAI;

namespace MonsterCardGame.Gameplay.Combat.States
{
    /// <summary>
    /// Tour du monstre. Délègue à MonsterAIController.
    /// Chaque action est séparée par une ReactiveWindowState.
    /// </summary>
    public class MonsterTurnState : ICombatState
    {
        private readonly Action<ICombatState> _transitionTo;
        private readonly ICombatState         _reactive;
        private ICombatState                  _draw;
        private readonly ICombatState         _end;
        private readonly MonsterAIController  _ai;

        public MonsterTurnState(
            Action<ICombatState> transitionTo,
            ICombatState reactive,
            ICombatState draw,
            ICombatState end,
            MonsterAIController ai)
        {
            _transitionTo = transitionTo;
            _reactive     = reactive;
            _draw         = draw;
            _end          = end;
            _ai           = ai;
        }

        public void SetDrawState(ICombatState draw) => _draw = draw;

        private bool _drawDoneThisTurn;

        public void Enter(CombatContext ctx)
        {
            Core.GameLog.Info("MonsterTurnState", "Tour monstre");

            if (!_drawDoneThisTurn)
            {
                DrawMonsterHand(ctx);
                _drawDoneThisTurn = true;
            }

            _ai.ExecuteNextAction(ctx, OnActionComplete);
        }

        private static void DrawMonsterHand(CombatContext ctx)
        {
            foreach (var ally in ctx.MonsterAllies)
                ally.SetSleeping(false);

            int toDraw = Core.GameRules.StartOfTurnHandSize - ctx.MonsterHand.Count;
            for (int i = 0; i < toDraw && ctx.MonsterDeck.Count > 0; i++)
            {
                var card = ctx.MonsterDeck[0];
                ctx.MonsterDeck.RemoveAt(0);
                ctx.MonsterHand.Add(card);
                Core.GameLog.Info("MonsterTurnState", $"Monstre pioche : {card.CardName}");
            }
        }

        public void Update(CombatContext ctx) { }

        public void Exit(CombatContext ctx) { }

        private void OnActionComplete(CombatContext ctx, bool hasMoreActions)
        {
            if (ctx.Result != CombatResult.None)
            {
                _transitionTo(_end);
                return;
            }

            if (ctx.PendingMonsterAction != null)
                _transitionTo(_reactive);
            else if (ctx.MonsterHand.Count > 0)
                _ai.ExecuteNextAction(ctx, OnActionComplete);
            else
            {
                _drawDoneThisTurn = false;
                ctx.Turn++;
                ctx.IsPlayerTurn = true;
                _transitionTo(_draw);
            }
        }
    }
}
