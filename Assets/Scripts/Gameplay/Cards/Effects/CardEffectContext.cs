using MonsterCardGame.Gameplay.Combat;

namespace MonsterCardGame.Gameplay.Cards.Effects
{
    /// <summary>Données disponibles pour un effet au moment de son déclenchement.</summary>
    public class CardEffectContext
    {
        public CombatContext  Combat { get; }
        /// <summary>Allié source de l'effet (attaquant, porteur d'équipement…). Null si déclenchement hors combat.</summary>
        public AlliedInstance Source { get; }

        public CardEffectContext(CombatContext combat, AlliedInstance source = null)
        {
            Combat = combat;
            Source = source;
        }
    }
}
