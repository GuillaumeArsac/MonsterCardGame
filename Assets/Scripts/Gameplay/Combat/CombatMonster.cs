using System;
using System.Collections.Generic;
using MonsterCardGame.Core;
using MonsterCardGame.Gameplay.Monsters;
using MonsterCardGame.Gameplay.Skills;
using MonsterCardGame.Gameplay.Skills.Effects;

namespace MonsterCardGame.Gameplay.Combat
{
    /// <summary>Les deux camps d'un combat. Le joueur joue toujours en premier.</summary>
    public enum Side
    {
        Player,
        Enemy
    }

    public static class SideExtensions
    {
        public static Side Opponent(this Side side) => side == Side.Player ? Side.Enemy : Side.Player;
    }

    /// <summary>Où se trouve un monstre pendant le combat.</summary>
    public enum MonsterZone
    {
        Hand,
        Field,
        /// <summary>Vaincu : retiré du combat.</summary>
        Defeated
    }

    /// <summary>Un monstre pendant un combat : ses PV, ses compteurs de recharge et ses effets temporaires.</summary>
    public class CombatMonster
    {
        public const int NoSlot = -1;

        private readonly int[]  _cooldowns;
        private readonly bool[] _usedThisPhase;
        /// <summary>Dégâts gagnés par chaque compétence pendant ce combat (Essence de nature).</summary>
        private readonly int[]  _damageGrowth;
        private readonly List<CombatModifier> _modifiers = new();

        public string                   Name   { get; }
        public MonsterRole              Role   { get; }
        public int                      MaxHp  { get; }
        public IReadOnlyList<SkillData> Skills { get; }
        /// <summary>Objet d'origine (OwnedMonster ou MonsterCardData), pour l'interface.</summary>
        public object                   Origin { get; }

        public Side        Side { get; internal set; }
        public int         Hp   { get; internal set; }
        public MonsterZone Zone { get; internal set; } = MonsterZone.Hand;
        public int         Slot { get; internal set; } = NoSlot;

        public bool IsOnField => Zone == MonsterZone.Field;

        /// <summary>Cumuls de saignement : retirés par un soin, un retour en main ou la défaite.</summary>
        public int BleedStacks { get; internal set; }

        /// <summary>Dégâts de saignement au début de la phase de son camp : 5 % des PV max par cumul, 1 minimum par cumul.</summary>
        public int BleedDamage => BleedStacks * Math.Max(1, MaxHp * GameRules.BleedPercentPerStack / 100);

        /// <summary>Effets temporaires en cours (bonus, réductions).</summary>
        public IReadOnlyList<CombatModifier> Modifiers => _modifiers;

        public CombatMonster(string name, MonsterRole role, int maxHp, IEnumerable<SkillData> skills, object origin = null)
        {
            Name   = name;
            Role   = role;
            MaxHp  = maxHp;
            Hp     = maxHp;
            Skills = new List<SkillData>(skills ?? throw new ArgumentNullException(nameof(skills)));
            Origin = origin;

            _cooldowns     = new int[Skills.Count];
            _usedThisPhase = new bool[Skills.Count];
            _damageGrowth  = new int[Skills.Count];
        }

        /// <summary>Monstre du joueur : ses compétences actuelles et ses PV avec le boost.</summary>
        public static CombatMonster From(OwnedMonster monster)
            => new(monster.Name, monster.Origin.Role, monster.MaxHp, monster.Skills, monster);

        /// <summary>Monstre adverse : sa carte telle quelle (son boost est une récompense du dépeçage).</summary>
        public static CombatMonster From(MonsterCardData card)
            => new(card.MonsterName, card.Role, card.MaxHp, card.Skills, card);

        // ── Compétences ───────────────────────────────────────────────────

        /// <summary>Compteur de recharge de la compétence (0 = disponible). Toujours 0 pour une passive.</summary>
        public int  GetCooldown(int skillIndex)      => _cooldowns[skillIndex];
        public bool WasUsedThisPhase(int skillIndex) => _usedThisPhase[skillIndex];

        public bool HasTaunt => HasAlwaysPassive<TauntEffect>(_ => true) || HasModifier(ModifierKind.Taunt);

        /// <summary>Vrai si les compétences adverses ne peuvent pas le choisir comme cible.</summary>
        public bool IsUntargetable => HasModifier(ModifierKind.Untargetable);

        /// <summary>Dégâts ajoutés à cette compétence par ses utilisations précédentes (Essence de nature).</summary>
        public int GetDamageGrowth(int skillIndex) => _damageGrowth[skillIndex];

        /// <summary>Bonus aux dégâts infligés : effets temporaires et passives permanentes.</summary>
        public int DamageBonus => SumModifiers(ModifierKind.DamageBonus)
                                + SumAlwaysPassives<DamageBonusEffect>(e => e.Amount);

        /// <summary>Réduction des dégâts subis : effets temporaires et passives permanentes (Carapace).</summary>
        public int DamageReduction => SumModifiers(ModifierKind.DamageReduction)
                                    + SumAlwaysPassives<DamageReductionEffect>(e => e.Amount);

        // ── Changements d'état (appelés par CombatEngine) ─────────────────

        /// <summary>Entrée sur le terrain : tous les compteurs au maximum, aucun effet temporaire.</summary>
        internal void EnterField(int slot)
        {
            Zone = MonsterZone.Field;
            Slot = slot;
            for (int i = 0; i < Skills.Count; i++)
            {
                _cooldowns[i]     = Skills[i].IsActive ? Skills[i].Cooldown : 0;
                _usedThisPhase[i] = false;
            }
            _modifiers.Clear();
        }

        /// <summary>Retour en main : garde ses PV, perd ses effets temporaires, son ralentissement et ses saignements.</summary>
        internal void ReturnToHand()
        {
            Zone = MonsterZone.Hand;
            Slot = NoSlot;
            Array.Clear(_cooldowns, 0, _cooldowns.Length);
            _modifiers.Clear();
            BleedStacks = 0;
        }

        internal void Defeat()
        {
            Zone = MonsterZone.Defeated;
            Slot = NoSlot;
            Hp   = 0;
            _modifiers.Clear();
            BleedStacks = 0;
        }

        internal void MarkUsed(int skillIndex)
        {
            _cooldowns[skillIndex]     = Skills[skillIndex].Cooldown;
            _usedThisPhase[skillIndex] = true;
        }

        internal void ResetPhaseUsage() => Array.Clear(_usedThisPhase, 0, _usedThisPhase.Length);

        /// <summary>Ralentissement : +1 au compteur de toutes les compétences actives, même disponibles.</summary>
        internal void Slow()
        {
            for (int i = 0; i < Skills.Count; i++)
                if (Skills[i].IsActive) _cooldowns[i]++;
        }

        /// <summary>Fin de la phase du camp de ce monstre : les compteurs supérieurs à 0 baissent de 1.</summary>
        internal void TickCooldowns()
        {
            for (int i = 0; i < _cooldowns.Length; i++)
                if (_cooldowns[i] > 0) _cooldowns[i]--;
        }

        internal void AddModifier(CombatModifier modifier) => _modifiers.Add(modifier);

        internal void AddDamageGrowth(int skillIndex, int amount) => _damageGrowth[skillIndex] += amount;

        /// <summary>Fin de la phase de <paramref name="endingSide"/> : les effets qui comptent ses phases avancent.</summary>
        internal void TickModifiers(Side endingSide)
        {
            foreach (var modifier in _modifiers)
                if (modifier.CountdownSide == endingSide) modifier.Tick();
            _modifiers.RemoveAll(m => m.IsExpired);
        }

        private bool HasModifier(ModifierKind kind)
        {
            foreach (var modifier in _modifiers)
                if (modifier.Kind == kind) return true;
            return false;
        }

        private int SumModifiers(ModifierKind kind)
        {
            int total = 0;
            foreach (var modifier in _modifiers)
                if (modifier.Kind == kind) total += modifier.Amount;
            return total;
        }

        private int SumAlwaysPassives<T>(Func<T, int> value) where T : SkillEffect
        {
            int total = 0;
            foreach (var skill in Skills)
            {
                if (!skill.IsPassive || skill.Trigger != PassiveTrigger.Always) continue;
                foreach (var effect in skill.Effects)
                    if (effect is T typed) total += value(typed);
            }
            return total;
        }

        private bool HasAlwaysPassive<T>(Func<T, bool> predicate) where T : SkillEffect
        {
            foreach (var skill in Skills)
            {
                if (!skill.IsPassive || skill.Trigger != PassiveTrigger.Always) continue;
                foreach (var effect in skill.Effects)
                    if (effect is T typed && predicate(typed)) return true;
            }
            return false;
        }

        public override string ToString() => $"{Name} ({Side}, {Hp}/{MaxHp} PV)";
    }
}
