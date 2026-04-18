using System;
using MonsterCardGame.Gameplay.Cards;

namespace MonsterCardGame.Gameplay.Combat.States
{
    /// <summary>
    /// Optionnel : le joueur peut sacrifier 1 ou plusieurs cartes de sa main (→ bas du deck, +1 mana).
    /// Passer sans sacrifier → PlayState directement.
    /// </summary>
    public class SacrificeState : ICombatState
    {
        private readonly Action<ICombatState> _transitionTo;
        private readonly ICombatState _play;

        public SacrificeState(Action<ICombatState> transitionTo, ICombatState play)
        {
            _transitionTo = transitionTo;
            _play = play;
        }

        public void Enter(CombatContext ctx)
        {
            Core.GameLog.Info("SacrificeState", "En attente de sacrifice ou de passage");
        }

        public void Update(CombatContext ctx)
        { }

        public void Exit(CombatContext ctx)
        { }

        /// <summary>Appelé par l'UI quand le joueur choisit une carte à sacrifier.</summary>
        public void TrySacrifice(CombatContext ctx, CardData card)
        {
            if (!ctx.PlayerHand.Remove(card))
            {
                Core.GameLog.Warning("SacrificeState", $"Carte {card.CardName} introuvable en main");
                return;
            }

            ctx.PlayerMana += card.ManaGenerated;
            ctx.PlayerDeck.Add(card); // bas du deck

            Core.GameLog.Info("SacrificeState", $"Sacrifice : {card.CardName} → bas du deck. Mana : {ctx.PlayerMana}");
        }

        /// <summary>Appelé par l'UI quand le joueur passe la phase de sacrifice.</summary>
        public void Skip(CombatContext ctx) => _transitionTo(_play);
    }
}