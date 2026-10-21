using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MonsterCardGame.Core;
using MonsterCardGame.Gameplay.Combat;
using MonsterCardGame.Gameplay.Combat.AI;
using MonsterCardGame.Gameplay.Encounters;
using MonsterCardGame.Gameplay.Monsters;

namespace MonsterCardGame.UI.Combat
{
    /// <summary>
    /// Écran de combat : affiche le moteur et transmet les clics du joueur.
    /// Mise en place : choisir une carte de la main, puis un emplacement. Pendant sa phase : poser de la même façon,
    /// puis cliquer une compétence, puis sa cible. Échap annule le choix en cours.
    /// </summary>
    public class CombatScreen : MonoBehaviour
    {
        [Header("Combat (en attendant le branchement au monde, étape 8)")]
        [SerializeField] private List<MonsterCardData> _playerTeam = new();
        [SerializeField] private EncounterData _encounter;
        [SerializeField, Tooltip("0 = au hasard")] private int _seed;
        [SerializeField, Tooltip("Pause entre les étapes de la phase adverse, en secondes")] private float _enemyDelay = 0.8f;

        [Header("Vues")]
        [SerializeField] private MonsterCardView _cardPrefab;
        [SerializeField] private SlotView[] _playerSlots;
        [SerializeField] private SlotView[] _enemySlots;
        [SerializeField] private RectTransform _playerHand;
        [SerializeField] private RectTransform _hiddenCards;

        [Header("Informations")]
        [SerializeField] private TMP_Text _scoreLabel;
        [SerializeField] private TMP_Text _turnLabel;
        [SerializeField] private TMP_Text _fatigueLabel;
        [SerializeField] private TMP_Text _enemyHandLabel;
        [SerializeField] private TMP_Text _hintLabel;
        [SerializeField] private CombatLog _log;

        [Header("Boutons et fin du combat")]
        [SerializeField] private Button   _actionButton;
        [SerializeField] private TMP_Text _actionButtonLabel;
        [SerializeField] private GameObject _endPanel;
        [SerializeField] private TMP_Text _endTitle;
        [SerializeField] private TMP_Text _endReason;
        [SerializeField] private Button   _replayButton;

        private CombatEngine _engine;
        private CombatAI _enemyAI;
        private readonly Dictionary<CombatMonster, MonsterCardView> _views = new();

        /// <summary>Poses choisies pendant la mise en place, révélées toutes ensemble.</summary>
        private readonly Dictionary<CombatMonster, int> _staged = new();

        private CombatMonster _selected;
        private int _selectedSkill = -1;
        private bool _enemyPlaying;

        private bool IsPlayerTurn => _engine.State == CombatState.Phase && _engine.CurrentSide == Side.Player && !_enemyPlaying;
        private bool IsTargeting  => _selected != null && _selectedSkill >= 0;

        private void Start()
        {
            var random = _seed != 0 ? new System.Random(_seed) : new System.Random();
            var player = _playerTeam.Where(m => m != null).Select(CombatMonster.From).ToList();
            var enemy  = _encounter.Monsters.Where(m => m != null).Select(CombatMonster.From).ToList();

            _engine  = new CombatEngine(player, enemy, new System.Random(random.Next()));
            _enemyAI = new CombatAI(new System.Random(random.Next()));
            _log.Listen(_engine);
            _engine.CombatEnded += _ => ClearSelection();

            foreach (var monster in player.Concat(enemy))
            {
                var view = Instantiate(_cardPrefab, _hiddenCards);
                view.name = monster.Name;
                view.Bind(monster);
                view.Clicked += OnCardClicked;
                view.SkillClicked += OnSkillClicked;
                _views[monster] = view;
            }

            foreach (var slot in _playerSlots.Concat(_enemySlots))
                slot.Clicked += OnSlotClicked;

            _actionButton.onClick.AddListener(OnActionButton);
            _replayButton.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().name));
            _endPanel.SetActive(false);

            Refresh();
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame && _selected != null)
            {
                ClearSelection();
                Refresh();
            }
        }

        // ── Clics ─────────────────────────────────────────────────────────

        private void OnCardClicked(MonsterCardView view)
        {
            var monster = view.Monster;

            if (_engine.State == CombatState.Setup)
            {
                if (monster.Side != Side.Player) return;
                if (_staged.Remove(monster)) ClearSelection();
                else ToggleSelection(monster);
            }
            else if (IsTargeting)
            {
                if (_engine.GetValidTargets(_selected, _selectedSkill).Contains(monster))
                {
                    var (user, skill) = (_selected, _selectedSkill);
                    ClearSelection();
                    _engine.UseSkill(user, skill, monster);
                }
                else ClearSelection();
            }
            else if (IsPlayerTurn && monster.Side == Side.Player && monster.Zone == MonsterZone.Hand)
            {
                ToggleSelection(monster);
            }

            Refresh();
        }

        private void OnSlotClicked(SlotView slot)
        {
            if (_selected == null || IsTargeting || slot.Side != Side.Player) return;

            if (_engine.State == CombatState.Setup)
            {
                if (!_staged.ContainsValue(slot.Index))
                {
                    _staged[_selected] = slot.Index;
                    ClearSelection();
                }
            }
            else if (_engine.CanPlace(_selected, slot.Index))
            {
                _engine.Place(_selected, slot.Index);
                ClearSelection();
            }

            Refresh();
        }

        private void OnSkillClicked(MonsterCardView view, int skillIndex)
        {
            var monster = view.Monster;
            bool sameSkill = IsTargeting && _selected == monster && _selectedSkill == skillIndex;

            // Les compétences couvrent la carte : sinon, le clic vaut pour la carte (sélection, cible)
            if (!sameSkill && (IsTargeting || !IsPlayerTurn || !_engine.CanUseSkill(monster, skillIndex)))
            {
                OnCardClicked(view);
                return;
            }

            if (sameSkill)
            {
                ClearSelection();
            }
            else if (CombatEngine.RequiresTargetChoice(monster.Skills[skillIndex].Target))
            {
                _selected = monster;
                _selectedSkill = skillIndex;
            }
            else
            {
                ClearSelection();
                _engine.UseSkill(monster, skillIndex);
            }

            Refresh();
        }

        private void OnActionButton()
        {
            ClearSelection();

            if (_engine.State == CombatState.Setup)
            {
                var playerSetup = _staged.Select(p => (p.Key, p.Value)).ToList();
                _engine.Setup(playerSetup, _enemyAI.ChooseSetup(_engine, Side.Enemy));
                _staged.Clear();
            }
            else if (IsPlayerTurn)
            {
                _engine.EndPhase();
                if (_engine.State == CombatState.Phase && _engine.CurrentSide == Side.Enemy)
                    StartCoroutine(PlayEnemyPhase());
            }

            Refresh();
        }

        private IEnumerator PlayEnemyPhase()
        {
            _enemyPlaying = true;
            Refresh();

            yield return new WaitForSeconds(_enemyDelay);
            _enemyAI.PlayPhase(_engine);
            Refresh();

            yield return new WaitForSeconds(_enemyDelay);
            if (_engine.State == CombatState.Phase) _engine.EndPhase();

            _enemyPlaying = false;
            Refresh();
        }

        private void ToggleSelection(CombatMonster monster)
        {
            bool same = _selected == monster && !IsTargeting;
            ClearSelection();
            if (!same) _selected = monster;
        }

        private void ClearSelection()
        {
            _selected = null;
            _selectedSkill = -1;
        }

        // ── Affichage ─────────────────────────────────────────────────────

        private void Refresh()
        {
            var targets = IsTargeting ? _engine.GetValidTargets(_selected, _selectedSkill) : new List<CombatMonster>();

            foreach (var (monster, view) in _views)
            {
                PlaceView(monster, view);

                var highlight = targets.Contains(monster) ? CardHighlight.Target
                              : monster == _selected      ? CardHighlight.Selected
                              : CardHighlight.None;
                int selectedSkill = monster == _selected ? _selectedSkill : -1;
                view.Refresh(highlight, i => IsPlayerTurn && monster.Side == Side.Player && _engine.CanUseSkill(monster, i), selectedSkill);
            }

            RefreshSlots(_playerSlots, Side.Player);
            RefreshSlots(_enemySlots, Side.Enemy);
            RefreshInfo();
        }

        private void PlaceView(CombatMonster monster, MonsterCardView view)
        {
            Transform parent;
            if (_staged.TryGetValue(monster, out int stagedSlot))      parent = _playerSlots[stagedSlot].CardAnchor;
            else if (monster.Zone == MonsterZone.Field)                 parent = SlotsOf(monster.Side)[monster.Slot].CardAnchor;
            else if (monster.Zone == MonsterZone.Hand && monster.Side == Side.Player) parent = _playerHand;
            else                                                        parent = _hiddenCards;

            if (view.transform.parent != parent)
            {
                view.transform.SetParent(parent, false);
                var rect = (RectTransform)view.transform;
                rect.anchoredPosition = Vector2.zero;
            }
            view.gameObject.SetActive(monster.Zone != MonsterZone.Defeated);
        }

        private SlotView[] SlotsOf(Side side) => side == Side.Player ? _playerSlots : _enemySlots;

        private void RefreshSlots(SlotView[] slots, Side side)
        {
            foreach (var slot in slots)
            {
                bool placeable = side == Side.Player && _selected != null && !IsTargeting &&
                                 (_engine.State == CombatState.Setup
                                     ? !_staged.ContainsValue(slot.Index)
                                     : _engine.CanPlace(_selected, slot.Index));
                int bonus = _engine.PointsForDefeatAt(side, slot.Index) - 1;
                slot.Refresh(placeable, bonus);
            }
        }

        private void RefreshInfo()
        {
            _scoreLabel.text = $"Vous {_engine.GetPoints(Side.Player)}  —  {_engine.GetPoints(Side.Enemy)} Adversaire\n" +
                               $"<size=70%>{GameRules.PointsToWin} points pour gagner</size>";

            int enemyHand = _engine.GetHand(Side.Enemy).Count;
            _enemyHandLabel.text = $"Main adverse : {enemyHand} monstre{(enemyHand > 1 ? "s" : "")}";

            _turnLabel.text = _engine.State switch
            {
                CombatState.Setup => "Mise en place",
                CombatState.Ended => $"Tour {_engine.Turn} — combat terminé",
                _ => $"Tour {_engine.Turn} — {(_engine.CurrentSide == Side.Player ? "votre phase" : "phase de l'adversaire")}"
            };

            _fatigueLabel.text = FatigueText();
            _hintLabel.text = HintText();

            _actionButton.gameObject.SetActive(_engine.State != CombatState.Ended);
            _actionButton.interactable = _engine.State == CombatState.Setup || IsPlayerTurn;
            _actionButtonLabel.text = _engine.State == CombatState.Setup ? "Révéler les poses" : "Fin de phase";

            if (_engine.State == CombatState.Ended && !_endPanel.activeSelf)
            {
                _endPanel.SetActive(true);
                bool won = _engine.Winner == Side.Player;
                _endTitle.text = won ? "Victoire" : "Défaite";
                _endReason.text = _engine.EndReason switch
                {
                    CombatEndReason.Points         => won ? "Vous avez marqué 3 points." : "L'adversaire a marqué 3 points.",
                    CombatEndReason.NoMonstersLeft => won ? "L'adversaire n'a plus de monstre." : "Vous n'avez plus de monstre.",
                    _                              => "Les deux camps ont perdu leur dernier monstre en même temps."
                };
            }
        }

        private string FatigueText()
        {
            int turn = _engine.Turn;
            int now  = CombatEngine.FatigueDamage(turn);
            if (now > 0) return $"Fatigue : {now} dégât{(now > 1 ? "s" : "")} ce tour, {now + 1} au prochain";

            int left = GameRules.FatigueStartTurn - turn;
            return turn > 0 && left <= 3 ? $"Fatigue dans {left} tour{(left > 1 ? "s" : "")}" : "";
        }

        private string HintText()
        {
            if (_engine.State == CombatState.Setup)
                return _selected != null
                    ? "Choisissez un emplacement libre."
                    : "Choisissez en secret les monstres à poser : une carte de votre main, puis un emplacement. Cliquez une carte posée pour la reprendre.";
            if (_engine.State == CombatState.Ended) return "";
            if (_enemyPlaying || _engine.CurrentSide == Side.Enemy) return "L'adversaire joue…";
            if (IsTargeting) return $"Choisissez la cible de {_selected.Skills[_selectedSkill].SkillName}. Échap pour annuler.";
            if (_selected != null) return "Choisissez un emplacement libre. Échap pour annuler.";
            return _engine.PlacementLocked
                ? "Utilisez les compétences de vos monstres, puis terminez la phase."
                : "Posez des monstres de votre main, puis utilisez leurs compétences. Une fois une compétence utilisée, on ne peut plus poser.";
        }
    }
}
