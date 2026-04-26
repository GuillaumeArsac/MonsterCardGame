using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using MonsterCardGame.Core.Services;
using MonsterCardGame.Gameplay.Combat.Data;
using MonsterCardGame.Gameplay.World;

namespace MonsterCardGame.UI.WorldMap
{
    [RequireComponent(typeof(UIDocument))]
    public class WorldMapUI : MonoBehaviour
    {
        private IWorldProgressionService _progression;
        private ICombatSetupService      _setup;

        private VisualElement _zoneList;
        private VisualElement _monsterList;
        private Label         _zoneNameLabel;
        private Label         _zoneDescLabel;
        private Button        _fightBtn;

        private ZoneData    _selectedZone    = null;
        private MonsterData _selectedMonster = null;
        private VisualElement _selectedZoneRow    = null;
        private VisualElement _selectedMonsterRow = null;

        private void OnEnable()
        {
            _progression = Services.Get<IWorldProgressionService>();
            _setup       = Services.Get<ICombatSetupService>();

            var root = GetComponent<UIDocument>().rootVisualElement;

            _zoneList      = root.Q<VisualElement>("zone-list");
            _monsterList   = root.Q<VisualElement>("monster-list");
            _zoneNameLabel = root.Q<Label>("zone-name-label");
            _zoneDescLabel = root.Q<Label>("zone-desc-label");
            _fightBtn      = root.Q<Button>("fight-btn");

            root.Q<Button>("deckbuilder-btn").clicked += () => SceneManager.LoadScene("DeckBuilder");
            root.Q<Button>("forge-btn").clicked       += () => SceneManager.LoadScene("Forge");
            _fightBtn.clicked                         += OnFightClicked;
        }

        private void Start()
        {
            BuildZoneList();
            _fightBtn.SetEnabled(false);
        }

        // ── Construction de la liste des zones ────────────────────────────

        private void BuildZoneList()
        {
            _zoneList.Clear();

            if (_progression == null || _progression.AllZones.Count == 0)
            {
                var empty = new Label("Aucune zone configurée.");
                empty.AddToClassList("zone-desc");
                _zoneList.Add(empty);
                return;
            }

            foreach (var zone in _progression.AllZones)
            {
                var z       = zone;
                bool locked = !_progression.IsZoneUnlocked(zone);

                var row = new VisualElement();
                row.AddToClassList("zone-row");
                if (locked) row.AddToClassList("zone-row--locked");

                var nameLabel = new Label(zone.ZoneName);
                nameLabel.AddToClassList("zone-row-name");
                row.Add(nameLabel);

                if (locked)
                {
                    var lockLabel = new Label("Vaincre tous les boss");
                    lockLabel.AddToClassList("zone-lock-label");
                    row.Add(lockLabel);
                }

                if (!locked)
                {
                    row.RegisterCallback<PointerDownEvent>(evt =>
                    {
                        if (evt.button == 0) SelectZone(z, row);
                    });
                }

                _zoneList.Add(row);
            }
        }

        // ── Sélection d'une zone ──────────────────────────────────────────

        private void SelectZone(ZoneData zone, VisualElement row)
        {
            _selectedZoneRow?.RemoveFromClassList("zone-row--selected");
            _selectedZone    = zone;
            _selectedZoneRow = row;
            row.AddToClassList("zone-row--selected");

            _zoneNameLabel.text = zone.ZoneName;
            _zoneDescLabel.text = zone.Description;

            ClearMonsterSelection();
            BuildMonsterList(zone);
        }

        // ── Construction de la liste des monstres ─────────────────────────

        private void BuildMonsterList(ZoneData zone)
        {
            _monsterList.Clear();

            foreach (var monster in zone.Monsters)
                AddMonsterRow(monster, isBoss: false);

            if (zone.Boss != null)
                AddMonsterRow(zone.Boss, isBoss: true);
        }

        private void AddMonsterRow(MonsterData monster, bool isBoss)
        {
            var m   = monster;
            bool defeated = _progression?.IsMonsterDefeated(monster) ?? false;

            var row = new VisualElement();
            row.AddToClassList("monster-row");
            if (isBoss) row.AddToClassList("monster-row--boss");

            var nameLabel = new Label(monster.MonsterName);
            nameLabel.AddToClassList("monster-row-name");
            row.Add(nameLabel);

            if (isBoss)
            {
                var bossLabel = new Label("BOSS");
                bossLabel.AddToClassList("monster-boss-label");
                row.Add(bossLabel);
            }

            if (defeated)
            {
                var defeatedLabel = new Label("✓ Vaincu");
                defeatedLabel.AddToClassList("monster-defeated-label");
                row.Add(defeatedLabel);
            }

            row.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button == 0) SelectMonster(m, row);
            });

            _monsterList.Add(row);
        }

        // ── Sélection d'un monstre ────────────────────────────────────────

        private void SelectMonster(MonsterData monster, VisualElement row)
        {
            _selectedMonsterRow?.RemoveFromClassList("monster-row--selected");
            _selectedMonster    = monster;
            _selectedMonsterRow = row;
            row.AddToClassList("monster-row--selected");

            _fightBtn.SetEnabled(true);
        }

        private void ClearMonsterSelection()
        {
            _selectedMonsterRow?.RemoveFromClassList("monster-row--selected");
            _selectedMonster    = null;
            _selectedMonsterRow = null;
            _fightBtn.SetEnabled(false);
        }

        // ── Lancement du combat ───────────────────────────────────────────

        private void OnFightClicked()
        {
            if (_selectedZone == null || _selectedMonster == null) return;
            _setup?.Setup(_selectedZone, _selectedMonster);
            SceneManager.LoadScene("Combat");
        }
    }
}
