namespace MonsterCardGame.Gameplay.Combat
{
    public enum ModifierKind
    {
        DamageBonus,
        DamageReduction,
        /// <summary>Provocation temporaire (Essence de pierre).</summary>
        Taunt,
        /// <summary>Ne peut pas être choisi par les compétences adverses (Essence de l'ombre).</summary>
        Untargetable
    }

    /// <summary>
    /// Effet temporaire sur un monstre (Bénédiction, Bouclier, Provocation temporaire, inciblable).
    /// Un effet de X tours dure jusqu'à la fin de la X-ième phase de l'autre camp que celui qui l'applique.
    /// Référence : docs/competences.md (Durées).
    /// </summary>
    public class CombatModifier
    {
        public ModifierKind Kind           { get; }
        public int          Amount         { get; }
        /// <summary>Camp dont les fins de phase font avancer la durée : l'adversaire de celui qui applique l'effet.</summary>
        public Side         CountdownSide  { get; }
        public bool         IsPermanent    { get; }
        /// <summary>Fins de phase de CountdownSide restantes avant la fin de l'effet.</summary>
        public int          RemainingPhases { get; private set; }

        public bool IsExpired => !IsPermanent && RemainingPhases <= 0;

        public CombatModifier(ModifierKind kind, int amount, Side appliedBy, int durationTurns)
        {
            Kind            = kind;
            Amount          = amount;
            CountdownSide   = appliedBy.Opponent();
            IsPermanent     = durationTurns <= 0;
            RemainingPhases = durationTurns;
        }

        internal void Tick()
        {
            if (!IsPermanent) RemainingPhases--;
        }
    }
}
