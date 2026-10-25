using System;
using System.Collections.Generic;
using System.Linq;
using MonsterCardGame.Core;
using MonsterCardGame.Gameplay.Skills;
using MonsterCardGame.Gameplay.Skills.Effects;

namespace MonsterCardGame.Gameplay.Combat
{
    public enum CombatState
    {
        /// <summary>Mise en place : les deux camps choisissent en secret leurs premières poses.</summary>
        Setup,
        /// <summary>Phase d'un camp (CurrentSide) : pose libre, puis compétences.</summary>
        Phase,
        Ended
    }

    /// <summary>
    /// Moteur de combat, en C# pur : il applique les règles et prévient l'interface par des événements.
    /// Le joueur et l'IA le pilotent avec les mêmes commandes : Setup, Place, UseSkill, EndPhase.
    /// Références : docs/combat.md et docs/competences.md.
    /// </summary>
    public class CombatEngine
    {
        private readonly Random _random;
        private readonly Dictionary<Side, List<CombatMonster>> _team  = new();
        private readonly Dictionary<Side, List<CombatMonster>> _hand  = new();
        private readonly Dictionary<Side, CombatMonster[]>     _field = new();
        private readonly Dictionary<Side, int>                 _points = new();
        /// <summary>Emplacements de chaque camp marqués par les Chasseurs de trophées adverses (un par exemplaire).</summary>
        private readonly Dictionary<Side, List<int>>           _markedSlots = new();

        private bool _skillUsedThisPhase;

        public event Action<MonsterPlacedEvent>   MonsterPlaced;
        public event Action<SkillUsedEvent>       SkillUsed;
        public event Action<DamageEvent>          Damaged;
        public event Action<HealEvent>            Healed;
        public event Action<BleedEvent>           BleedChanged;
        public event Action<CombatMonster>        Slowed;
        public event Action<ModifierAppliedEvent> ModifierApplied;
        public event Action<CombatMonster>        ReturnedToHand;
        public event Action<MonsterDefeatedEvent> MonsterDefeated;
        public event Action<PhaseEvent>           PhaseStarted;
        public event Action<PhaseEvent>           PhaseEnded;
        public event Action<CombatEndedEvent>     CombatEnded;

        public CombatState     State       { get; private set; } = CombatState.Setup;
        /// <summary>Tour en cours, à partir de 1. 0 pendant la mise en place.</summary>
        public int             Turn        { get; private set; }
        public Side            CurrentSide { get; private set; } = Side.Player;
        public Side?           Winner      { get; private set; }
        public CombatEndReason EndReason   { get; private set; }

        /// <summary>Vrai dès qu'une compétence a été utilisée pendant la phase : on ne peut plus poser.</summary>
        public bool PlacementLocked => _skillUsedThisPhase;

        /// <param name="random">Hasard du combat (tirage des emplacements marqués). Fixer la graine pour les tests.</param>
        public CombatEngine(IEnumerable<CombatMonster> playerTeam, IEnumerable<CombatMonster> enemyTeam, Random random = null)
        {
            _random = random ?? new Random();

            InitSide(Side.Player, playerTeam);
            InitSide(Side.Enemy,  enemyTeam);

            // Chasseur de trophées : le tirage a lieu avant la mise en place, visible des deux camps
            foreach (var side in new[] { Side.Player, Side.Enemy })
                foreach (var monster in _team[side])
                    foreach (var skill in monster.Skills)
                        if (skill.IsPassive && skill.Trigger == PassiveTrigger.StartOfCombat)
                            foreach (var effect in skill.Effects)
                                if (effect is MarkedSlotEffect mark)
                                    MarkSlot(side.Opponent(), _random.Next(GameRules.FieldSlots), mark.BonusPoints);
        }

        /// <summary>Ajoute un point bonus par marque : un emplacement marqué deux fois rapporte 2 points de plus.</summary>
        private void MarkSlot(Side side, int slot, int bonusPoints)
        {
            for (int i = 0; i < bonusPoints; i++)
                _markedSlots[side].Add(slot);
        }

        private void InitSide(Side side, IEnumerable<CombatMonster> team)
        {
            var members = team?.ToList() ?? throw new ArgumentNullException(nameof(team));
            if (members.Count == 0 || members.Count > GameRules.TeamSize)
                throw new ArgumentException($"Une équipe a de 1 à {GameRules.TeamSize} monstres ({members.Count} reçus)");

            foreach (var monster in members) monster.Side = side;

            _team[side]        = members;
            _hand[side]        = new List<CombatMonster>(members);
            _field[side]       = new CombatMonster[GameRules.FieldSlots];
            _points[side]      = 0;
            _markedSlots[side] = new List<int>();
        }

        // ── Lecture de l'état ─────────────────────────────────────────────

        public IReadOnlyList<CombatMonster> GetTeam(Side side) => _team[side];
        public IReadOnlyList<CombatMonster> GetHand(Side side) => _hand[side];
        public CombatMonster GetSlot(Side side, int slot)       => _field[side][slot];
        public IEnumerable<CombatMonster> GetField(Side side)   => _field[side].Where(m => m != null);
        public int GetPoints(Side side)                         => _points[side];

        /// <summary>Emplacements de ce camp marqués par l'adversaire. Un emplacement peut apparaître plusieurs fois.</summary>
        public IReadOnlyList<int> GetMarkedSlots(Side side) => _markedSlots[side];

        /// <summary>Points que rapporte à l'adversaire un monstre de ce camp vaincu sur cet emplacement.</summary>
        public int PointsForDefeatAt(Side side, int slot) => 1 + _markedSlots[side].Count(s => s == slot);

        /// <summary>Dégâts de fatigue au début du tour : 0 avant le tour 16, puis 10, 20, 30...</summary>
        public static int FatigueDamage(int turn)
            => turn < GameRules.FatigueStartTurn ? 0 : (turn - GameRules.FatigueStartTurn + 1) * GameRules.FatigueDamageStep;

        /// <summary>Dégâts qu'infligerait <paramref name="source"/> à <paramref name="target"/> : bonus, puis réductions.</summary>
        public static int ComputeDamage(CombatMonster source, CombatMonster target, int baseAmount, bool ignoresReductions)
        {
            int amount = baseAmount + (source?.DamageBonus ?? 0);
            if (!ignoresReductions) amount -= target.DamageReduction;
            return Math.Max(0, amount);
        }

        // ── Mise en place ─────────────────────────────────────────────────

        /// <summary>Pose secrète simultanée : les deux camps révèlent leurs poses en même temps, puis le tour 1 commence.</summary>
        public void Setup(IEnumerable<(CombatMonster monster, int slot)> playerPlacements,
                          IEnumerable<(CombatMonster monster, int slot)> enemyPlacements)
        {
            if (State != CombatState.Setup) throw new InvalidOperationException("La mise en place est déjà faite");

            var player = playerPlacements?.ToList() ?? new List<(CombatMonster, int)>();
            var enemy  = enemyPlacements?.ToList()  ?? new List<(CombatMonster, int)>();
            ValidatePlacements(Side.Player, player);
            ValidatePlacements(Side.Enemy,  enemy);

            foreach (var (monster, slot) in player.Concat(enemy))
                PutOnField(monster, slot, duringSetup: true);

            StartTurn();
        }

        private void ValidatePlacements(Side side, List<(CombatMonster monster, int slot)> placements)
        {
            var slots = new HashSet<int>();
            var monsters = new HashSet<CombatMonster>();
            foreach (var (monster, slot) in placements)
            {
                if (monster == null || monster.Side != side || !_hand[side].Contains(monster))
                    throw new InvalidOperationException($"{monster} n'est pas dans la main du camp {side}");
                if (slot < 0 || slot >= GameRules.FieldSlots || !slots.Add(slot))
                    throw new InvalidOperationException($"Emplacement {slot} invalide ou déjà pris");
                if (!monsters.Add(monster))
                    throw new InvalidOperationException($"{monster.Name} est posé deux fois");
            }
        }

        // ── Phase : pose ──────────────────────────────────────────────────

        public bool CanPlace(CombatMonster monster, int slot)
            => State == CombatState.Phase
            && monster != null
            && monster.Side == CurrentSide
            && monster.Zone == MonsterZone.Hand
            && !_skillUsedThisPhase
            && slot >= 0 && slot < GameRules.FieldSlots
            && _field[CurrentSide][slot] == null;

        /// <summary>Pose un monstre de la main sur un emplacement libre. Seulement avant la première compétence de la phase.</summary>
        public void Place(CombatMonster monster, int slot)
        {
            if (!CanPlace(monster, slot))
                throw new InvalidOperationException($"Impossible de poser {monster?.Name} sur l'emplacement {slot}");

            PutOnField(monster, slot, duringSetup: false);
        }

        private void PutOnField(CombatMonster monster, int slot, bool duringSetup)
        {
            _hand[monster.Side].Remove(monster);
            _field[monster.Side][slot] = monster;
            monster.EnterField(slot);
            RefreshAllegiances(monster.Side);
            MonsterPlaced?.Invoke(new MonsterPlacedEvent(monster, slot, duringSetup));
        }

        // ── Phase : compétences ───────────────────────────────────────────

        /// <summary>Vrai si la cible de la compétence doit être choisie par le joueur.</summary>
        public static bool RequiresTargetChoice(SkillTarget target)
            => target is SkillTarget.AnyMonster or SkillTarget.Enemy or SkillTarget.Ally or SkillTarget.OtherAlly;

        public bool CanUseSkill(CombatMonster monster, int skillIndex)
        {
            if (State != CombatState.Phase || monster == null || monster.Side != CurrentSide || !monster.IsOnField)
                return false;
            if (skillIndex < 0 || skillIndex >= monster.Skills.Count)
                return false;

            var skill = monster.Skills[skillIndex];
            if (!skill.IsActive || monster.GetCooldown(skillIndex) > 0 || monster.WasUsedThisPhase(skillIndex))
                return false;

            return !RequiresTargetChoice(skill.Target) || GetValidTargets(monster, skillIndex).Count > 0;
        }

        /// <summary>Cibles possibles d'une compétence à cible choisie, Provocation comprise. Vide sinon.</summary>
        public IReadOnlyList<CombatMonster> GetValidTargets(CombatMonster monster, int skillIndex)
        {
            var target = monster.Skills[skillIndex].Target;
            var allies = GetField(monster.Side).ToList();

            return target switch
            {
                SkillTarget.AnyMonster => allies.Concat(EnemyTargets(monster)).ToList(),
                SkillTarget.Enemy      => EnemyTargets(monster),
                SkillTarget.Ally       => allies,
                SkillTarget.OtherAlly  => allies.Where(m => m != monster).ToList(),
                _                      => Array.Empty<CombatMonster>()
            };
        }

        /// <summary>
        /// Ennemis ciblables par <paramref name="attacker"/>, sauf les inciblables : ceux qui ont Provocation s'il y en a,
        /// sinon tous. Un attaquant qui ignore la Provocation (allégeance de l'Archer) peut viser tous les ennemis.
        /// </summary>
        private List<CombatMonster> EnemyTargets(CombatMonster attacker)
        {
            var enemies  = GetField(attacker.Side.Opponent()).Where(m => !m.IsUntargetable).ToList();
            if (attacker.IgnoresTaunt) return enemies;
            var taunters = enemies.Where(m => m.HasTaunt).ToList();
            return taunters.Count > 0 ? taunters : enemies;
        }

        /// <summary>Utilise une compétence active. <paramref name="target"/> est ignorée si la compétence ne choisit pas de cible.</summary>
        public void UseSkill(CombatMonster monster, int skillIndex, CombatMonster target = null)
        {
            if (!CanUseSkill(monster, skillIndex))
                throw new InvalidOperationException($"{monster?.Name} ne peut pas utiliser la compétence {skillIndex}");

            var skill   = monster.Skills[skillIndex];
            var targets = ResolveTargets(monster, skill, target, skillIndex);

            // Le compteur prend la valeur du temps de recharge avant les effets
            monster.MarkUsed(skillIndex);
            _skillUsedThisPhase = true;

            SkillUsed?.Invoke(new SkillUsedEvent(monster, skillIndex, skill, targets));
            ApplyEffects(monster, skillIndex, targets);
            CheckEnd();
        }

        private IReadOnlyList<CombatMonster> ResolveTargets(CombatMonster monster, SkillData skill, CombatMonster chosen, int skillIndex)
        {
            if (RequiresTargetChoice(skill.Target))
            {
                if (chosen == null || !GetValidTargets(monster, skillIndex).Contains(chosen))
                    throw new InvalidOperationException($"{chosen?.Name ?? "Aucune cible"} n'est pas une cible valide pour {skill.SkillName}");
                return new[] { chosen };
            }

            return AutomaticTargets(monster, skill.Target);
        }

        private IReadOnlyList<CombatMonster> AutomaticTargets(CombatMonster monster, SkillTarget target) => target switch
        {
            SkillTarget.Self       => new[] { monster },
            SkillTarget.AllEnemies => GetField(monster.Side.Opponent()).ToList(),
            SkillTarget.AllAllies  => GetField(monster.Side).ToList(),
            _                      => Array.Empty<CombatMonster>()
        };

        // ── Effets ────────────────────────────────────────────────────────

        /// <summary>
        /// Applique les effets dans l'ordre de la fiche. Chaque effet touche les cibles de la compétence,
        /// ou les monstres qu'il désigne lui-même (ce monstre, tous les alliés).
        /// </summary>
        private void ApplyEffects(CombatMonster source, int skillIndex, IReadOnlyList<CombatMonster> targets)
        {
            // PV retirés par les dégâts de la compétence, pour le vol de vie
            int dealt = 0;

            foreach (var effect in source.Skills[skillIndex].Effects)
            {
                if (effect == null) continue;

                // Effets sur ce monstre, quelle que soit la cible (Essence de nature)
                if (effect is DamageGrowthEffect growth)
                {
                    source.AddDamageGrowth(skillIndex, growth.Amount);
                    continue;
                }
                if (effect is MaxHpGrowthEffect hpGrowth)
                {
                    if (source.IsOnField) source.GrowMaxHp(hpGrowth.Amount);
                    continue;
                }

                // Vol de vie (Essence de vie) : l'allié le plus blessé récupère les PV retirés par la compétence
                if (effect is LifeStealEffect)
                {
                    var wounded = GetField(source.Side).OrderByDescending(m => m.MaxHp - m.Hp).FirstOrDefault();
                    if (wounded != null && dealt > 0) Heal(source, wounded, dealt);
                    continue;
                }

                // Une cible vaincue ou renvoyée en main ne subit pas la suite des effets
                foreach (var target in EffectTargets(source, effect, targets).ToList())
                    if (target.IsOnField) dealt += ApplyEffect(source, skillIndex, effect, target);
            }
        }

        private IEnumerable<CombatMonster> EffectTargets(CombatMonster source, SkillEffect effect, IReadOnlyList<CombatMonster> skillTargets)
            => effect.AppliesTo switch
            {
                EffectTarget.Self      => source.IsOnField ? new[] { source } : Array.Empty<CombatMonster>(),
                EffectTarget.AllAllies => GetField(source.Side),
                _                      => skillTargets
            };

        /// <summary>Applique un effet à une cible. Retourne les PV retirés par des dégâts (0 pour les autres effets).</summary>
        private int ApplyEffect(CombatMonster source, int skillIndex, SkillEffect effect, CombatMonster target)
        {
            switch (effect)
            {
                case DamageEffect damage:
                    return DealDamage(source, target, damage.Amount + source.GetDamageGrowth(skillIndex), damage.IgnoresReductions);

                case BleedEffect bleed:
                    target.BleedStacks += bleed.Stacks;
                    BleedChanged?.Invoke(new BleedEvent(target, source, bleed.Stacks, target.BleedStacks));
                    break;

                case UntargetableEffect untargetable:
                    AddModifier(source, target, new CombatModifier(ModifierKind.Untargetable, 0, source.Side, untargetable.DurationTurns));
                    break;

                // Une Provocation passive est lue directement sur la fiche ; dans une active, elle dure le temps indiqué
                case TauntEffect taunt:
                    if (source.Skills[skillIndex].IsActive)
                        AddModifier(source, target, new CombatModifier(ModifierKind.Taunt, 0, source.Side, taunt.DurationTurns));
                    break;

                case HealEffect heal:
                    Heal(source, target, heal.Amount);
                    break;

                case DamageBonusEffect bonus:
                    AddModifier(source, target, new CombatModifier(ModifierKind.DamageBonus, bonus.Amount, source.Side, bonus.DurationTurns));
                    break;

                case DamageReductionEffect reduction:
                    AddModifier(source, target, new CombatModifier(ModifierKind.DamageReduction, reduction.Amount, source.Side, reduction.DurationTurns));
                    break;

                case SlowEffect:
                    target.Slow();
                    Slowed?.Invoke(target);
                    break;

                case ExecuteEffect execute:
                    if (target.Hp <= execute.HpThreshold) Defeat(target, byFatigue: false);
                    else DealDamage(source, target, execute.FallbackDamage, ignoresReductions: false);
                    break;

                case ReturnToHandEffect:
                    SendToHand(target);
                    break;

                // Passives traitées ailleurs : emplacement marqué (début du combat), chef et allégeances (terrain)
                case MarkedSlotEffect:
                case LeaderEffect:
                case PerpetualEffect:
                case AllegianceEffect:
                    break;

                default:
                    throw new NotSupportedException($"Effet non géré par le moteur : {effect?.GetType().Name}");
            }
            return 0;
        }

        /// <summary>Inflige des dégâts. Retourne les PV réellement retirés.</summary>
        private int DealDamage(CombatMonster source, CombatMonster target, int baseAmount, bool ignoresReductions)
        {
            int amount = ComputeDamage(source, target, baseAmount, ignoresReductions);
            int removed = Math.Min(amount, target.Hp);
            target.Hp -= removed;
            Damaged?.Invoke(new DamageEvent(target, source, amount, target.Hp, isFatigue: false));

            if (target.Hp <= 0) Defeat(target, byFatigue: false);
            return removed;
        }

        /// <summary>Soin : sur une cible qui saigne, retire tous les saignements au lieu de rendre des PV.</summary>
        private void Heal(CombatMonster source, CombatMonster target, int amount)
        {
            if (target.BleedStacks > 0)
            {
                target.BleedStacks = 0;
                BleedChanged?.Invoke(new BleedEvent(target, source, 0, 0));
                return;
            }

            int healed = Math.Min(amount, target.MaxHp - target.Hp);
            if (healed <= 0) return;

            target.Hp += healed;
            Healed?.Invoke(new HealEvent(target, source, healed, target.Hp));
        }

        private void AddModifier(CombatMonster source, CombatMonster target, CombatModifier modifier)
        {
            target.AddModifier(modifier);
            ModifierApplied?.Invoke(new ModifierAppliedEvent(target, source, modifier));
        }

        private void SendToHand(CombatMonster monster)
        {
            _field[monster.Side][monster.Slot] = null;
            monster.ReturnToHand();
            _hand[monster.Side].Add(monster);
            RefreshAllegiances(monster.Side);
            ReturnedToHand?.Invoke(monster);
        }

        /// <summary>Monstre vaincu : retiré du combat, 1 point pour l'adversaire (plus l'emplacement marqué), fatigue et saignement compris.</summary>
        private void Defeat(CombatMonster monster, bool byFatigue)
        {
            var side = monster.Side;
            int slot = monster.Slot;

            _field[side][slot] = null;
            monster.Defeat();

            int points = PointsForDefeatAt(side, slot);
            _points[side.Opponent()] += points;
            RefreshAllegiances(side);

            MonsterDefeated?.Invoke(new MonsterDefeatedEvent(monster, slot, points, byFatigue));

            // Perpétuel (Thrâk) : quand l'adversaire marque, les monstres perpétuels de ce camp se régénèrent
            if (points > 0)
                foreach (var perpetual in GetField(side).Where(m => m.IsPerpetual).ToList())
                    RestoreFully(perpetual);
        }

        /// <summary>Tous les PV et plus de saignement. Ce n'est pas un soin : la règle du saignement ne s'applique pas.</summary>
        private void RestoreFully(CombatMonster monster)
        {
            if (monster.BleedStacks > 0)
            {
                monster.BleedStacks = 0;
                BleedChanged?.Invoke(new BleedEvent(monster, null, 0, 0));
            }

            int restored = monster.MaxHp - monster.Hp;
            if (restored <= 0) return;
            monster.Hp = monster.MaxHp;
            Healed?.Invoke(new HealEvent(monster, null, restored, monster.Hp));
        }

        /// <summary>
        /// Allégeance : chaque chef sur le terrain reçoit les bonus des passives d'allégeance de ses alliés sur le terrain.
        /// Recalculé à chaque arrivée ou départ d'un monstre de ce camp.
        /// </summary>
        private void RefreshAllegiances(Side side)
        {
            var field = GetField(side).ToList();
            foreach (var monster in _team[side])
            {
                int damage = 0, reduction = 0;
                bool ignoresTaunt = false;
                if (monster.IsOnField && monster.IsLeader)
                {
                    foreach (var ally in field)
                    {
                        if (ally == monster) continue;
                        foreach (var skill in ally.Skills)
                        {
                            if (!skill.IsPassive || skill.Trigger != PassiveTrigger.Always) continue;
                            foreach (var effect in skill.Effects)
                            {
                                if (effect is not AllegianceEffect allegiance) continue;
                                switch (allegiance.Bonus)
                                {
                                    case AllegianceBonus.DamageBonus:     damage    += allegiance.Amount; break;
                                    case AllegianceBonus.DamageReduction: reduction += allegiance.Amount; break;
                                    case AllegianceBonus.IgnoresTaunt:    ignoresTaunt = true;            break;
                                }
                            }
                        }
                    }
                }
                monster.AllegianceDamageBonus     = damage;
                monster.AllegianceDamageReduction = reduction;
                monster.IgnoresTaunt              = ignoresTaunt;
            }
        }

        // ── Déroulement ───────────────────────────────────────────────────

        /// <summary>Termine la phase du camp en cours et passe à la suivante.</summary>
        public void EndPhase()
        {
            if (State != CombatState.Phase) throw new InvalidOperationException("Aucune phase en cours");

            var side = CurrentSide;

            // Les compteurs des monstres de ce camp baissent de 1
            foreach (var monster in GetField(side))
                monster.TickCooldowns();

            // Les effets appliqués par l'autre camp avancent d'une phase
            foreach (var monster in GetField(Side.Player).Concat(GetField(Side.Enemy)))
                monster.TickModifiers(side);

            PhaseEnded?.Invoke(new PhaseEvent(side, Turn));

            if (side == Side.Player) StartPhase(Side.Enemy);
            else StartTurn();
        }

        private void StartTurn()
        {
            Turn++;

            // Fatigue : dégâts à tous les monstres sur le terrain, sans réduction. Un monstre vaincu rapporte ses points
            int fatigue = FatigueDamage(Turn);
            if (fatigue > 0)
            {
                foreach (var monster in GetField(Side.Player).Concat(GetField(Side.Enemy)).ToList())
                {
                    monster.Hp = Math.Max(0, monster.Hp - fatigue);
                    Damaged?.Invoke(new DamageEvent(monster, null, fatigue, monster.Hp, isFatigue: true));
                    if (monster.Hp <= 0) Defeat(monster, byFatigue: true);
                }
                if (CheckEnd()) return;
            }

            StartPhase(Side.Player);
        }

        private void StartPhase(Side side)
        {
            State               = CombatState.Phase;
            CurrentSide         = side;
            _skillUsedThisPhase = false;

            foreach (var monster in _team[side])
                monster.ResetPhaseUsage();

            PhaseStarted?.Invoke(new PhaseEvent(side, Turn));

            // Saignement, avant les passives : une Aura apaisante le retire, mais après qu'il a frappé
            foreach (var monster in GetField(side).Where(m => m.BleedStacks > 0).ToList())
            {
                int bleed = monster.BleedDamage;
                monster.Hp = Math.Max(0, monster.Hp - bleed);
                Damaged?.Invoke(new DamageEvent(monster, null, bleed, monster.Hp, isFatigue: false, isBleed: true));
                if (monster.Hp <= 0) Defeat(monster, byFatigue: false);
            }
            if (CheckEnd()) return;

            // Passives de début de phase, pour les monstres déjà sur le terrain
            foreach (var monster in GetField(side).ToList())
            {
                for (int i = 0; i < monster.Skills.Count; i++)
                {
                    var skill = monster.Skills[i];
                    if (!monster.IsOnField || !skill.IsPassive || skill.Trigger != PassiveTrigger.StartOfOwnPhase) continue;
                    if (RequiresTargetChoice(skill.Target)) continue;

                    var targets = AutomaticTargets(monster, skill.Target);
                    SkillUsed?.Invoke(new SkillUsedEvent(monster, i, skill, targets));
                    ApplyEffects(monster, i, targets);
                }
            }

            CheckEnd();
        }

        private bool HasMonstersLeft(Side side) => _hand[side].Count > 0 || GetField(side).Any();

        private bool CheckEnd()
        {
            if (State == CombatState.Ended) return true;

            // La fatigue peut faire marquer les deux camps en même temps : le joueur perd
            if (_points[Side.Player] >= GameRules.PointsToWin && _points[Side.Enemy] >= GameRules.PointsToWin)
                return End(Side.Enemy, CombatEndReason.BothReachedPoints);

            foreach (var side in new[] { Side.Player, Side.Enemy })
                if (_points[side] >= GameRules.PointsToWin)
                    return End(side, CombatEndReason.Points);

            bool playerEmpty = !HasMonstersLeft(Side.Player);
            bool enemyEmpty  = !HasMonstersLeft(Side.Enemy);

            if (playerEmpty && enemyEmpty) return End(Side.Enemy, CombatEndReason.BothSidesEmpty);
            if (playerEmpty)               return End(Side.Enemy, CombatEndReason.NoMonstersLeft);
            if (enemyEmpty)                return End(Side.Player, CombatEndReason.NoMonstersLeft);
            return false;
        }

        private bool End(Side winner, CombatEndReason reason)
        {
            State     = CombatState.Ended;
            Winner    = winner;
            EndReason = reason;
            CombatEnded?.Invoke(new CombatEndedEvent(winner, reason));
            return true;
        }
    }
}
