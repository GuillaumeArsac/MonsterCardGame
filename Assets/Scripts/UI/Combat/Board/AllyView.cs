using UnityEngine;
using MonsterCardGame.Gameplay.Combat;

namespace MonsterCardGame.UI.Combat.Board
{
    /// <summary>
    /// Carte d'allié posée sur le terrain (prefab 2D). Affiche les données via
    /// <see cref="CardView2D"/> et expose les états visuels du combat
    /// (ciblable, attaquant sélectionné, endormi). Aucune règle de jeu ici :
    /// les clics sont résolus par <see cref="AlliesController"/> via raycast.
    /// </summary>
    [RequireComponent(typeof(CardView2D))]
    public class AllyView : MonoBehaviour
    {
        [SerializeField] private CardView2D _view;

        [Header("États visuels (optionnels)")]
        [SerializeField] private GameObject _targetableHighlight;
        [SerializeField] private GameObject _attackerHighlight;
        [SerializeField] private GameObject _sleepingOverlay;

        public AlliedInstance Instance { get; private set; }

        private void Reset() => _view = GetComponent<CardView2D>();
        private void Awake()
        {
            if (_view == null) _view = GetComponent<CardView2D>();
        }

        public void Bind(AlliedInstance instance)
        {
            Instance = instance;
            _view.Bind(instance.Data);
            _view.RefreshInstance(instance);
            SetTargetable(false);
            SetAttacker(false);
            SetSleeping(instance.IsSleeping);
        }

        /// <summary>Met à jour les données runtime (charges, sommeil) sans réinstancier.</summary>
        public void Refresh()
        {
            if (Instance == null) return;
            _view.RefreshInstance(Instance);
            SetSleeping(Instance.IsSleeping);
        }

        public void SetTargetable(bool on)
        {
            if (_targetableHighlight != null) _targetableHighlight.SetActive(on);
        }

        public void SetAttacker(bool on)
        {
            if (_attackerHighlight != null) _attackerHighlight.SetActive(on);
        }

        public void SetSleeping(bool on)
        {
            if (_sleepingOverlay != null) _sleepingOverlay.SetActive(on);
        }
    }
}
