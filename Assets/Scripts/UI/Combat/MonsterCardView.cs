using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MonsterCardGame.Gameplay.Combat;
using MonsterCardGame.Gameplay.Monsters;

namespace MonsterCardGame.UI.Combat
{
    public enum CardHighlight
    {
        None,
        /// <summary>Carte choisie par le joueur (à poser, ou qui agit).</summary>
        Selected,
        /// <summary>Cible possible de la compétence choisie.</summary>
        Target
    }

    /// <summary>Carte monstre en combat : nom, illustration, PV, effets en cours et compétences. Référence : docs/combat.md (La carte monstre).</summary>
    public class MonsterCardView : MonoBehaviour
    {
        [SerializeField] private Button   _button;
        [SerializeField] private Image    _highlight;
        [SerializeField] private Image    _frame;
        [SerializeField] private Image    _artwork;
        [SerializeField] private TMP_Text _nameLabel;
        [SerializeField] private TMP_Text _hpLabel;
        [SerializeField] private TMP_Text _statusLabel;
        [SerializeField] private SkillRowView[] _skillRows;

        [Header("Couleurs")]
        [SerializeField] private Color _playerFrame    = new(0.16f, 0.26f, 0.40f);
        [SerializeField] private Color _enemyFrame     = new(0.42f, 0.17f, 0.17f);
        [SerializeField] private Color _selectedColor  = new(1.00f, 0.82f, 0.25f);
        [SerializeField] private Color _targetColor    = new(1.00f, 0.40f, 0.30f);
        [SerializeField] private Color _noArtworkColor = new(0.10f, 0.10f, 0.12f);

        public CombatMonster Monster { get; private set; }

        public event Action<MonsterCardView> Clicked;
        public event Action<MonsterCardView, int> SkillClicked;

        private void Awake()
        {
            _button.onClick.AddListener(() => Clicked?.Invoke(this));
            foreach (var row in _skillRows)
                row.Clicked += r => SkillClicked?.Invoke(this, r.SkillIndex);
        }

        public void Bind(CombatMonster monster)
        {
            Monster = monster;
            _nameLabel.text = monster.Name;
            _frame.color = monster.Side == Side.Player ? _playerFrame : _enemyFrame;

            var sprite = (monster.Origin as MonsterCardData)?.Artwork
                      ?? (monster.Origin as OwnedMonster)?.Origin.Artwork;
            _artwork.sprite = sprite;
            _artwork.color  = sprite != null ? Color.white : _noArtworkColor;
        }

        /// <param name="canUseSkill">Dit si une compétence peut être utilisée maintenant par le joueur.</param>
        /// <param name="selectedSkill">Index de la compétence en cours de ciblage, ou -1.</param>
        public void Refresh(CardHighlight highlight, Func<int, bool> canUseSkill, int selectedSkill)
        {
            var m = Monster;
            _hpLabel.text = $"PV {m.Hp} / {m.MaxHp}";
            _statusLabel.text = StatusText(m);

            _highlight.enabled = highlight != CardHighlight.None;
            _highlight.color   = highlight == CardHighlight.Target ? _targetColor : _selectedColor;

            for (int i = 0; i < _skillRows.Length; i++)
            {
                if (i >= m.Skills.Count) { _skillRows[i].Hide(); continue; }
                _skillRows[i].Bind(i, m.Skills[i], m.GetCooldown(i), m.WasUsedThisPhase(i), canUseSkill(i), i == selectedSkill);
            }
        }

        private static string StatusText(CombatMonster m)
        {
            var parts = new List<string>();
            if (m.DamageBonus > 0)     parts.Add($"+{m.DamageBonus} dégât{(m.DamageBonus > 1 ? "s" : "")} infligé{(m.DamageBonus > 1 ? "s" : "")}");
            if (m.DamageReduction > 0) parts.Add($"-{m.DamageReduction} dégât{(m.DamageReduction > 1 ? "s" : "")} subi{(m.DamageReduction > 1 ? "s" : "")}");
            return string.Join("  ·  ", parts);
        }
    }
}
