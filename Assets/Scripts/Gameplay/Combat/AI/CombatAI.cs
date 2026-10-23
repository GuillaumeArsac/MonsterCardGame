using System;
using System.Collections.Generic;
using System.Linq;
using MonsterCardGame.Core;
using MonsterCardGame.Gameplay.Skills;
using MonsterCardGame.Gameplay.Skills.Effects;

namespace MonsterCardGame.Gameplay.Combat.AI
{
    /// <summary>
    /// IA adverse, portée du simulateur (tools/simulation/combat_sim.py) : elle choisit le meilleur coup
    /// immédiat sans anticiper. Elle remplit le terrain, puis fait agir les supports, les tanks et les carry,
    /// chacun utilisant ses compétences tant qu'une action a une note positive.
    /// Elle reconnaît les compétences à leurs effets, pas à leur nom.
    /// </summary>
    public class CombatAI
    {
        /// <summary>
        /// Valeur d'un dégât dans les notes. Les PV et les dégâts ont été multipliés par 10 pour un équilibrage plus fin :
        /// les notes divisent les PV et les dégâts par cette unité, pour garder le comportement du simulateur.
        /// </summary>
        private const double Unit = 10;
        /// <summary>En dessous de ces PV, un monstre est en danger (Soin plus urgent, Bouclier plus utile).</summary>
        private const int DangerHp = 80;
        /// <summary>Note d'une action qui vainc la cible, par point marqué.</summary>
        private const double KillScore = 10;
        /// <summary>Bruit ajouté à chaque note, pour varier les parties.</summary>
        private const double Noise = 0.6;

        private readonly Random _random;

        public CombatAI(Random random = null) => _random = random ?? new Random();

        // ── Pose ──────────────────────────────────────────────────────────

        /// <summary>Poses de la mise en place : remplit le terrain.</summary>
        public List<(CombatMonster monster, int slot)> ChooseSetup(CombatEngine engine, Side side)
            => ChoosePlacements(engine, side, new List<CombatMonster>());

        /// <summary>Joue la phase en cours : pose, puis compétences. N'appelle pas EndPhase.</summary>
        public void PlayPhase(CombatEngine engine)
        {
            if (engine.State != CombatState.Phase) return;
            var side = engine.CurrentSide;

            foreach (var (monster, slot) in ChoosePlacements(engine, side, engine.GetField(side).ToList()))
                engine.Place(monster, slot);

            var order = engine.GetField(side).OrderBy(m => ActionOrder(m.Role)).ToList();
            foreach (var monster in order)
            {
                while (monster.IsOnField && engine.State == CombatState.Phase)
                {
                    var best = GetOptions(engine, monster).Where(o => o.Score > 0).OrderByDescending(o => o.Score).FirstOrDefault();
                    if (best == null) break;
                    engine.UseSkill(monster, best.SkillIndex, best.Target);
                }
                if (engine.State != CombatState.Phase) return;
            }
        }

        private List<(CombatMonster monster, int slot)> ChoosePlacements(CombatEngine engine, Side side, List<CombatMonster> field)
        {
            var hand   = engine.GetHand(side).ToList();
            var placed = new List<(CombatMonster, int)>();
            var used   = new HashSet<int>(field.Select(m => m.Slot));
            var marked = engine.GetMarkedSlots(side);
            var roles  = field.Select(m => m.Role).ToList();

            int count = Math.Min(GameRules.FieldSlots - field.Count, hand.Count);
            for (int n = 0; n < count; n++)
            {
                var monster = hand.Select(m => (m, value: PlacementValue(m.Role, roles)))
                                  .OrderByDescending(x => x.value).First().m;
                hand.Remove(monster);

                // Remplit d'abord les emplacements non marqués
                int slot = Enumerable.Range(0, GameRules.FieldSlots)
                                     .Where(i => !used.Contains(i))
                                     .Select(i => (i, marks: marked.Count(s => s == i), tie: _random.NextDouble()))
                                     .OrderBy(x => x.marks).ThenBy(x => x.tie)
                                     .First().i;
                used.Add(slot);
                roles.Add(monster.Role);
                placed.Add((monster, slot));
            }
            return placed;
        }

        /// <summary>Priorités simples : un tank s'il n'y en a pas, puis les carry, puis les supports.</summary>
        private double PlacementValue(MonsterRole role, List<MonsterRole> fieldRoles)
        {
            double value = role == MonsterRole.Carry ? 3 : 2;
            if (role == MonsterRole.Tank && !fieldRoles.Contains(MonsterRole.Tank)) value += 2;
            if (role == MonsterRole.Support && fieldRoles.Contains(MonsterRole.Support)) value -= 1;
            return value + _random.NextDouble() * 1.5;
        }

        private static int ActionOrder(MonsterRole role) => role switch
        {
            MonsterRole.Support => 0,
            MonsterRole.Tank    => 1,
            _                   => 2
        };

        // ── Actions ───────────────────────────────────────────────────────

        private sealed class Option
        {
            public double        Score;
            public int           SkillIndex;
            public CombatMonster Target;
        }

        private List<Option> GetOptions(CombatEngine engine, CombatMonster monster)
        {
            var options = new List<Option>();
            for (int i = 0; i < monster.Skills.Count; i++)
            {
                if (!engine.CanUseSkill(monster, i)) continue;

                var skill = monster.Skills[i];
                var kind  = Classify(skill);

                if (!CombatEngine.RequiresTargetChoice(skill.Target))
                {
                    if (kind is SkillKind.AreaDamage or SkillKind.AreaSlow && !engine.GetField(monster.Side.Opponent()).Any()) continue;
                    var self = skill.Target == SkillTarget.Self ? monster : null;
                    options.Add(new Option { Score = Score(engine, monster, i, kind, self), SkillIndex = i, Target = self });
                    continue;
                }

                // L'IA vise les ennemis avec ses attaques, et ses alliés avec le reste
                bool offensive = kind is SkillKind.SingleDamage or SkillKind.SingleDamageSlow or SkillKind.Execute;
                foreach (var target in engine.GetValidTargets(monster, i))
                {
                    if ((target.Side != monster.Side) != offensive) continue;
                    options.Add(new Option { Score = Score(engine, monster, i, kind, target), SkillIndex = i, Target = target });
                }
            }
            return options;
        }

        private double Score(CombatEngine engine, CombatMonster m, int skillIndex, SkillKind kind, CombatMonster t)
        {
            double noise = _random.NextDouble() * Noise;
            var skill = m.Skills[skillIndex];

            switch (kind)
            {
                case SkillKind.SingleDamage:
                {
                    var damage = skill.GetEffect<DamageEffect>();
                    int d = CombatEngine.ComputeDamage(m, t, damage.Amount + m.GetDamageGrowth(skillIndex), damage.IgnoresReductions);
                    if (d >= t.Hp) return KillScore * engine.PointsForDefeatAt(t.Side, t.Slot) + Threat(t) + ExtraValue(engine, m, skill, null) + noise;
                    return d / Unit + 0.15 * Threat(t) + 2.0 * d / Math.Max(t.Hp, 1) + ExtraValue(engine, m, skill, t) + noise;
                }

                case SkillKind.AreaSlow:
                    return engine.GetField(m.Side.Opponent()).Sum(e => 0.5 * Threat(e)) + noise;

                case SkillKind.Execute:
                {
                    var execute = skill.GetEffect<ExecuteEffect>();
                    if (t.Hp <= execute.HpThreshold) return KillScore * engine.PointsForDefeatAt(t.Side, t.Slot) + Threat(t) + noise;
                    return CombatEngine.ComputeDamage(m, t, execute.FallbackDamage, false) / Unit * 0.8 + noise;
                }

                case SkillKind.SingleDamageSlow:
                {
                    var damage = skill.GetEffect<DamageEffect>();
                    int d = CombatEngine.ComputeDamage(m, t, damage.Amount, damage.IgnoresReductions);
                    if (d >= t.Hp) return KillScore + Threat(t) + noise;
                    return d / Unit + 0.4 * Threat(t) + noise;
                }

                case SkillKind.AreaDamage:
                {
                    var damage = skill.GetEffect<DamageEffect>();
                    double value = 0;
                    foreach (var enemy in engine.GetField(m.Side.Opponent()))
                    {
                        int d = CombatEngine.ComputeDamage(m, enemy, damage.Amount, damage.IgnoresReductions);
                        value += d / Unit + 0.4 * Threat(enemy) + (d >= enemy.Hp ? KillScore : 0);
                    }
                    return value + noise;
                }

                case SkillKind.Heal:
                {
                    // Sur un monstre qui saigne, le soin retire les saignements
                    if (t.BleedStacks > 0) return BleedValue(t, t.BleedStacks) + noise;
                    int missing = t.MaxHp - t.Hp;
                    if (missing < 2 * Unit) return -1;
                    return Math.Min(skill.GetEffect<HealEffect>().Amount, missing) / Unit * (t.Hp <= DangerHp ? 1.5 : 0.9) + noise;
                }

                case SkillKind.Protect:
                    return 0.5 + Math.Max(0, DangerHp - t.Hp) / Unit * 0.5 + (t.Role == MonsterRole.Carry ? 1 : 0) + noise;

                case SkillKind.Bless:
                {
                    int dealers = engine.GetField(m.Side).Count(a => a.Skills.Any(s => s.IsActive && s.GetEffect<DamageEffect>() != null));
                    return 1.6 * dealers + noise;
                }

                case SkillKind.Retreat:
                    return t.Hp <= 3 * Unit && engine.GetHand(m.Side).Count > 0 ? 6 + noise : -1;

                default:
                    return -5;
            }
        }

        /// <summary>Saignement évité ou infligé : environ deux déclenchements avant un soin ou la défaite.</summary>
        private static double BleedValue(CombatMonster monster, int stacks)
            => 2.0 * stacks * Math.Max(1, monster.MaxHp * GameRules.BleedPercentPerStack / 100) / Unit;

        /// <summary>
        /// Valeur des effets d'une attaque en plus de ses dégâts : saignement sur la cible (si elle survit),
        /// et effets sur ce monstre ou ses alliés (inciblable, Provocation, dégâts croissants, soin, bonus).
        /// </summary>
        private static double ExtraValue(CombatEngine engine, CombatMonster m, SkillData skill, CombatMonster survivingTarget)
        {
            double value = 0;
            var allies = engine.GetField(m.Side).ToList();
            foreach (var effect in skill.Effects)
            {
                switch (effect)
                {
                    case BleedEffect bleed when effect.AppliesTo == EffectTarget.SkillTargets && survivingTarget != null:
                        value += BleedValue(survivingTarget, bleed.Stacks);
                        break;
                    case UntargetableEffect:
                        value += 1;
                        break;
                    case TauntEffect:
                        value += allies.Count > 1 ? 1 : 0.3;
                        break;
                    case DamageGrowthEffect growth:
                        value += 2 * growth.Amount / Unit;
                        break;
                    case HealEffect heal when effect.AppliesTo == EffectTarget.AllAllies:
                        foreach (var ally in allies)
                            value += ally.BleedStacks > 0 ? BleedValue(ally, ally.BleedStacks)
                                                          : Math.Min(heal.Amount, ally.MaxHp - ally.Hp) / Unit * 0.9;
                        break;
                    case DamageBonusEffect bonus when effect.AppliesTo == EffectTarget.AllAllies:
                        value += 1.6 * bonus.Amount / Unit * allies.Count(a => a.Skills.Any(s => s.IsActive && s.GetEffect<DamageEffect>() != null));
                        break;
                }
            }
            return value;
        }

        /// <summary>Dangerosité d'un monstre : somme du poids de ses compétences actives, plus son bonus de dégâts.</summary>
        private static double Threat(CombatMonster monster)
        {
            double threat = monster.DamageBonus / Unit;
            foreach (var skill in monster.Skills)
                if (skill.IsActive) threat += ThreatWeight(skill);
            return threat;
        }

        /// <summary>
        /// Poids d'une compétence dans la menace. Formule choisie pour retrouver les valeurs du simulateur :
        /// Griffe 3, Morsure perforante 2, Exécution 2, Grand balayage 2, Coup de bouclier 1,5,
        /// Soin 2, Bénédiction 2,5, Bouclier 1, Repli 0,5. Les effets en plus d'une attaque (Essences) l'alourdissent.
        /// </summary>
        internal static double ThreatWeight(SkillData skill)
        {
            switch (Classify(skill))
            {
                case SkillKind.SingleDamage:
                case SkillKind.SingleDamageSlow:
                case SkillKind.AreaDamage:
                    double weight = skill.GetEffect<DamageEffect>().Amount / Unit;
                    if (skill.GetEffect<SlowEffect>() != null)  weight += 0.5;
                    if (skill.Target == SkillTarget.AllEnemies) weight += 0.5;
                    if (skill.GetEffect<BleedEffect>() is { } bleed)      weight += 0.5 * bleed.Stacks;
                    if (skill.GetEffect<DamageGrowthEffect>() != null)    weight += 1;
                    if (skill.GetEffect<HealEffect>() is { } heal)        weight += heal.Amount / Unit;
                    if (skill.GetEffect<DamageBonusEffect>() is { } more) weight += 2.5 * more.Amount / Unit;
                    return weight;
                case SkillKind.AreaSlow: return 1.5;
                case SkillKind.Execute: return 2;
                case SkillKind.Heal:    return skill.GetEffect<HealEffect>().Amount / Unit / 2.0;
                case SkillKind.Protect: return skill.GetEffect<DamageReductionEffect>().Amount / Unit / 2.0;
                case SkillKind.Bless:   return 2.5 * skill.GetEffect<DamageBonusEffect>().Amount / Unit;
                case SkillKind.Retreat: return 0.5;
                default:                return 0;
            }
        }

        // ── Reconnaissance des compétences ────────────────────────────────

        internal enum SkillKind
        {
            Unknown,
            /// <summary>Dégâts à une cible (Griffe, Morsure perforante).</summary>
            SingleDamage,
            /// <summary>Dégâts et ralentissement à une cible (Coup de bouclier).</summary>
            SingleDamageSlow,
            /// <summary>Dégâts à tous les ennemis (Grand balayage).</summary>
            AreaDamage,
            /// <summary>Ralentissement de tous les ennemis, sans dégâts (Essence des profondeurs).</summary>
            AreaSlow,
            Execute,
            Heal,
            /// <summary>Réduction de dégâts sur un allié (Bouclier).</summary>
            Protect,
            /// <summary>Bonus de dégâts à tous les alliés (Bénédiction).</summary>
            Bless,
            /// <summary>Retour en main d'un allié (Repli).</summary>
            Retreat
        }

        internal static SkillKind Classify(SkillData skill)
        {
            if (!skill.IsActive) return SkillKind.Unknown;

            if (skill.GetEffect<ExecuteEffect>() != null)      return SkillKind.Execute;
            if (skill.GetEffect<DamageEffect>() != null)
            {
                if (skill.Target == SkillTarget.AllEnemies)    return SkillKind.AreaDamage;
                if (!CombatEngine.RequiresTargetChoice(skill.Target)) return SkillKind.Unknown;
                return skill.GetEffect<SlowEffect>() != null ? SkillKind.SingleDamageSlow : SkillKind.SingleDamage;
            }
            if (skill.GetEffect<ReturnToHandEffect>() != null) return SkillKind.Retreat;
            if (skill.GetEffect<HealEffect>() != null)         return skill.Target == SkillTarget.AllAllies ? SkillKind.Unknown : SkillKind.Heal;
            if (skill.GetEffect<DamageReductionEffect>() != null) return skill.Target == SkillTarget.AllAllies ? SkillKind.Unknown : SkillKind.Protect;
            if (skill.GetEffect<DamageBonusEffect>() != null && skill.Target == SkillTarget.AllAllies) return SkillKind.Bless;
            if (skill.GetEffect<SlowEffect>() != null && skill.Target == SkillTarget.AllEnemies)       return SkillKind.AreaSlow;
            return SkillKind.Unknown;
        }
    }
}
