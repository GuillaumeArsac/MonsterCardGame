using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using MonsterCardGame.Gameplay.Combat;

namespace MonsterCardGame.UI.Combat
{
    /// <summary>Journal des dernières actions du combat, écrit à partir des événements du moteur.</summary>
    public class CombatLog : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField, Min(1)] private int _maxLines = 14;

        private readonly Queue<string> _lines = new();

        public void Listen(CombatEngine engine)
        {
            engine.PhaseStarted    += e => Add($"<b>Tour {e.Turn} — {(e.Side == Side.Player ? "votre phase" : "phase adverse")}</b>");
            engine.MonsterPlaced   += e => { if (!e.DuringSetup) Add($"{Name(e.Monster)} est posé."); };
            engine.MonsterSummoned += e => Add($"{Name(e.Summoner)} invoque {Name(e.Monster)}.");
            engine.Reinforced      += e => Add($"{Name(e.Monster)} est renforcé : +{e.Hp} PV, +{e.Damage} dégâts infligés.");
            engine.SkillUsed       += e => Add(SkillText(e));
            engine.Damaged         += e => Add(e.IsFatigue ? $"{Name(e.Target)} subit {e.Amount} de fatigue ({e.HpAfter} PV)."
                                             : e.IsBleed   ? $"{Name(e.Target)} saigne : {e.Amount} dégât{(e.Amount > 1 ? "s" : "")} ({e.HpAfter} PV)."
                                             : $"{Name(e.Target)} subit {e.Amount} dégât{(e.Amount > 1 ? "s" : "")} ({e.HpAfter} PV).");
            engine.Healed          += e => Add($"{Name(e.Target)} récupère {e.Amount} PV ({e.HpAfter} PV).");
            engine.BleedChanged    += e => Add(e.IsCleansed
                                                ? $"{Name(e.Target)} ne saigne plus."
                                                : $"{Name(e.Target)} saigne (×{e.Total}).");
            engine.ModifierApplied += e =>
            {
                if (e.Modifier.Kind == ModifierKind.Untargetable) Add($"{Name(e.Target)} devient inciblable.");
                if (e.Modifier.Kind == ModifierKind.Taunt)        Add($"{Name(e.Target)} provoque l'adversaire.");
            };
            engine.Slowed          += m => Add($"{Name(m)} est ralenti.");
            engine.ReturnedToHand  += m => Add($"{Name(m)} retourne dans la main.");
            engine.MonsterDefeated += e => Add(e.PointsAwarded == 0
                                             ? $"{Name(e.Monster)} est vaincu{(e.ByFatigue ? " par la fatigue" : "")}, sans point marqué."
                                             : $"{Name(e.Monster)} est vaincu{(e.ByFatigue ? " par la fatigue" : "")} : "
                                             + $"+{e.PointsAwarded} point{(e.PointsAwarded > 1 ? "s" : "")} pour {(e.Monster.Side == Side.Player ? "l'adversaire" : "vous")}.");
            engine.CombatEnded     += e => Add($"<b>{(e.Winner == Side.Player ? "Victoire !" : "Défaite.")}</b>");
        }

        private static string Name(CombatMonster m)
            => m.Side == Side.Player ? $"<color=#8fc3ff>{m.Name}</color>" : $"<color=#ff9a8f>{m.Name}</color>";

        private static string SkillText(SkillUsedEvent e)
        {
            string skill = $"<i>{e.Skill.SkillName}</i>";
            if (e.Skill.IsPassive) return $"{Name(e.Monster)} : {skill}.";
            if (e.Targets.Count == 1 && e.Targets[0] != e.Monster) return $"{Name(e.Monster)} utilise {skill} sur {Name(e.Targets[0])}.";
            return $"{Name(e.Monster)} utilise {skill}.";
        }

        private void Add(string line)
        {
            _lines.Enqueue(line);
            while (_lines.Count > _maxLines) _lines.Dequeue();
            _label.text = string.Join("\n", _lines.Reverse());
        }
    }
}
