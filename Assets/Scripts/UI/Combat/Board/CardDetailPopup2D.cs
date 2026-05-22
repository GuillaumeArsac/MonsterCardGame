using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MonsterCardGame.Gameplay.Cards;
using MonsterCardGame.Gameplay.Combat;

namespace MonsterCardGame.UI.Combat.Board
{
    /// <summary>
    /// Overlay plein-écran (uGUI) affiché au clic droit sur une carte : montre toutes
    /// les infos en grand. Le bouton de fond (_closeButton) ferme la popup ; le panneau
    /// central a son propre Raycast Target pour ne pas la fermer au clic dessus.
    /// </summary>
    public class CardDetailPopup2D : MonoBehaviour
    {
        [Header("Racine")]
        [SerializeField] private GameObject _root;
        [SerializeField] private Button     _closeButton;

        [Header("Contenu")]
        [SerializeField] private Image    _artwork;
        [SerializeField] private TMP_Text _nameLabel;
        [SerializeField] private TMP_Text _typeLabel;
        [SerializeField] private TMP_Text _regionLabel;
        [SerializeField] private TMP_Text _costLabel;
        [SerializeField] private TMP_Text _manaGenLabel;
        [SerializeField] private TMP_Text _statsLabel;
        [SerializeField] private TMP_Text _descLabel;
        [SerializeField] private TMP_Text _keywordsLabel;

        private void Awake()
        {
            if (_closeButton != null) _closeButton.onClick.AddListener(Hide);
            Hide();
        }

        public void Show(AlliedInstance instance) => Show(instance.Data, instance.DEF);

        public void Show(CardData data) => Show(data, data.Defense);

        private void Show(CardData data, int currentDef)
        {
            if (_artwork != null)
            {
                bool hasArt = data.Artwork != null;
                _artwork.gameObject.SetActive(hasArt);
                if (hasArt) _artwork.sprite = data.Artwork;
            }

            if (_nameLabel != null) _nameLabel.text = data.CardName;
            if (_typeLabel != null) _typeLabel.text = GetTypeName(data.CardType);

            if (_regionLabel != null)
            {
                bool hasRegion = data.Region != Region.Aucune;
                _regionLabel.gameObject.SetActive(hasRegion);
                if (hasRegion) _regionLabel.text = $"Région : {GetRegionName(data.Region)}";
            }

            if (_costLabel != null) _costLabel.text = $"Coût : {data.ManaCost} mana";

            if (_manaGenLabel != null)
            {
                bool hasMana = data.ManaGenerated > 0;
                _manaGenLabel.gameObject.SetActive(hasMana);
                if (hasMana) _manaGenLabel.text = $"Sacrifice : +{data.ManaGenerated} mana";
            }

            if (_statsLabel != null) SetStats(data, currentDef);

            if (_descLabel != null)
                _descLabel.text = string.IsNullOrEmpty(data.Description) ? "—" : data.Description;

            if (_keywordsLabel != null)
            {
                var kwText = BuildKeywordsText(data);
                bool hasKw = !string.IsNullOrEmpty(kwText);
                _keywordsLabel.gameObject.SetActive(hasKw);
                if (hasKw) _keywordsLabel.text = kwText;
            }

            if (_root != null) _root.SetActive(true);
        }

        public void Hide()
        {
            if (_root != null) _root.SetActive(false);
        }

        private void SetStats(CardData data, int currentDef)
        {
            string text = data.CardType switch
            {
                CardType.Allie      => $"ATK {data.Attack}   DEF {data.Defense}",
                CardType.Equipement => $"DMG {data.Attack}   USE {currentDef}",
                CardType.Action     => $"DMG {data.Attack}",
                CardType.Blocage    => $"BLK {data.Defense}",
                _                   => null
            };

            _statsLabel.gameObject.SetActive(text != null);
            if (text != null) _statsLabel.text = text;
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private static string GetRegionName(Region region) => region switch
        {
            Region.Montagne => "Montagne",
            Region.Marais   => "Marais",
            Region.Plaines  => "Plaines",
            Region.Ruines   => "Ruines",
            Region.Foret    => "Forêt",
            Region.Ocean    => "Océan",
            _               => ""
        };

        private static string GetTypeName(CardType type) => type switch
        {
            CardType.Allie      => "Allié",
            CardType.Action     => "Action",
            CardType.Blocage    => "Blocage",
            CardType.Equipement => "Équipement",
            _                   => ""
        };

        private static string BuildKeywordsText(CardData data)
        {
            var parts = new List<string>();

            if (data.HasKeyword(Keyword.Vol))         parts.Add("Vol");
            if (data.HasKeyword(Keyword.Portee))      parts.Add("Portée");
            if (data.HasKeyword(Keyword.Instantane))  parts.Add("Instantané");
            if (data.HasKeyword(Keyword.Provocation)) parts.Add("Provocation");
            if (data.HasKeyword(Keyword.Eveille))     parts.Add("Éveillé");
            if (data.HasKeyword(Keyword.Rituel))      parts.Add($"Rituel ({data.RitualValue})");
            if (data.HasKeyword(Keyword.Invincible))  parts.Add("Invincible");
            if (data.HasKeyword(Keyword.Rampant))     parts.Add("Rampant");

            return parts.Count > 0 ? "Mots-clés : " + string.Join(", ", parts) : "";
        }
    }
}
