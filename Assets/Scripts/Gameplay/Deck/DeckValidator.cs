using System.Collections.Generic;
using MonsterCardGame.Core;
using MonsterCardGame.Gameplay.Cards;

namespace MonsterCardGame.Gameplay.Deck
{
    public static class DeckValidator
    {
        /// <summary>Valide les 3 règles de construction : taille exacte, poids max, copies max.</summary>
        public static bool Validate(DeckData deck)
        {
            if (deck == null)
            {
                GameLog.Error("DeckValidator", "Deck null passé à Validate");
                return false;
            }
            return HasCorrectSize(deck)
                && IsWithinWeightLimit(deck)
                && RespectsCopyLimits(deck);
        }

        public static bool HasCorrectSize(DeckData deck)
            => deck.Cards.Count == GameRules.DeckSize;

        public static bool IsWithinWeightLimit(DeckData deck)
            => GetTotalWeight(deck) <= GameRules.MaxDeckWeight;

        public static bool RespectsCopyLimits(DeckData deck)
        {
            var copyCount = new Dictionary<CardData, int>();
            foreach (var card in deck.Cards)
            {
                if (card == null) continue;
                copyCount.TryGetValue(card, out int count);
                copyCount[card] = count + 1;
            }
            foreach (var (card, count) in copyCount)
            {
                if (count > GetMaxCopies(card)) return false;
            }
            return true;
        }

        public static int GetTotalWeight(DeckData deck)
        {
            int total = 0;
            foreach (var card in deck.Cards)
                if (card != null) total += card.Weight;
            return total;
        }

        public static int GetCardCopies(DeckData deck, CardData card)
        {
            int count = 0;
            foreach (var c in deck.Cards)
                if (c == card) count++;
            return count;
        }

        private static int GetMaxCopies(CardData card) => card.Rarity switch
        {
            CardRarity.Commune    => GameRules.MaxCopiesCommon,
            CardRarity.Rare       => GameRules.MaxCopiesRare,
            CardRarity.Legendaire => GameRules.MaxCopiesLegendary,
            CardRarity.Unique     => GameRules.MaxCopiesUnique,
            _                     => GameRules.MaxCopiesCommon
        };
    }
}
