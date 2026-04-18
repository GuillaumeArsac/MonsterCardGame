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

        public void Enter(CombatContext ctx)
        {
            Core.GameLog.Info("MonsterTurnState", "Tour monstre");
            _ai.ExecuteNextAction(ctx, OnActionComplete);
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

            if (ctx.PendingMonsterAction != null || hasMoreActions)
                _transitionTo(_reactive);
            else
            {
                ctx.Turn++;
                ctx.IsPlayerTurn = true;
                _ai.ResetForNewTurn();
                _transitionTo(_draw);
            }
        }
    }
}
