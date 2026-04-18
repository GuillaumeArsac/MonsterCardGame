using System.Collections.Generic;
using MonsterCardGame.Core.Services;
using MonsterCardGame.Gameplay.Cards;

namespace MonsterCardGame.Gameplay.Combat.Keywords
{
    public interface IKeywordResolver : IService
    {
        /// <summary>L'attaquant peut-il cibler la cible ? Respecte Vol et Portée.</summary>
        bool CanTarget(AlliedInstance attacker, AlliedInstance target);

        /// <summary>Retourne la cible prioritaire parmi les alliés adverses (Provocation en premier).</summary>
        AlliedInstance GetPriorityTarget(IReadOnlyList<AlliedInstance> allies);

        /// <summary>La source peut-elle être bloquée ? Les Instantané ne peuvent pas.</summary>
        bool CanBeBlocked(AlliedInstance source);

        /// <summary>La carte est-elle Éveillée (peut agir dès sa mise en jeu) ?</summary>
        bool IsEveille(AlliedInstance ally);

        /// <summary>La carte est-elle Rampante ?</summary>
        bool IsRampant(AlliedInstance ally);

        /// <summary>Retourne le coût Rituel(X) de la carte, ou 0 si absent.</summary>
        int GetRitualCost(CardData card);
    }
}
