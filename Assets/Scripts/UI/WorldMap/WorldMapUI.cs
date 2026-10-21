using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using MonsterCardGame.Core.Services;
using MonsterCardGame.Gameplay.World;

namespace MonsterCardGame.UI.WorldMap
{
    /// <summary>
    /// Carte du monde : affiche les zones et le détail de la zone sélectionnée.
    /// Les combats de zone seront branchés à l'étape 8 du plan d'implémentation.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class WorldMapUI : MonoBehaviour
    {
        private IWorldProgressionService _progression;

        private VisualElement _mapArea;
        private VisualElement _zoneArtwork;
        private Label         _zoneNameLabel;
        private Label         _zoneDescLabel;

        private VisualElement _selectedMarker = null;

        private void OnEnable()
        {
            _progression = Services.Get<IWorldProgressionService>();

            var root = GetComponent<UIDocument>().rootVisualElement;

            _mapArea       = root.Q<VisualElement>("map-area");
            _zoneArtwork   = root.Q<VisualElement>("zone-artwork");
            _zoneNameLabel = root.Q<Label>("zone-name-label");
            _zoneDescLabel = root.Q<Label>("zone-desc-label");

            root.Q<Button>("team-btn").clicked += () => SceneManager.LoadScene(SceneNames.Team);
        }

        private void Start()
        {
            BuildZoneMarkers();
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
                marker.style.left       = Length.Percent(zone.MapPosition.x * 100f);
                marker.style.top        = Length.Percent(zone.MapPosition.y * 100f);
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
            _selectedMarker = marker;
            marker.AddToClassList("zone-marker--selected");

            _zoneNameLabel.text = zone.ZoneName;
            _zoneDescLabel.text = zone.Description;

            _zoneArtwork.style.backgroundImage = zone.Artwork != null
                ? new StyleBackground(zone.Artwork)
                : StyleKeyword.None;
        }
    }
}
