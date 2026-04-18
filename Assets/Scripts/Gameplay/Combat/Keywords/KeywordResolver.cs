using System.Collections.Generic;
using System.Linq;
using MonsterCardGame.Gameplay.Cards;

namespace MonsterCardGame.Gameplay.Combat.Keywords
{
    public class KeywordResolver : IKeywordResolver
    {
        public bool CanTarget(AlliedInstance attacker, AlliedInstance target)
        {
            // Les équipements ne peuvent pas être ciblés
            if (target.Data.CardType == CardType.Equipement)
                return false;

            bool targetHasVol = target.Data.HasKeyword(Keyword.Vol);
            bool attackerHasVol = attacker.Data.HasKeyword(Keyword.Vol);
            bool attackerHasPortee = attacker.Data.HasKeyword(Keyword.Portee);

            // Un allié au sol ne peut pas cibler un allié Vol adverse sauf s'il a Vol ou Portée
            if (targetHasVol && !attackerHasVol && !attackerHasPortee)
                return false;

            return true;
        }

        public AlliedInstance GetPriorityTarget(IReadOnlyList<AlliedInstance> allies)
        {
            if (allies == null || allies.Count == 0) return null;

            // Provocation : cible prioritaire (jamais un équipement)
            foreach (var ally in allies)
                if (ally.Data.HasKeyword(Keyword.Provocation) && ally.Data.CardType != CardType.Equipement)
                    return ally;

            // Sinon premier allié non-équipement
            return allies.FirstOrDefault(a => a.Data.CardType != CardType.Equipement);
        }

        public bool CanBeBlocked(AlliedInstance source)
            => !source.Data.HasKeyword(Keyword.Instantane);

        public bool IsEveille(AlliedInstance ally)
            => ally.Data.HasKeyword(Keyword.Eveille);

        public bool IsRampant(AlliedInstance ally)
            => ally.Data.HasKeyword(Keyword.Rampant);

        public int GetRitualCost(CardData card)
        {
            if (!card.HasKeyword(Keyword.Rituel)) return 0;
            return card.RitualValue;
        }
    }
}