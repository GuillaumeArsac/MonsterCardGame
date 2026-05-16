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

        private VisualElement _mapArea;
        private VisualElement _zoneArtwork;
        private Label         _zoneNameLabel;
        private Label         _zoneDescLabel;
        private VisualElement _monsterList;
        private Button        _fightBtn;

        private ZoneData      _selectedZone       = null;
        private MonsterData   _selectedMonster    = null;
        private VisualElement _selectedMarker     = null;
        private VisualElement _selectedMonsterRow = null;

        private void OnEnable()
        {
            _progression = Services.Get<IWorldProgressionService>();
            _setup       = Services.Get<ICombatSetupService>();

            var root = GetComponent<UIDocument>().rootVisualElement;

            _mapArea       = root.Q<VisualElement>("map-area");
            _zoneArtwork   = root.Q<VisualElement>("zone-artwork");
            _zoneNameLabel = root.Q<Label>("zone-name-label");
            _zoneDescLabel = root.Q<Label>("zone-desc-label");
            _monsterList   = root.Q<VisualElement>("monster-list");
            _fightBtn      = root.Q<Button>("fight-btn");

            root.Q<Button>("deckbuilder-btn").clicked += () => SceneManager.LoadScene("DeckBuilder");
            root.Q<Button>("forge-btn").clicked       += () => SceneManager.LoadScene("Forge");
            _fightBtn.clicked                         += OnFightClicked;
        }

        private void Start()
        {
            BuildZoneMarkers();
            _fightBtn.SetEnabled(false);
        }

        // ── Marqueurs de zones ────────────────────────────────────────────

        private void BuildZoneMarkers()
        {
            _mapArea.Clear();

            if (_progression == null || _progression.AllZones.Count == 0)
                return;

            foreach (var zone in _progression.AllZones)
            {
                var z      = zone;
                bool locked = !_progression.IsZoneUnlocked(zone);

                var marker = new VisualElement();
                marker.AddToClassList("zone-marker");
                if (locked) marker.AddToClassList("zone-marker--locked");

                // Positionne le marqueur sur la carte (0–1 → %)
                // Le translate CSS (-50% -50%) n'étant pas disponible en USS,
                // on décale manuellement de la moitié de la largeur du marqueur (90px/2 = 45px).
                marker.style.left      = Length.Percent(zone.MapPosition.x * 100f);
                marker.style.top       = Length.Percent(zone.MapPosition.y * 100f);
                marker.style.marginLeft = -45;
                marker.style.marginTop  = -12;

                var dot = new VisualElement();
                dot.AddToClassList("zone-marker-dot");
                marker.Add(dot);

                var label = new Label(zone.ZoneName);
                label.AddToClassList("zone-marker-label");
                marker.Add(label);

                if (!locked)
                {
                    marker.RegisterCallback<PointerDownEvent>(evt =>
                    {
                        if (evt.button == 0) SelectZone(z, marker);
                    });
                }

                _mapArea.Add(marker);
            }
        }

        // ── Sélection d'une zone ──────────────────────────────────────────

        private void SelectZone(ZoneData zone, VisualElement marker)
        {
            _selectedMarker?.RemoveFromClassList("zone-marker--selected");
            _selectedZone   = zone;
            _selectedMarker = marker;
            marker.AddToClassList("zone-marker--selected");

            _zoneNameLabel.text = zone.ZoneName;
            _zoneDescLabel.text = zone.Description;

            _zoneArtwork.style.backgroundImage = zone.Artwork != null
                ? new StyleBackground(zone.Artwork)
                : StyleKeyword.None;

            ClearMonsterSelection();
            BuildMonsterList(zone);
        }

        // ── Liste des monstres ────────────────────────────────────────────

        private void BuildMonsterList(ZoneData zone)
        {
            _monsterList.Clear();

            foreach (var monster in zone.Monsters)
                _monsterList.Add(CreateMonsterRow(monster, isBoss: false));

            if (zone.Boss != null)
                _monsterList.Add(CreateMonsterRow(zone.Boss, isBoss: true));
        }

        private VisualElement CreateMonsterRow(MonsterData monster, bool isBoss)
        {
            var m        = monster;
            bool defeated = _progression?.IsMonsterDefeated(monster) ?? false;

            var row = new VisualElement();
            row.AddToClassList("monster-row");
            if (isBoss) row.AddToClassList("monster-row--boss");

            // Portrait à gauche
            var portrait = new VisualElement();
            portrait.AddToClassList("monster-portrait");
            if (monster.Portrait != null)
                portrait.style.backgroundImage = new StyleBackground(monster.Portrait);
            row.Add(portrait);

            // Infos à droite
            var info = new VisualElement();
            info.AddToClassList("monster-info");

            var header = new VisualElement();
            header.AddToClassList("monster-row-header");

            var nameLabel = new Label(monster.MonsterName);
            nameLabel.AddToClassList("monster-row-name");
            header.Add(nameLabel);

            if (isBoss)
            {
                var bossBadge = new Label("BOSS");
                bossBadge.AddToClassList("monster-boss-badge");
                header.Add(bossBadge);
            }

            if (defeated)
            {
                var defeatedBadge = new Label("✓ Vaincu");
                defeatedBadge.AddToClassList("monster-defeated-badge");
                header.Add(defeatedBadge);
            }

            info.Add(header);

            var hpLabel = new Label($"PV : {monster.StartingHP}");
            hpLabel.AddToClassList("monster-hp-label");
            info.Add(hpLabel);

            row.Add(info);

            row.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button == 0) SelectMonster(m, row);
            });

            return row;
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
