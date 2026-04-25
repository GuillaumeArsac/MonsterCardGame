namespace MonsterCardGame.Gameplay.Combat
{
    public class CombatTarget
    {
        public AlliedInstance Ally      { get; }
        public bool           IsMonster { get; }

        private CombatTarget(AlliedInstance ally, bool isMonster)
        {
            Ally      = ally;
            IsMonster = isMonster;
        }

        public static readonly CombatTarget Monster = new(null, true);
        public static CombatTarget ForAlly(AlliedInstance ally) => new(ally, false);
    }
}
