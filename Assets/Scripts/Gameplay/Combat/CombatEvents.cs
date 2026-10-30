using System.Collections.Generic;
using MonsterCardGame.Gameplay.Skills;

namespace MonsterCardGame.Gameplay.Combat
{
    // Événements du moteur de combat, écoutés par l'interface.

    public enum CombatEndReason
    {
        /// <summary>Le vainqueur a atteint 3 points.</summary>
        Points,
        /// <summary>Le perdant n'a plus aucun monstre, ni en main ni sur le terrain.</summary>
        NoMonstersLeft,
        /// <summary>Les deux camps ont perdu leur dernier monstre en même temps : le joueur perd.</summary>
        BothSidesEmpty,
        /// <summary>Les deux camps ont atteint 3 points en même temps (fatigue) : le joueur perd.</summary>
        BothReachedPoints
    }

    public readonly struct MonsterPlacedEvent
    {
        public readonly CombatMonster Monster;
        public readonly int           Slot;
        public readonly bool          DuringSetup;

        public MonsterPlacedEvent(CombatMonster monster, int slot, bool duringSetup)
        {
            Monster     = monster;
            Slot        = slot;
            DuringSetup = duringSetup;
        }
    }

    /// <summary>Monstre invoqué sur le terrain par une compétence (insectes de Vaelisse).</summary>
    public readonly struct MonsterSummonedEvent
    {
        public readonly CombatMonster Monster;
        public readonly CombatMonster Summoner;
        public readonly int           Slot;

        public MonsterSummonedEvent(CombatMonster monster, CombatMonster summoner, int slot)
        {
            Monster  = monster;
            Summoner = summoner;
            Slot     = slot;
        }
    }

    /// <summary>Monstre invoqué renforcé, faute d'emplacement libre pour une nouvelle invocation.</summary>
    public readonly struct ReinforcedEvent
    {
        public readonly CombatMonster Monster;
        public readonly CombatMonster Summoner;
        public readonly int           Hp;
        public readonly int           Damage;

        public ReinforcedEvent(CombatMonster monster, CombatMonster summoner, int hp, int damage)
        {
            Monster  = monster;
            Summoner = summoner;
            Hp       = hp;
            Damage   = damage;
        }
    }

    /// <summary>Compétence active utilisée, ou passive déclenchée (Aura apaisante).</summary>
    public readonly struct SkillUsedEvent
    {
        public readonly CombatMonster                Monster;
        public readonly int                          SkillIndex;
        public readonly SkillData                    Skill;
        public readonly IReadOnlyList<CombatMonster> Targets;

        public SkillUsedEvent(CombatMonster monster, int skillIndex, SkillData skill, IReadOnlyList<CombatMonster> targets)
        {
            Monster    = monster;
            SkillIndex = skillIndex;
            Skill      = skill;
            Targets    = targets;
        }
    }

    public readonly struct DamageEvent
    {
        public readonly CombatMonster Target;
        /// <summary>Null pour la fatigue et le saignement.</summary>
        public readonly CombatMonster Source;
        public readonly int           Amount;
        public readonly int           HpAfter;
        public readonly bool          IsFatigue;
        public readonly bool          IsBleed;

        public DamageEvent(CombatMonster target, CombatMonster source, int amount, int hpAfter, bool isFatigue, bool isBleed = false)
        {
            IsBleed   = isBleed;
            Target    = target;
            Source    = source;
            Amount    = amount;
            HpAfter   = hpAfter;
            IsFatigue = isFatigue;
        }
    }

    public readonly struct HealEvent
    {
        public readonly CombatMonster Target;
        public readonly CombatMonster Source;
        /// <summary>PV réellement rendus, après le plafond des PV de départ.</summary>
        public readonly int           Amount;
        public readonly int           HpAfter;

        public HealEvent(CombatMonster target, CombatMonster source, int amount, int hpAfter)
        {
            Target  = target;
            Source  = source;
            Amount  = amount;
            HpAfter = hpAfter;
        }
    }

    /// <summary>Saignements appliqués à un monstre, ou retirés par un soin (Added = 0).</summary>
    public readonly struct BleedEvent
    {
        public readonly CombatMonster Target;
        public readonly CombatMonster Source;
        /// <summary>Cumuls ajoutés. 0 quand un soin retire les saignements.</summary>
        public readonly int           Added;
        /// <summary>Cumuls après l'effet.</summary>
        public readonly int           Total;

        public bool IsCleansed => Added == 0;

        public BleedEvent(CombatMonster target, CombatMonster source, int added, int total)
        {
            Target = target;
            Source = source;
            Added  = added;
            Total  = total;
        }
    }

    public readonly struct ModifierAppliedEvent
    {
        public readonly CombatMonster  Target;
        public readonly CombatMonster  Source;
        public readonly CombatModifier Modifier;

        public ModifierAppliedEvent(CombatMonster target, CombatMonster source, CombatModifier modifier)
        {
            Target   = target;
            Source   = source;
            Modifier = modifier;
        }
    }

    public readonly struct MonsterDefeatedEvent
    {
        public readonly CombatMonster Monster;
        /// <summary>Emplacement qu'occupait le monstre.</summary>
        public readonly int           Slot;
        /// <summary>Points marqués par l'adversaire, emplacement marqué compris, fatigue comprise.</summary>
        public readonly int           PointsAwarded;
        public readonly bool          ByFatigue;

        public MonsterDefeatedEvent(CombatMonster monster, int slot, int pointsAwarded, bool byFatigue)
        {
            Monster       = monster;
            Slot          = slot;
            PointsAwarded = pointsAwarded;
            ByFatigue     = byFatigue;
        }
    }

    public readonly struct PhaseEvent
    {
        public readonly Side Side;
        public readonly int  Turn;

        public PhaseEvent(Side side, int turn)
        {
            Side = side;
            Turn = turn;
        }
    }

    public readonly struct CombatEndedEvent
    {
        public readonly Side            Winner;
        public readonly CombatEndReason Reason;

        public CombatEndedEvent(Side winner, CombatEndReason reason)
        {
            Winner = winner;
            Reason = reason;
        }
    }
}
