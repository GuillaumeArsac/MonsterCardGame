namespace MonsterCardGame.Gameplay.Combat.States
{
    public interface ICombatState
    {
        void Enter(CombatContext ctx);
        void Update(CombatContext ctx);
        void Exit(CombatContext ctx);
    }
}
