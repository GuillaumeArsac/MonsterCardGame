using MonsterCardGame.Gameplay.Combat.Data;

namespace MonsterCardGame.Gameplay.World
{
    public class CombatSetupService : ICombatSetupService
    {
        public ZoneData    PendingZone    { get; private set; }
        public MonsterData PendingMonster { get; private set; }

        public void Setup(ZoneData zone, MonsterData monster)
        {
            PendingZone    = zone;
            PendingMonster = monster;
            Core.GameLog.Info("CombatSetup", $"Combat préparé : {monster?.MonsterName} ({zone?.ZoneName})");
        }
    }
}
