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
        BothSidesEmpty
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
        /// <summary>Null pour la fatigue.</summary>
        public readonly CombatMonster Source;
        public readonly int           Amount;
        public readonly int           HpAfter;
        public readonly bool          IsFatigue;

        public DamageEvent(CombatMonster target, CombatMonster source, int amount, int hpAfter, bool isFatigue)
        {
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
        /// <summary>Points marqués par l'adversaire, emplacement marqué compris. 0 pour la fatigue.</summary>
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
