using System;
using System.Collections.Generic;
using UnityEngine;
using MonsterCardGame.Gameplay.Encounters;
using MonsterCardGame.Gameplay.Monsters;
using MonsterCardGame.Gameplay.Skills;
using MonsterCardGame.Gameplay.Skills.Effects;

namespace MonsterCardGame.Tests.EditMode
{
    /// <summary>
    /// Compétences, monstres et combats des docs, créés en mémoire pour les tests.
    /// Références : docs/competences.md, docs/monstres.md, docs/combats-scenario.md.
    /// Appeler Dispose() dans le TearDown pour détruire les ScriptableObjects créés.
    /// </summary>
    public sealed class TestCatalog : IDisposable
    {
        private readonly List<ScriptableObject> _created = new();
        private readonly Dictionary<string, SkillData> _skills = new();

        // ── Compétences (docs/competences.md) ─────────────────────────────

        public SkillData GrandBalayage => Skill("Grand balayage", () => SkillData.CreateActive(
            "Grand balayage", MonsterRole.Tank, SkillTarget.AllEnemies, 3, new DamageEffect(10), new SlowEffect()));

        public SkillData Griffe => Skill("Griffe", () => SkillData.CreateActive(
            "Griffe", MonsterRole.Carry, SkillTarget.AnyMonster, 0, new DamageEffect(30)));

        public SkillData Soin => Skill("Soin", () => SkillData.CreateActive(
            "Soin", MonsterRole.Support, SkillTarget.AnyMonster, 2, new HealEffect(40)));

        public SkillData Benediction => Skill("Bénédiction", () => SkillData.CreateActive(
            "Bénédiction", MonsterRole.Support, SkillTarget.AllAllies, 3, new DamageBonusEffect(10, 6)));

        public SkillData MorsurePerforante => Skill("Morsure perforante", () => SkillData.CreateActive(
            "Morsure perforante", MonsterRole.Carry, SkillTarget.AnyMonster, 0, new DamageEffect(20, ignoresReductions: true)));

        public SkillData Execution => Skill("Exécution", () => SkillData.CreateActive(
            "Exécution", MonsterRole.Carry, SkillTarget.AnyMonster, 4, new ExecuteEffect(80, 10)));

        public SkillData ChasseurDeTrophees => Skill("Chasseur de trophées", () => SkillData.CreatePassive(
            "Chasseur de trophées", MonsterRole.Carry, SkillTarget.None, PassiveTrigger.StartOfCombat, new MarkedSlotEffect(1)));

        public SkillData Bouclier => Skill("Bouclier", () => SkillData.CreateActive(
            "Bouclier", MonsterRole.Support, SkillTarget.AnyMonster, 3, new DamageReductionEffect(20, 2)));

        public SkillData AuraApaisante => Skill("Aura apaisante", () => SkillData.CreatePassive(
            "Aura apaisante", MonsterRole.Support, SkillTarget.AllAllies, PassiveTrigger.StartOfOwnPhase, new HealEffect(10)));

        public SkillData Repli => Skill("Repli", () => SkillData.CreateActive(
            "Repli", MonsterRole.Support, SkillTarget.OtherAlly, 4, new ReturnToHandEffect()));

        public SkillData Provocation => Skill("Provocation", () => SkillData.CreatePassive(
            "Provocation", MonsterRole.Tank, SkillTarget.Self, PassiveTrigger.Always, new TauntEffect()));

        public SkillData Carapace => Skill("Carapace", () => SkillData.CreatePassive(
            "Carapace", MonsterRole.Tank, SkillTarget.Self, PassiveTrigger.Always, new DamageReductionEffect(10, 0)));

        public SkillData CoupDeBouclier => Skill("Coup de bouclier", () => SkillData.CreateActive(
            "Coup de bouclier", MonsterRole.Tank, SkillTarget.AnyMonster, 3, new DamageEffect(10), new SlowEffect()));

        public IEnumerable<SkillData> AllSkills => new[]
        {
            GrandBalayage, Griffe, Soin, Benediction, MorsurePerforante, Execution, ChasseurDeTrophees,
            Bouclier, AuraApaisante, Repli, Provocation, Carapace, CoupDeBouclier
        };

        // ── Monstres et combats ───────────────────────────────────────────

        public MonsterCardData Monster(string name, MonsterRole role, int maxHp, int boostHp, params SkillData[] skills)
            => Track(MonsterCardData.Create(name, role, maxHp, new StatBoost(boostHp), skills));

        /// <summary>Monstre de quête : sa première compétence est unique.</summary>
        public MonsterCardData QuestMonster(string name, MonsterRole role, int maxHp, params SkillData[] skills)
        {
            var card = Track(ScriptableObject.CreateInstance<MonsterCardData>());
            card.Init(name, role, maxHp, skills, uniqueSkill: skills[0]);
            return card;
        }

        /// <summary>Combat 1, la patrouille centaure (docs/combats-scenario.md).</summary>
        public EncounterData PatrouilleCentaure()
        {
            var encounter = Track(ScriptableObject.CreateInstance<EncounterData>());
            encounter.Init("plaines-patrouille-centaure", "La patrouille centaure", EncounterType.Scenario, new[]
            {
                Monster("Éclaireur centaure", MonsterRole.Carry,   140, 20, Griffe),
                Monster("Archer novice",      MonsterRole.Carry,   120, 20, MorsurePerforante),
                Monster("Jeune taureau",      MonsterRole.Tank,    200, 20, Provocation, Carapace),
                Monster("Porte-étendard",     MonsterRole.Support, 140, 20, AuraApaisante),
                Monster("Cavalier rabatteur", MonsterRole.Carry,   160, 20, Griffe, CoupDeBouclier),
            });
            return encounter;
        }

        // ── Équipes de test (docs/monstres.md, tools/simulation) ──────────

        /// <summary>Équipe A, équipe de test du joueur.</summary>
        public MonsterCardData[] TeamA() => new[]
        {
            Monster("Griffon",          MonsterRole.Carry,   160, 0, Griffe, Execution),
            Monster("Archer centaure",  MonsterRole.Carry,   140, 0, MorsurePerforante, ChasseurDeTrophees),
            Monster("Orbe de lumière",  MonsterRole.Support, 180, 0, Soin, Benediction, AuraApaisante),
            Monster("Svathurim",        MonsterRole.Support, 200, 0, Griffe, Bouclier, Repli),
            Monster("Paladin centaure", MonsterRole.Tank,    280, 0, Provocation, Carapace, CoupDeBouclier),
        };

        /// <summary>Équipe B, la horde de Thrâk.</summary>
        public MonsterCardData[] TeamB() => new[]
        {
            Monster("Thrâk le Perpétuel",  MonsterRole.Tank,    260, 0, Provocation, Griffe, GrandBalayage),
            Monster("Aigle Runique Géant", MonsterRole.Carry,   180, 0, Griffe, Execution, ChasseurDeTrophees),
            Monster("Guerrier centaure",   MonsterRole.Carry,   160, 0, MorsurePerforante, CoupDeBouclier),
            Monster("Chaman centaure",     MonsterRole.Support, 160, 0, Griffe, Soin, Benediction),
            Monster("Taureau de guerre",   MonsterRole.Tank,    300, 0, Carapace, Griffe, CoupDeBouclier),
        };

        /// <summary>Meilleure équipe de 5 de la collection de départ (clé « Départ » du simulateur).</summary>
        public MonsterCardData[] TeamStart() => new[]
        {
            Monster("Loup des cendres",     MonsterRole.Carry,   150, 0, Griffe),
            Monster("Renard de braise",     MonsterRole.Carry,   140, 0, MorsurePerforante),
            Monster("Ours brun",            MonsterRole.Carry,   200, 0, Griffe),
            Monster("Chouette guérisseuse", MonsterRole.Support, 160, 0, Soin),
            Monster("Chien de garde",       MonsterRole.Tank,    260, 0, Provocation),
        };

        public void Dispose()
        {
            foreach (var obj in _created)
                if (obj != null) UnityEngine.Object.DestroyImmediate(obj);
            _created.Clear();
            _skills.Clear();
        }

        private SkillData Skill(string key, Func<SkillData> create)
        {
            if (!_skills.TryGetValue(key, out var skill))
                _skills[key] = skill = Track(create());
            return skill;
        }

        /// <summary>Garde un ScriptableObject créé par un test pour le détruire dans Dispose().</summary>
        public T Track<T>(T obj) where T : ScriptableObject
        {
            _created.Add(obj);
            return obj;
        }
    }
}
