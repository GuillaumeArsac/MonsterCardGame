using System;
using MonsterCardGame.Core.Services;
using MonsterCardGame.Gameplay.Cards;
using MonsterCardGame.Gameplay.Cards.Effects;
using MonsterCardGame.Gameplay.Combat.Keywords;

namespace MonsterCardGame.Gameplay.Combat.States
{
    /// <summary>
    /// Le joueur joue des cartes depuis sa main (drag &amp; drop).
    /// Fin de phase → ReactiveWindowState.
    /// </summary>
    public class PlayState : ICombatState
    {
        private readonly Action<ICombatState> _transitionTo;
        private readonly ICombatState _monsterTurn;
        private IKeywordResolver _resolver;

        public PlayState(Action<ICombatState> transitionTo, ICombatState monsterTurn)
        {
            _transitionTo = transitionTo;
            _monsterTurn = monsterTurn;
        }

        public void Enter(CombatContext ctx)
        {
            _resolver = Services.Get<IKeywordResolver>();
            Core.GameLog.Info("PlayState", "Phase de jeu — en attente d'actions joueur");
        }

        public void Update(CombatContext ctx)
        { }

        public void Exit(CombatContext ctx)
        { }

        /// <summary>Tente de jouer une carte depuis la main. Retourne true si réussi.</summary>
        public bool TryPlayCard(CombatContext ctx, CardData card)
        {
            if (!ctx.PlayerHand.Contains(card))
            {
                Core.GameLog.Warning("PlayState", $"{card.CardName} introuvable en main");
                return false;
            }
            if (ctx.PlayerMana < card.ManaCost)
            {
                Core.GameLog.Warning("PlayState", $"Mana insuffisant pour {card.CardName} (coût {card.ManaCost}, dispo {ctx.PlayerMana})");
                return false;
            }

            ctx.PlayerMana -= card.ManaCost;
            ctx.PlayerHand.Remove(card);

            switch (card.CardType)
            {
                case CardType.Allie:
                    var ally = new AlliedInstance(card);
                    ctx.PlayerAllies.Add(ally);
                    Core.GameLog.Info("PlayState", $"Allié joué : {card.CardName} (Éveillé={ally.Data.HasKeyword(Keyword.Eveille)})");
                    break;

                case CardType.Action:
                    ctx.MonsterHP -= card.Attack;
                    ctx.PlayerDeck.Add(card); // bas du deck
                    Core.GameLog.Info("PlayState", $"Carte jouée : {card.CardName} → bas du deck");
                    break;

                case CardType.Blocage:
                    ctx.PlayerDeck.Add(card); // bas du deck
                    Core.GameLog.Info("PlayState", $"Carte jouée : {card.CardName} → bas du deck");
                    break;

                case CardType.Equipement:
                    var equipment = new AlliedInstance(card);
                    equipment.SetSleeping(false);
                    ctx.PlayerAllies.Add(equipment);
                    Core.GameLog.Info("PlayState", $"Équipement joué : {card.CardName} → terrain");
                    break;
            }

            foreach (var effect in card.OnPlayEffects)
                effect.Apply(new CardEffectContext(ctx));

            CheckCombatEnd(ctx);
            return true;
        }

        /// <summary>
        /// Attaque un allié monstre avec un allié joueur.
        /// Les deux s'infligent mutuellement leurs dégâts d'ATK.
        /// </summary>
        public bool TryAttackWithAlly(CombatContext ctx, AlliedInstance attacker, AlliedInstance target)
        {
            if (!ValidateAttacker(ctx, attacker)) return false;

            if (!_resolver.CanTarget(attacker, target))
            {
                Core.GameLog.Warning("PlayState", $"{attacker.Data.CardName} ne peut pas cibler {target.Data.CardName} (Vol non atteignable)");
                return false;
            }

            // Player Ally stats
            int atkDmg = attacker.ATK;
            int atkDef = attacker.DEF;

            // Monster Ally stats
            int tgtDmg = target.ATK;
            int tgtDef = target.DEF;

            Core.GameLog.Info("PlayState", $"{attacker.Data.CardName} ({atkDmg}/{atkDef} ATK/DEF) ↔ {target.Data.CardName} ({tgtDmg}/{tgtDef} ATK/DEF)");

            if (atkDmg >= tgtDef)
                CombatHelper.DestroyAlly(ctx, ctx.MonsterAllies, ctx.MonsterCemetery, target, isPlayer: false);

            if (tgtDmg >= atkDef && attacker.Data.CardType == CardType.Allie)
                CombatHelper.DestroyAlly(ctx, ctx.PlayerAllies, ctx.PlayerCemetery, attacker, isPlayer: true);
            else if (attacker.Data.CardType == CardType.Equipement && attacker.SpendCharge())
                CombatHelper.DestroyAlly(ctx, ctx.PlayerAllies, ctx.PlayerCemetery, attacker, isPlayer: true);
            else if (attacker.Data.CardType != CardType.Equipement)
                attacker.SetSleeping(true);

            foreach (var effect in attacker.Data.OnAttackEffects)
                effect.Apply(new CardEffectContext(ctx, attacker));

            CheckCombatEnd(ctx);
            return true;
        }

        /// <summary>
        /// Attaque le monstre directement.
        /// Impossible si un allié monstre avec Provocation est présent.
        /// </summary>
        public bool TryAttackMonsterDirectly(CombatContext ctx, AlliedInstance attacker)
        {
            if (!ValidateAttacker(ctx, attacker)) return false;

            // Provocation : doit attaquer l'allié prioritaire en premier
            var blocker = _resolver.GetPriorityTarget(ctx.MonsterAllies);
            if (blocker != null && blocker.Data.HasKeyword(Keyword.Provocation))
            {
                Core.GameLog.Warning("PlayState",
                    $"Impossible d'attaquer directement — {blocker.Data.CardName} a Provocation");
                return false;
            }

            ctx.MonsterHP -= attacker.ATK;
            if (attacker.Data.CardType == CardType.Equipement && attacker.SpendCharge())
                CombatHelper.DestroyAlly(ctx, ctx.PlayerAllies, ctx.PlayerCemetery, attacker, isPlayer: true);
            else if (attacker.Data.CardType != CardType.Equipement)
                attacker.SetSleeping(true);

            foreach (var effect in attacker.Data.OnAttackEffects)
                effect.Apply(new CardEffectContext(ctx, attacker));

            Core.GameLog.Info("PlayState",
                $"{attacker.Data.CardName} attaque le monstre pour {attacker.ATK} dégâts. PV monstre : {ctx.MonsterHP}");

            CheckCombatEnd(ctx);
            return true;
        }

        /// <summary>Le joueur termine sa phase de jeu → ReactiveWindowState.</summary>
        public void EndPlay(CombatContext ctx)
        {
            // Réveil des alliés endormis pour le prochain tour
            foreach (var a in ctx.PlayerAllies)
                if (a.IsSleeping) a.SetSleeping(false);

            _transitionTo(_monsterTurn);
        }

        // --- Helpers privés ---

        private bool ValidateAttacker(CombatContext ctx, AlliedInstance attacker)
        {
            if (!ctx.PlayerAllies.Contains(attacker))
            {
                Core.GameLog.Warning("PlayState", "Attaquant introuvable sur le terrain");
                return false;
            }
            if (attacker.IsSleeping)
            {
                Core.GameLog.Warning("PlayState",
                    $"{attacker.Data.CardName} est endormi et ne peut pas attaquer ce tour");
                return false;
            }
            return true;
        }

        private static void CheckCombatEnd(CombatContext ctx)
        {
            if (ctx.PlayerHP <= 0) ctx.Result = CombatResult.PlayerLose;
            if (ctx.MonsterHP <= 0) ctx.Result = CombatResult.PlayerWin;
        }
    }
}