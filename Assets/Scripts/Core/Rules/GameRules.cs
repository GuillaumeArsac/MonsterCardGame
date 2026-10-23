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

        // Fatigue : à partir de ce tour, dégâts croissants (10, puis 20, puis 30...) au début de chaque tour
        public const int FatigueStartTurn = 16;
        public const int FatigueDamageStep = 10;

        // Saignement : dégâts par cumul au début de chaque phase du camp du monstre, en % de ses PV max (arrondi à l'entier inférieur, 1 minimum)
        public const int BleedPercentPerStack = 5;

        // Monstres
        public const int MaxSkillsPerMonster = 3;
    }
}
