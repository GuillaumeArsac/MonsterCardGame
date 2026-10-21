using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MonsterCardGame.Gameplay.Combat;
using MonsterCardGame.Gameplay.Monsters;
using MonsterCardGame.Gameplay.Skills;

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
    public class MonsterCardView : MonoBehaviour, IPointerClickHandler
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

        /// <summary>Monstre affiché en combat. Null hors combat.</summary>
        public CombatMonster Monster { get; private set; }
        /// <summary>Monstre du joueur affiché hors combat (collection, dépeçage).</summary>
        public OwnedMonster Owned { get; private set; }
        /// <summary>Carte affichée hors combat (monstre adverse dépecé).</summary>
        public MonsterCardData Card { get; private set; }

        public event Action<MonsterCardView> Clicked;
        public event Action<MonsterCardView, int> SkillClicked;

        /// <summary>Nom affiché, quel que soit le monstre montré.</summary>
        public string DisplayName => Monster?.Name ?? Owned?.Name ?? (Card != null ? Card.MonsterName : "");

        /// <summary>Compétences affichées, quel que soit le monstre montré.</summary>
        public IReadOnlyList<SkillData> DisplayedSkills
            => Monster?.Skills ?? Owned?.Skills ?? (Card != null ? Card.Skills : Array.Empty<SkillData>());

        /// <summary>Vrai pour la compétence unique d'un monstre de quête.</summary>
        public bool IsSkillLocked(int skillIndex)
            => Owned != null ? Owned.IsLocked(skillIndex)
             : Card != null && skillIndex < Card.Skills.Count && Card.Skills[skillIndex] == Card.UniqueSkill;

        private void Awake()
        {
            _button.onClick.AddListener(() => Clicked?.Invoke(this));
            foreach (var row in _skillRows)
            {
                row.Clicked += r => SkillClicked?.Invoke(this, r.SkillIndex);
                row.RightClicked += _ => ShowDetails();
            }
        }

        /// <summary>Clic droit sur la carte : détail de ses compétences. Le clic gauche passe par le bouton.</summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right) ShowDetails();
        }

        private void ShowDetails()
        {
            if (CardDetailsPopup.Current != null) CardDetailsPopup.Current.Show(this);
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

        // ── Hors combat ───────────────────────────────────────────────────

        /// <summary>Affiche un monstre du joueur : PV avec le boost, compétences et compétence unique.</summary>
        /// <param name="choosableSkill">Dit si une ligne de compétence peut être choisie.</param>
        public void ShowOwned(OwnedMonster monster, CardHighlight highlight,
                              Func<int, bool> choosableSkill = null, int selectedSkill = -1)
        {
            Owned = monster;
            var card = monster.Origin;
            ShowInfo(monster.Name, card.Artwork, _playerFrame, highlight);
            _hpLabel.text = $"PV {monster.MaxHp}";
            _statusLabel.text = monster.Boost.IsEmpty ? "" : $"Boost {monster.Boost}";

            for (int i = 0; i < _skillRows.Length; i++)
            {
                if (i >= monster.Skills.Count) { _skillRows[i].Hide(); continue; }
                _skillRows[i].BindInfo(i, monster.Skills[i], monster.IsLocked(i), choosableSkill?.Invoke(i) ?? false, i == selectedSkill);
            }
        }

        /// <summary>Affiche une carte telle quelle (monstre adverse), avec son boost de dépeçage.</summary>
        public void ShowCard(MonsterCardData card, CardHighlight highlight, int selectedSkill = -1)
        {
            Card = card;
            ShowInfo(card.MonsterName, card.Artwork, _enemyFrame, highlight);
            _hpLabel.text = $"PV {card.MaxHp}";
            _statusLabel.text = card.StatBoost.IsEmpty ? "" : $"Boost : {card.StatBoost}";

            for (int i = 0; i < _skillRows.Length; i++)
            {
                if (i >= card.Skills.Count) { _skillRows[i].Hide(); continue; }
                _skillRows[i].BindInfo(i, card.Skills[i], card.Skills[i] == card.UniqueSkill, false, i == selectedSkill);
            }
        }

        private void ShowInfo(string monsterName, Sprite artwork, Color frame, CardHighlight highlight)
        {
            _nameLabel.text = monsterName;
            _frame.color    = frame;
            _artwork.sprite = artwork;
            _artwork.color  = artwork != null ? Color.white : _noArtworkColor;
            _highlight.enabled = highlight != CardHighlight.None;
            _highlight.color   = highlight == CardHighlight.Target ? _targetColor : _selectedColor;
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
