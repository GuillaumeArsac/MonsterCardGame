using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using MonsterCardGame.Gameplay.Skills;

namespace MonsterCardGame.UI.Combat
{
    /// <summary>
    /// Détail des compétences d'une carte, ouvert par un clic droit sur la carte.
    /// Un clic (gauche ou droit) ou Échap le ferme. Une instance par écran, trouvée par les cartes.
    /// </summary>
    public class CardDetailsPopup : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text   _titleLabel;
        [SerializeField] private TMP_Text   _bodyLabel;

        [Header("Couleurs")]
        [SerializeField] private Color _skillNameColor = new(1f, 0.82f, 0.35f);
        [SerializeField] private Color _summaryColor   = new(0.70f, 0.78f, 0.90f);

        /// <summary>Le panneau de l'écran en cours. Null si l'écran n'en a pas.</summary>
        public static CardDetailsPopup Current { get; private set; }

        public bool IsOpen => _root.activeSelf;

        private void Awake()
        {
            Current = this;
            Close();
        }

        private void OnDestroy()
        {
            if (Current == this) Current = null;
        }

        private void Update()
        {
            if (IsOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                Close();
        }

        public void Show(MonsterCardView card)
        {
            _titleLabel.text = card.DisplayName;

            var text = new StringBuilder();
            var skills = card.DisplayedSkills;
            for (int i = 0; i < skills.Count; i++)
            {
                var skill = skills[i];
                if (i > 0) text.Append("\n\n");
                text.Append($"<b><color=#{ColorUtility.ToHtmlStringRGB(_skillNameColor)}>{skill.SkillName}</color></b>");
                if (card.IsSkillLocked(i)) text.Append("  <size=80%>(compétence unique : ne peut pas être remplacée)</size>");
                text.Append($"\n<size=85%><color=#{ColorUtility.ToHtmlStringRGB(_summaryColor)}>{SkillDescriber.Summary(skill)}</color></size>");
                if (skill.Target != SkillTarget.None && skill.Target != SkillTarget.Self)
                    text.Append($"\n<size=85%>Cible : {SkillDescriber.TargetText(skill.Target)}</size>");
                text.Append($"\n{SkillDescriber.Effect(skill)}");
                var note = SkillDescriber.TargetingNote(skill);
                if (note.Length > 0) text.Append($"\n<size=85%><i>{note}</i></size>");
            }
            if (skills.Count == 0) text.Append("Ce monstre n'a aucune compétence.");

            _bodyLabel.text = text.ToString();
            _root.SetActive(true);
            _root.transform.SetAsLastSibling();
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_root.transform);
        }

        public void Close() => _root.SetActive(false);

        /// <summary>Un clic n'importe où sur le panneau ou le fond le ferme.</summary>
        public void OnPointerClick(PointerEventData eventData) => Close();
    }
}
