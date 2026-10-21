using System;
using System.Collections.Generic;
using NUnit.Framework;
using MonsterCardGame.Gameplay.Combat;
using MonsterCardGame.Gameplay.Skills;
using MonsterCardGame.Gameplay.Skills.Effects;

namespace MonsterCardGame.Tests.EditMode.Combat
{
    /// <summary>Outils communs aux tests du moteur de combat.</summary>
    public abstract class CombatTestBase
    {
        protected TestCatalog Catalog;
        protected CombatEngine Engine;

        [SetUp]    public void BaseSetUp()    => Catalog = new TestCatalog();
        [TearDown] public void BaseTearDown() => Catalog.Dispose();

        protected static CombatMonster Mon(string name, int hp, params SkillData[] skills)
            => new(name, MonsterRole.Carry, hp, skills);

        /// <summary>Compétence active à cible choisie, créée pour un test.</summary>
        protected SkillData Active(string name, int cooldown, params SkillEffect[] effects)
            => Catalog.Track(SkillData.CreateActive(name, MonsterRole.Carry, SkillTarget.AnyMonster, cooldown, effects));

        /// <summary>Crée le combat et fait la mise en place. Le tour 1 commence, phase du joueur.</summary>
        protected CombatEngine Start(IEnumerable<CombatMonster> playerTeam, IEnumerable<CombatMonster> enemyTeam,
                                     IEnumerable<(CombatMonster, int)> playerSetup, IEnumerable<(CombatMonster, int)> enemySetup,
                                     int seed = 1)
        {
            Engine = new CombatEngine(playerTeam, enemyTeam, new Random(seed));
            Engine.Setup(playerSetup, enemySetup);
            return Engine;
        }

        /// <summary>Combat à un monstre de chaque côté, posés sur l'emplacement du centre.</summary>
        protected CombatEngine StartDuel(CombatMonster player, CombatMonster enemy)
            => Start(new[] { player }, new[] { enemy }, new[] { (player, 1) }, new[] { (enemy, 1) });

        protected static int IndexOf(CombatMonster monster, SkillData skill)
        {
            for (int i = 0; i < monster.Skills.Count; i++)
                if (monster.Skills[i] == skill) return i;
            throw new ArgumentException($"{monster.Name} n'a pas {skill.SkillName}");
        }

        protected void Use(CombatMonster monster, SkillData skill, CombatMonster target = null)
            => Engine.UseSkill(monster, IndexOf(monster, skill), target);

        protected bool CanUse(CombatMonster monster, SkillData skill)
            => Engine.CanUseSkill(monster, IndexOf(monster, skill));

        protected void EndPhases(int count)
        {
            for (int i = 0; i < count; i++) Engine.EndPhase();
        }

        /// <summary>Avance jusqu'au début de la phase demandée.</summary>
        protected void AdvanceTo(int turn, Side side)
        {
            while (Engine.State == CombatState.Phase && (Engine.Turn != turn || Engine.CurrentSide != side))
                Engine.EndPhase();

            Assert.That(Engine.Turn, Is.EqualTo(turn), "Le combat s'est terminé avant la phase demandée");
            Assert.That(Engine.CurrentSide, Is.EqualTo(side));
        }
    }
}
