namespace MonsterCardGame.Core
{
    public static class GameRules
    {
        // Construction du deck
        public const int DeckSize = 40;

        public const int MaxDeckWeight = 15;
        public const int MaxHandSize = 10;
        public const int StartOfTurnHandSize = 4;
        public const int MaxCopiesCommon = 3;
        public const int MaxCopiesRare = 3;
        public const int MaxCopiesLegendary = 1;
        public const int MaxCopiesUnique = 1;

        // Poids par rareté
        public const int WeightCommon = 0;

        public const int WeightRare = 1;
        public const int WeightLegendary = 2;
        public const int WeightUnique = 2;

        // Combat — placeholder à confirmer lors du balance Epic 2
        public const int PlayerStartingHP = 30;
    }
}