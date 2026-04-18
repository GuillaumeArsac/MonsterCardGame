using System;
using MonsterCardGame.Gameplay.Cards;

namespace MonsterCardGame.Gameplay.Combat.States
{
    /// <summary>
    /// Fenêtre de réponse adverse avant/entre les actions du monstre.
    /// Si PendingMonsterAction != null, le joueur peut jouer un Blocage ou Passer (subir les dégâts).
    /// </summary>
    public class ReactiveWindowState : ICombatState
    {
        private readonly Action<ICombatState> _transitionTo;
        private ICombatState _afterReactive;

        public ReactiveWindowState(Action<ICombatState> transitionTo, ICombatState afterReactive)
        {
            _transitionTo = transitionTo;
            _afterReactive = afterReactive;
        }

        public void SetAfterReactive(ICombatState afterReactive) => _afterReactive = afterReactive;

        public void Enter(CombatContext ctx)
        {
            if (ctx.PendingMonsterAction != null)
            {
                var targetName = ctx.PendingMonsterTarget != null
                    ? ctx.PendingMonsterTarget.Data.CardName
                    : "le joueur";
                Core.GameLog.Info("ReactiveWindowState",
                    $"Le monstre joue {ctx.PendingMonsterAction.CardName} contre {targetName} — Bloquer ou Passer ?");
            }
            else
            {
                Core.GameLog.Info("ReactiveWindowState", "Fenêtre réactive — Passer pour continuer");
            }
        }

        public void Update(CombatContext ctx)
        { }

        public void Exit(CombatContext ctx)
        { }

        /// <summary>Le joueur passe : résout l'action en attente (dégâts appliqués) puis termine le tour monstre.</summary>
        public void Pass(CombatContext ctx)
        {
            ResolvePendingAction(ctx);
            EndMonsterTurn(ctx);
        }

        /// <summary>Le joueur joue une carte Blocage : annule l'action en attente sans dégâts puis termine le tour monstre.</summary>
        public void TryBlock(CombatContext ctx, CardData blockCard)
        {
            if (ctx.PendingMonsterAction == null)
            {
                Core.GameLog.Warning("ReactiveWindowState", "Aucune action monstre à bloquer");
                return;
            }
            if (blockCard.CardType != CardType.Blocage)
            {
                Core.GameLog.Warning("ReactiveWindowState", $"{blockCard.CardName} n'est pas une carte Blocage");
                return;
            }
            if (!ctx.PlayerHand.Remove(blockCard))
            {
                Core.GameLog.Warning("ReactiveWindowState", $"{blockCard.CardName} introuvable en main");
                return;
            }

            ctx.PlayerDeck.Add(blockCard);

            Core.GameLog.Info("ReactiveWindowState",
                $"{blockCard.CardName} bloque {ctx.PendingMonsterAction.CardName} — aucun dégât !");

            ctx.PendingMonsterAction = null;
            ctx.PendingMonsterTarget = null;

            EndMonsterTurn(ctx);
        }

        private void EndMonsterTurn(CombatContext ctx)
        {
            ctx.Turn++;
            ctx.IsPlayerTurn = true;
            _transitionTo(_afterReactive);
        }

        private static void ResolvePendingAction(CombatContext ctx)
        {
            if (ctx.PendingMonsterAction == null) return;

            var action = ctx.PendingMonsterAction;
            var target = ctx.PendingMonsterTarget;

            ctx.PendingMonsterAction = null;
            ctx.PendingMonsterTarget = null;

            if (target != null)
            {
                Core.GameLog.Info("ReactiveWindowState", $"{action.CardName} inflige {action.Attack} dégâts à {target.Data.CardName}");

                if (target.DEF <= action.Attack)
                {
                    ctx.PlayerAllies.Remove(target);
                    ctx.PlayerCemetery.Add(target.Data);
                    Core.GameLog.Info("ReactiveWindowState", $"{target.Data.CardName} est détruit → cimetière");
                }
            }
            else
            {
                ctx.PlayerHP -= action.Attack;
                Core.GameLog.Info("ReactiveWindowState",
                    $"{action.CardName} inflige {action.Attack} dégâts au joueur. PV joueur : {ctx.PlayerHP}");
            }

            if (ctx.PlayerHP <= 0) ctx.Result = CombatResult.PlayerLose;
        }
    }
}