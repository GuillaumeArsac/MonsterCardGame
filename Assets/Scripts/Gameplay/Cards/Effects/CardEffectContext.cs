using MonsterCardGame.Gameplay.Combat;

namespace MonsterCardGame.Gameplay.Cards.Effects
{
    /// <summary>Données disponibles pour un effet au moment de son déclenchement.</summary>
    public class CardEffectContext
    {
        public CombatContext  Combat   { get; }
        /// <summary>Allié source de l'effet (attaquant, porteur d'équipement…). Null si déclenchement hors combat.</summary>
        public AlliedInstance Source   { get; }
        /// <summary>True si c'est le joueur qui déclenche l'effet, false si c'est le monstre.</summary>
        public bool           IsPlayer { get; }

        public CardEffectContext(CombatContext combat, AlliedInstance source = null, bool isPlayer = true)
        {
            Combat   = combat;
            Source   = source;
            IsPlayer = isPlayer;
        }
    }
}
