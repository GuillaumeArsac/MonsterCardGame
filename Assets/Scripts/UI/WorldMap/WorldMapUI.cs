using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using MonsterCardGame.Core;
using MonsterCardGame.Core.Services;
using MonsterCardGame.Gameplay.Encounters;
using MonsterCardGame.Gameplay.Monsters;
using MonsterCardGame.Gameplay.World;

namespace MonsterCardGame.UI.WorldMap
{
    /// <summary>
    /// Carte du monde : affiche les zones et, pour la zone sélectionnée, ses combats.
    /// Les combats du scénario se gagnent une fois, dans l'ordre ; les combats libres se rejouent.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class WorldMapUI : MonoBehaviour
    {
        private IWorldProgressionService _progression;
        private MonsterCollection _collection;

        private VisualElement _mapArea;
        private VisualElement _zoneArtwork;
        private Label         _zoneNameLabel;
        private Label         _zoneDescLabel;
        private VisualElement _encounterList;
        private Label         _fightStatusLabel;
        private Button        _fightButton;

        private VisualElement _selectedMarker;
        private ZoneData      _selectedZone;
        private EncounterData _selectedEncounter;

        private void OnEnable()
        {
            _progression = Services.Get<IWorldProgressionService>();
            _collection  = Services.Get<MonsterCollection>();

            var root = GetComponent<UIDocument>().rootVisualElement;

            _mapArea          = root.Q<VisualElement>("map-area");
            _zoneArtwork      = root.Q<VisualElement>("zone-artwork");
            _zoneNameLabel    = root.Q<Label>("zone-name-label");
            _zoneDescLabel    = root.Q<Label>("zone-desc-label");
            _encounterList    = root.Q<VisualElement>("encounter-list");
            _fightStatusLabel = root.Q<Label>("fight-status-label");
            _fightButton      = root.Q<Button>("fight-btn");

            root.Q<Button>("team-btn").clicked += () => SceneManager.LoadScene(SceneNames.Team);
            _fightButton.clicked += LaunchSelectedEncounter;
        }

        private void Start()
        {
            BuildZoneMarkers();
            RefreshFight();
        }

        // ── Marqueurs de zones ────────────────────────────────────────────

        private void BuildZoneMarkers()
        {
            _mapArea.Clear();

            if (_progression == null || _progression.AllZones.Count == 0)
                return;

            VisualElement firstUnlocked = null;
            ZoneData firstUnlockedZone = null;

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
                marker.style.left       = Length.Percent(zone.MapPosition.x * 100f);
                marker.style.top        = Length.Percent(zone.MapPosition.y * 100f);
                marker.style.marginLeft = -45;
                marker.style.marginTop  = -12;

                var dot = new VisualElement();
                dot.AddToClassList("zone-marker-dot");
                marker.Add(dot);

                var label = new Label(locked ? $"{zone.ZoneName} (verrouillée)" : zone.ZoneName);
                label.AddToClassList("zone-marker-label");
                marker.Add(label);

                if (!locked)
                {
                    marker.RegisterCallback<PointerDownEvent>(evt =>
                    {
                        if (evt.button == 0) SelectZone(z, marker);
                    });
                    if (firstUnlocked == null) { firstUnlocked = marker; firstUnlockedZone = zone; }
                }

                _mapArea.Add(marker);
            }

            // La première zone ouverte est sélectionnée d'office
            if (firstUnlocked != null) SelectZone(firstUnlockedZone, firstUnlocked);
        }

        // ── Sélection d'une zone ──────────────────────────────────────────

        private void SelectZone(ZoneData zone, VisualElement marker)
        {
            _selectedMarker?.RemoveFromClassList("zone-marker--selected");
            _selectedMarker = marker;
            marker.AddToClassList("zone-marker--selected");

            _selectedZone = zone;
            _zoneNameLabel.text = zone.ZoneName;
            _zoneDescLabel.text = zone.Description;

            _zoneArtwork.style.backgroundImage = zone.Artwork != null
                ? new StyleBackground(zone.Artwork)
                : StyleKeyword.None;

            // Sélectionne d'office le premier combat disponible
            _selectedEncounter = AllEncounters(zone).FirstOrDefault(e => _progression.GetEncounterState(zone, e) == EncounterState.Available);
            BuildEncounterList();
        }

        private static System.Collections.Generic.IEnumerable<EncounterData> AllEncounters(ZoneData zone)
            => zone.ScenarioEncounters.Concat(zone.FreeEncounters).Where(e => e != null);

        // ── Combats de la zone ────────────────────────────────────────────

        private void BuildEncounterList()
        {
            _encounterList.Clear();

            foreach (var encounter in AllEncounters(_selectedZone))
                _encounterList.Add(BuildEncounterRow(encounter));

            if (_encounterList.childCount == 0)
                _encounterList.Add(new Label("Aucun combat dans cette zone pour l'instant.") { name = "no-encounter" });

            RefreshFight();
        }

        private VisualElement BuildEncounterRow(EncounterData encounter)
        {
            var state = _progression.GetEncounterState(_selectedZone, encounter);

            var row = new VisualElement();
            row.AddToClassList("monster-row");
            if (encounter == _selectedZone.BossEncounter) row.AddToClassList("monster-row--boss");
            if (encounter == _selectedEncounter) row.AddToClassList("monster-row--selected");
            if (state != EncounterState.Available) row.AddToClassList("monster-row--unavailable");

            var info = new VisualElement();
            info.AddToClassList("monster-info");
            row.Add(info);

            var header = new VisualElement();
            header.AddToClassList("monster-row-header");
            info.Add(header);

            var name = new Label(encounter.DisplayName);
            name.AddToClassList("monster-row-name");
            header.Add(name);

            if (encounter == _selectedZone.BossEncounter) header.Add(Badge("BOSS", "monster-boss-badge"));
            if (encounter.IsRepeatable) header.Add(Badge("LIBRE", "encounter-free-badge"));
            if (state == EncounterState.Completed) header.Add(Badge("GAGNÉ", "monster-defeated-badge"));
            if (state == EncounterState.Locked) header.Add(Badge("VERROUILLÉ", "encounter-locked-badge"));

            var detail = new Label(DetailText(encounter, state));
            detail.AddToClassList("monster-hp-label");
            info.Add(detail);

            row.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button != 0) return;
                _selectedEncounter = encounter;
                BuildEncounterList();
            });
            return row;
        }

        private static Label Badge(string text, string cssClass)
        {
            var badge = new Label(text);
            badge.AddToClassList(cssClass);
            return badge;
        }

        private string DetailText(EncounterData encounter, EncounterState state) => state switch
        {
            EncounterState.Completed => "Combat du scénario gagné : il ne se rejoue pas.",
            EncounterState.Locked    => LockedText(encounter),
            _ => encounter.IsRepeatable
                ? $"Combat libre, rejouable — {encounter.Monsters.Count} monstres"
                : $"Combat du scénario — {encounter.Monsters.Count} monstres"
        };

        private string LockedText(EncounterData encounter)
        {
            var previous = _selectedZone.ScenarioEncounters
                .TakeWhile(e => e != encounter)
                .FirstOrDefault(e => e != null && !_progression.IsEncounterCompleted(e.EncounterId));
            return previous != null ? $"Gagnez d'abord : {previous.DisplayName}" : "Pas encore disponible.";
        }

        private void RefreshFight()
        {
            bool available = _selectedZone != null && _selectedEncounter != null &&
                             _progression.GetEncounterState(_selectedZone, _selectedEncounter) == EncounterState.Available;
            bool teamReady = _collection != null && _collection.IsTeamComplete;

            _fightButton.SetEnabled(available && teamReady);
            _fightStatusLabel.text = !teamReady
                ? $"Votre équipe doit compter {GameRules.TeamSize} monstres : bouton Équipe."
                : _selectedEncounter == null ? "Choisissez un combat." : "";
        }

        private void LaunchSelectedEncounter()
        {
            if (_selectedEncounter == null || !_fightButton.enabledSelf) return;
            Services.Get<CombatSession>().Begin(_selectedEncounter, SceneNames.WorldMap);
            SceneManager.LoadScene(SceneNames.Combat);
        }
    }
}
