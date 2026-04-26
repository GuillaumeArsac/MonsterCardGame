using MonsterCardGame.Core.Services;
using MonsterCardGame.Gameplay.Inventory;
using MonsterCardGame.Gameplay.World;

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
                    ResolveLoot(ctx);
                    RecordVictory(ctx);
                    Services.Get<ISaveService>()?.Save();
                    Core.GameLog.Info("CombatEndState", $"VICTOIRE — {ctx.DroppedMaterials.Count} matériau(x) obtenu(s)");
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

        private static void ResolveLoot(CombatContext ctx)
        {
            var drops     = LootResolver.Resolve(ctx.MonsterData);
            var inventory = Services.Get<IPlayerInventory>();

            foreach (var mat in drops)
            {
                ctx.DroppedMaterials.Add(mat);
                inventory?.AddMaterial(mat);
            }
        }

        private static void RecordVictory(CombatContext ctx)
        {
            Services.Get<IWorldProgressionService>()?.RecordVictory(ctx.MonsterData);
        }
    }
}
