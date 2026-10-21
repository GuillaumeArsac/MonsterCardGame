using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MonsterCardGame.Core;
using MonsterCardGame.Core.Services;
using MonsterCardGame.Gameplay.Monsters;
using MonsterCardGame.Gameplay.World;
using MonsterCardGame.UI.Combat;

namespace MonsterCardGame.UI.Progression
{
    /// <summary>
    /// Composition de l'équipe : le joueur choisit 5 monstres de sa collection. Cliquer une carte l'ajoute à l'équipe
    /// ou l'en retire. Les combats se lancent depuis la carte du monde.
    /// Référence : docs/progression.md (Collection).
    /// </summary>
    public class TeamScreen : MonoBehaviour
    {
        [SerializeField] private MonsterCardView _cardPrefab;
        [SerializeField] private RectTransform _grid;
        [SerializeField] private TMP_Text _teamLabel;
        [SerializeField] private TMP_Text _hintLabel;
        [SerializeField] private Button _worldMapButton;

        private MonsterCollection _collection;
        private ISaveService _save;
        private readonly List<MonsterCardView> _views = new();

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

            _worldMapButton.onClick.AddListener(() => SceneManager.LoadScene(SceneNames.WorldMap));
            Refresh();
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

            // Sans équipe complète, on ne peut pas combattre : le retour à la carte reste possible
            _worldMapButton.GetComponentInChildren<TMP_Text>().text = _collection.IsTeamComplete ? "Carte du monde" : "Carte du monde (équipe incomplète)";
        }
    }
}
