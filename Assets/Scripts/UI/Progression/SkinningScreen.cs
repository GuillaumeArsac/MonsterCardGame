using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MonsterCardGame.Core.Services;
using MonsterCardGame.Gameplay.Content;
using MonsterCardGame.Gameplay.Encounters;
using MonsterCardGame.Gameplay.Monsters;
using MonsterCardGame.Gameplay.Skills;
using MonsterCardGame.Gameplay.World;
using MonsterCardGame.UI.Combat;

namespace MonsterCardGame.UI.Progression
{
    /// <summary>
    /// Dépeçage après une victoire : le joueur choisit une compétence ou le boost du monstre tiré,
    /// puis le monstre qui la reçoit, puis la compétence remplacée si ce monstre en a déjà 3.
    /// Référence : docs/progression.md (Dépeçage).
    /// </summary>
    public class SkinningScreen : MonoBehaviour
    {
        [SerializeField] private MonsterCardView _cardPrefab;
        [SerializeField] private RectTransform _enemyCardAnchor;
        [SerializeField] private RectTransform _grid;
        [SerializeField] private TMP_Text _titleLabel;
        [SerializeField] private TMP_Text _hintLabel;
        [SerializeField, Tooltip("Bouton copié pour chaque récompense")] private Button _rewardButtonTemplate;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private TMP_Text _confirmLabel;

        [Header("Couleurs des récompenses")]
        [SerializeField] private Color _rewardColor   = new(0.20f, 0.24f, 0.34f);
        [SerializeField] private Color _selectedColor = new(0.85f, 0.65f, 0.15f);

        private MonsterCollection _collection;
        private CombatSession _session;
        private SkinningOffer _offer;
        private MonsterCardView _enemyCard;
        private readonly List<MonsterCardView> _views = new();
        private readonly List<(Button button, SkillData skill)> _rewardButtons = new();

        /// <summary>Compétence choisie. Null avec le boost.</summary>
        private SkillData _skill;
        private bool _boost;
        private OwnedMonster _target;
        private int _replacedIndex = -1;
        private bool _done;

        private bool HasReward => _skill != null || _boost;
        private bool NeedsReplacement => _skill != null && _target != null && SkinningOffer.NeedsReplacement(_target);
        private bool IsComplete => HasReward && _target != null && (!NeedsReplacement || _replacedIndex >= 0);

        private void Start()
        {
            _collection = Services.Get<MonsterCollection>();
            _session = Services.Get<CombatSession>();
            _offer = _session.Offer ?? TestOffer();

            _enemyCard = Instantiate(_cardPrefab, _enemyCardAnchor);
            ((RectTransform)_enemyCard.transform).anchoredPosition = Vector2.zero;

            foreach (var monster in _collection.Monsters)
            {
                var view = Instantiate(_cardPrefab, _grid);
                view.name = monster.Name;
                view.Clicked += v => ChooseTarget(v.Owned);
                view.SkillClicked += OnSkillClicked;
                _views.Add(view);
            }

            BuildRewardButtons();
            _confirmButton.onClick.AddListener(OnConfirm);
            Refresh();
        }

        /// <summary>Scène ouverte sans combat (test dans l'éditeur) : tirage dans le premier combat du contenu.</summary>
        private static SkinningOffer TestOffer()
        {
            var encounter = Services.Get<ContentDatabase>().Encounters.First();
            Core.GameLog.Warning("SkinningScreen", $"Aucun combat gagné : dépeçage de test dans {encounter.DisplayName}");
            return SkinningOffer.Draw(encounter, new System.Random());
        }

        private void BuildRewardButtons()
        {
            _rewardButtonTemplate.gameObject.SetActive(false);

            foreach (var skill in _offer.Skills)
                AddRewardButton($"Compétence : {skill.SkillName}", skill);
            if (_offer.HasBoost)
                AddRewardButton($"Boost : {_offer.Boost}", null);
        }

        private void AddRewardButton(string text, SkillData skill)
        {
            var button = Instantiate(_rewardButtonTemplate, _rewardButtonTemplate.transform.parent);
            button.gameObject.SetActive(true);
            button.GetComponentInChildren<TMP_Text>().text = text;
            button.onClick.AddListener(() =>
            {
                if (_done) return;
                _skill = skill;
                _boost = skill == null;
                _replacedIndex = -1;
                Refresh();
            });
            _rewardButtons.Add((button, skill));
        }

        private void ChooseTarget(OwnedMonster monster)
        {
            if (_done || !HasReward) return;
            if (_target != monster) _replacedIndex = -1;
            _target = monster;
            Refresh();
        }

        private void OnSkillClicked(MonsterCardView view, int skillIndex)
        {
            if (!_done && NeedsReplacement && view.Owned == _target && !_target.IsLocked(skillIndex))
            {
                _replacedIndex = skillIndex;
                Refresh();
            }
            else ChooseTarget(view.Owned);
        }

        private void OnConfirm()
        {
            if (_done)
            {
                string scene = _session.ReturnScene ?? SceneNames.Team;
                _session.Clear();
                SceneManager.LoadScene(scene);
                return;
            }
            if (!IsComplete) return;

            if (_boost) _offer.GiveBoost(_target);
            else _offer.GiveSkill(_skill, _target, _replacedIndex);

            _done = true;
            _collection.NotifyMonsterChanged();
            Services.Get<ISaveService>().Save();
            Refresh();
        }

        private void Refresh()
        {
            int selectedSkill = _skill != null ? _offer.Monster.Skills.ToList().IndexOf(_skill) : -1;
            _enemyCard.ShowCard(_offer.Monster, CardHighlight.None, selectedSkill);
            _titleLabel.text = $"Dépeçage : {_offer.Monster.MonsterName}";

            foreach (var (button, skill) in _rewardButtons)
            {
                bool selected = skill != null ? skill == _skill : _boost;
                button.image.color = selected ? _selectedColor : _rewardColor;
                button.interactable = !_done;
            }

            for (int i = 0; i < _views.Count; i++)
            {
                var monster = _collection.Monsters[i];
                bool isTarget = monster == _target;
                var highlight = isTarget ? CardHighlight.Selected : HasReward && !_done ? CardHighlight.Target : CardHighlight.None;
                _views[i].ShowOwned(monster, highlight,
                                    s => !_done && isTarget && NeedsReplacement && !monster.IsLocked(s),
                                    isTarget ? _replacedIndex : -1);
            }

            _confirmButton.interactable = _done || IsComplete;
            _confirmLabel.text = _done ? "Continuer" : "Confirmer";
            _hintLabel.text = HintText();
        }

        private string HintText()
        {
            if (_done)
                return _boost ? $"{_target.Name} porte maintenant le boost {_offer.Boost}."
                              : $"{_target.Name} a appris {_skill.SkillName}.";
            if (!HasReward)
                return $"Choisissez une récompense : une compétence de {_offer.Monster.MonsterName}, ou son boost de stats.";
            if (_target == null)
                return _boost ? "Choisissez le monstre qui reçoit le boost. Il remplace le boost qu'il portait."
                              : $"Choisissez le monstre qui apprend {_skill.SkillName}.";
            if (NeedsReplacement && _replacedIndex < 0)
                return $"{_target.Name} a déjà 3 compétences : cliquez celle que {_skill.SkillName} remplace. Elle sera perdue.";
            if (NeedsReplacement)
                return $"{_skill.SkillName} remplacera {_target.Skills[_replacedIndex].SkillName}. Confirmez.";
            if (_boost && !_target.Boost.IsEmpty)
                return $"Le boost {_offer.Boost} remplacera le boost {_target.Boost} de {_target.Name}. Confirmez.";
            return "Confirmez votre choix.";
        }
    }
}
