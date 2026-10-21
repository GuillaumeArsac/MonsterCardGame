using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MonsterCardGame.Gameplay.Skills;

namespace MonsterCardGame.UI.Combat
{
    /// <summary>Ligne d'une compétence sur une carte : nom et compteur de recharge. Cliquable pour l'utiliser.</summary>
    public class SkillRowView : MonoBehaviour
    {
        [SerializeField] private Button   _button;
        [SerializeField] private Image    _background;
        [SerializeField] private TMP_Text _nameLabel;
        [SerializeField] private TMP_Text _counterLabel;

        [Header("Couleurs")]
        [SerializeField] private Color _readyColor    = new(0.22f, 0.42f, 0.26f);
        [SerializeField] private Color _waitingColor  = new(0.18f, 0.18f, 0.22f);
        [SerializeField] private Color _passiveColor  = new(0.20f, 0.24f, 0.34f);
        [SerializeField] private Color _selectedColor = new(0.85f, 0.65f, 0.15f);

        /// <summary>Index de la compétence dans le monstre.</summary>
        public int SkillIndex { get; private set; }

        public event Action<SkillRowView> Clicked;

        private void Awake() => _button.onClick.AddListener(() => Clicked?.Invoke(this));

        public void Bind(int skillIndex, SkillData skill, int cooldown, bool usedThisPhase, bool usable, bool selected)
        {
            SkillIndex = skillIndex;
            gameObject.SetActive(true);

            _nameLabel.text = skill.SkillName;
            _counterLabel.text = skill.IsPassive ? "passive"
                               : usedThisPhase   ? "utilisée"
                               : cooldown > 0    ? $"recharge {cooldown}"
                               : "prête";

            // Le bouton reste actif : un clic sur une compétence inutilisable compte comme un clic sur la carte
            _background.color = selected        ? _selectedColor
                              : skill.IsPassive ? _passiveColor
                              : usable          ? _readyColor
                              : _waitingColor;
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
