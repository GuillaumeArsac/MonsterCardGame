using System;
using MonsterCardGame.Core.Services;
using MonsterCardGame.Gameplay.Monsters;

namespace MonsterCardGame.Gameplay.Encounters
{
    /// <summary>
    /// Le combat en cours, d'une scène à l'autre : le combat à lancer, puis son résultat et la récompense du dépeçage.
    /// La scène de combat le lit au démarrage ; sans session, elle utilise son combat de test.
    /// </summary>
    public class CombatSession : IService
    {
        public EncounterData Encounter    { get; private set; }
        /// <summary>Scène où revenir après le combat (et le dépeçage).</summary>
        public string        ReturnScene  { get; private set; }
        public bool?         Victory      { get; private set; }
        /// <summary>Récompense du dépeçage, tirée une seule fois à la victoire.</summary>
        public SkinningOffer Offer        { get; private set; }

        public bool IsActive   => Encounter != null;
        public bool IsFinished => Victory.HasValue;

        public void Begin(EncounterData encounter, string returnScene)
        {
            Encounter   = encounter ?? throw new ArgumentNullException(nameof(encounter));
            ReturnScene = returnScene;
            Victory     = null;
            Offer       = null;
        }

        /// <summary>Enregistre le résultat. Une victoire tire le monstre à dépecer.</summary>
        public void Complete(bool victory, Random random)
        {
            if (!IsActive) throw new InvalidOperationException("Aucun combat en cours");
            if (IsFinished) return;

            Victory = victory;
            if (victory) Offer = SkinningOffer.Draw(Encounter, random);
        }

        public void Clear()
        {
            Encounter   = null;
            ReturnScene = null;
            Victory     = null;
            Offer       = null;
        }
    }
}
