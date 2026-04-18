namespace MonsterCardGame.Gameplay.Combat.States
{
    /// <summary>Affiche victoire ou défaite. État terminal — aucune transition sortante.</summary>
    public class CombatEndState : ICombatState
    {
        public void Enter(CombatContext ctx)
        {
            switch (ctx.Result)
            {
                case CombatResult.PlayerWin:
                    Core.GameLog.Info("CombatEndState", "VICTOIRE");
                    break;
                case CombatResult.PlayerLose:
                    Core.GameLog.Info("CombatEndState", "DÉFAITE");
                    break;
                default:
                    Core.GameLog.Warning("CombatEndState", "CombatEndState atteint sans résultat défini");
                    break;
            }
        }

        public void Update(CombatContext ctx) { }
        public void Exit(CombatContext ctx) { }
    }
}
