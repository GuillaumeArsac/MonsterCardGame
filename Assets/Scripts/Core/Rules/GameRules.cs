namespace MonsterCardGame.Core
{
    /// <summary>Constantes des règles du jeu. Référence : docs/combat.md et docs/progression.md.</summary>
    public static class GameRules
    {
        // Équipe et terrain
        public const int TeamSize = 5;
        public const int FieldSlots = 3;

        // Victoire
        public const int PointsToWin = 3;

        // Fatigue : à partir de ce tour, dégâts croissants (1, puis 2, puis 3...) au début de chaque tour
        public const int FatigueStartTurn = 16;

        // Monstres
        public const int MaxSkillsPerMonster = 3;
    }
}
