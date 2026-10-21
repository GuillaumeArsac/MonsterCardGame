using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MonsterCardGame.Core;
using MonsterCardGame.Core.Services;
using MonsterCardGame.Gameplay.Content;
using MonsterCardGame.Gameplay.Encounters;
using MonsterCardGame.Gameplay.Monsters;
using MonsterCardGame.Gameplay.World;
using MonsterCardGame.UI.Combat;

namespace MonsterCardGame.UI.Progression
{
    /// <summary>
    /// Composition de l'équipe : le joueur choisit 5 monstres de sa collection. Cliquer une carte l'ajoute à l'équipe
    /// ou l'en retire. En attendant le branchement au monde (étape 8), l'écran lance aussi les combats.
    /// Référence : docs/progression.md (Collection).
    /// </summary>
    public class TeamScreen : MonoBehaviour
    {
        [SerializeField] private MonsterCardView _cardPrefab;
        [SerializeField] private RectTransform _grid;
        [SerializeField] private TMP_Text _teamLabel;
        [SerializeField] private TMP_Text _hintLabel;
        [SerializeField, Tooltip("Bouton copié pour chaque combat")] private Button _encounterButtonTemplate;
        [SerializeField] private Button _worldMapButton;

        private MonsterCollection _collection;
        private ISaveService _save;
        private readonly List<MonsterCardView> _views = new();
        private readonly List<Button> _encounterButtons = new();

        private void Start()
        {
            _collection = Services.Get<MonsterCollection>();
            _save = Services.Get<ISaveService>();

            foreach (var monster in _collection.Monsters)
            {
                var view = Instantiate(_cardPrefab, _grid);
                view.name = monster.Name;
                view.Clicked += v => Toggle(v.Owned);
                view.SkillClicked += (v, _) => Toggle(v.Owned);
                _views.Add(view);
            }

            BuildEncounterButtons();
            _worldMapButton.onClick.AddListener(() => SceneManager.LoadScene(SceneNames.WorldMap));
            Refresh();
        }

        private void BuildEncounterButtons()
        {
            _encounterButtonTemplate.gameObject.SetActive(false);
            if (!Services.Has<ContentDatabase>()) return;

            var progression = Services.Get<IWorldProgressionService>();
            foreach (var encounter in Services.Get<ContentDatabase>().Encounters)
            {
                var button = Instantiate(_encounterButtonTemplate, _encounterButtonTemplate.transform.parent);
                button.gameObject.SetActive(true);
                bool won = progression.IsEncounterCompleted(encounter.EncounterId);
                button.GetComponentInChildren<TMP_Text>().text = won ? $"{encounter.DisplayName} (gagné)" : encounter.DisplayName;

                var e = encounter;
                button.onClick.AddListener(() =>
                {
                    Services.Get<CombatSession>().Begin(e, SceneNames.Team);
                    SceneManager.LoadScene(SceneNames.Combat);
                });
                _encounterButtons.Add(button);
            }
        }

        private void Toggle(OwnedMonster monster)
        {
            bool changed = _collection.IsInTeam(monster) ? _collection.RemoveFromTeam(monster) : _collection.AddToTeam(monster);
            if (changed) _save.Save();
            Refresh();
        }

        private void Refresh()
        {
            for (int i = 0; i < _views.Count; i++)
            {
                var monster = _collection.Monsters[i];
                _views[i].ShowOwned(monster, _collection.IsInTeam(monster) ? CardHighlight.Selected : CardHighlight.None);
            }

            int count = _collection.Team.Count;
            _teamLabel.text = $"Équipe : {count} / {GameRules.TeamSize}";
            _hintLabel.text = _collection.IsTeamComplete
                ? "Votre équipe est prête. Cliquez un monstre encadré pour le retirer, puis un autre pour le remplacer."
                : $"Choisissez encore {GameRules.TeamSize - count} monstre{(GameRules.TeamSize - count > 1 ? "s" : "")} : cliquez une carte pour l'ajouter à l'équipe.";

            foreach (var button in _encounterButtons)
                button.interactable = _collection.IsTeamComplete;
        }
    }
}
