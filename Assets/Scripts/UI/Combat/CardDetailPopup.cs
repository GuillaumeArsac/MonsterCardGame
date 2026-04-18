using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using MonsterCardGame.Gameplay.Cards;

namespace MonsterCardGame.UI.Combat
{
    /// <summary>
    /// Overlay plein-écran affiché au clic droit sur une carte.
    /// Montre toutes les informations de la carte en grand format.
    /// Cliquer n'importe où ferme la popup.
    /// </summary>
    public class CardDetailPopup : VisualElement
    {
        private readonly VisualElement _artwork;
        private readonly Label _nameLabel;
        private readonly Label _typeLabel;
        private readonly Label _regionLabel;
        private readonly Label _costLabel;
        private readonly Label _manaGenLabel;
        private readonly Label _statsLabel;
        private readonly Label _descLabel;
        private readonly Label _keywordsLabel;

        public CardDetailPopup()
        {
            name = "card-detail-popup";
            AddToClassList("card-detail-overlay");
            style.display = DisplayStyle.None;

            // Panneau centré
            var panel = new VisualElement();
            panel.AddToClassList("card-detail-panel");

            _artwork = new VisualElement { name = "card-detail-artwork" };
            _nameLabel = new Label();
            _typeLabel = new Label();
            _regionLabel = new Label();
            _costLabel = new Label();
            _manaGenLabel = new Label();
            _statsLabel = new Label();
            _descLabel = new Label();
            _keywordsLabel = new Label();

            _nameLabel.AddToClassList("card-detail-name");
            _typeLabel.AddToClassList("card-detail-type");
            _regionLabel.AddToClassList("card-detail-region");
            _costLabel.AddToClassList("card-detail-cost");
            _manaGenLabel.AddToClassList("card-detail-mana-gen");
            _statsLabel.AddToClassList("card-detail-stats");
            _descLabel.AddToClassList("card-detail-desc");
            _keywordsLabel.AddToClassList("card-detail-keywords");

            panel.Add(_artwork);
            panel.Add(_nameLabel);
            panel.Add(_typeLabel);
            panel.Add(_regionLabel);
            panel.Add(_costLabel);
            panel.Add(_manaGenLabel);
            panel.Add(_statsLabel);

            var separator = new VisualElement();
            separator.AddToClassList("card-detail-separator");
            panel.Add(separator);

            panel.Add(_descLabel);
            panel.Add(_keywordsLabel);

            Add(panel);

            // Clic sur l'overlay (hors panneau) → ferme
            RegisterCallback<PointerDownEvent>(_ => Hide());

            // Clic sur le panneau → ne ferme pas
            panel.RegisterCallback<PointerDownEvent>(evt => evt.StopPropagation());
        }

        public void Show(CardData data)
        {
            // Illustration
            if (data.Artwork != null)
            {
                _artwork.style.backgroundImage = new StyleBackground(data.Artwork);
                _artwork.style.display = DisplayStyle.Flex;
            }
            else
            {
                _artwork.style.display = DisplayStyle.None;
            }

            _nameLabel.text = data.CardName;
            _typeLabel.text = GetTypeName(data.CardType);
            _regionLabel.text = data.Region == Region.Aucune ? "" : $"Région : {GetRegionName(data.Region)}";
            _regionLabel.style.display = data.Region == Region.Aucune ? DisplayStyle.None : DisplayStyle.Flex;
            _costLabel.text = $"Coût : {data.ManaCost} mana";

            if (data.ManaGenerated > 0)
            {
                _manaGenLabel.text = $"Sacrifice : +{data.ManaGenerated} mana";
                _manaGenLabel.style.display = DisplayStyle.Flex;
            }
            else
            {
                _manaGenLabel.style.display = DisplayStyle.None;
            }

            switch (data.CardType)
            {
                case CardType.Allie:
                    _statsLabel.text = $"ATK {data.Attack}   DEF {data.Defense}";
                    _statsLabel.style.display = DisplayStyle.Flex;
                    break;

                case CardType.Equipement:
                    _statsLabel.text = $"DMG {data.Attack}   USE {data.Defense}";
                    _statsLabel.style.display = DisplayStyle.Flex;
                    break;

                case CardType.Action:
                    _statsLabel.text = $"DMG {data.Attack}";
                    _statsLabel.style.display = DisplayStyle.Flex;
                    break;

                case CardType.Blocage:
                    _statsLabel.text = $"BLK {data.Defense}";
                    _statsLabel.style.display = DisplayStyle.Flex;
                    break;

                default:
                    _statsLabel.style.display = DisplayStyle.None;
                    break;
            }

            // Description
            _descLabel.text = string.IsNullOrEmpty(data.Description) ? "—" : data.Description;

            // Mots-clés actifs
            var kwText = BuildKeywordsText(data);
            if (!string.IsNullOrEmpty(kwText))
            {
                _keywordsLabel.text = kwText;
                _keywordsLabel.style.display = DisplayStyle.Flex;
            }
            else
            {
                _keywordsLabel.style.display = DisplayStyle.None;
            }

            style.display = DisplayStyle.Flex;
        }

        public void Hide() => style.display = DisplayStyle.None;

        // --- Helpers ---

        private static string GetRegionName(Region region) => region switch
        {
            Region.Montagne => "Montagne",
            Region.Marais => "Marais",
            Region.Plaines => "Plaines",
            Region.Ruines => "Ruines",
            Region.Foret => "Forêt",
            Region.Ocean => "Océan",
            _ => ""
        };

        private static string GetTypeName(CardType type) => type switch
        {
            CardType.Allie => "Allié",
            CardType.Action => "Action",
            CardType.Blocage => "Blocage",

            CardType.Equipement => "Équipement",
            _ => ""
        };

        private static string BuildKeywordsText(CardData data)
        {
            var parts = new List<string>();

            if (data.HasKeyword(Keyword.Vol)) parts.Add("Vol");
            if (data.HasKeyword(Keyword.Portee)) parts.Add("Portée");
            if (data.HasKeyword(Keyword.Instantane)) parts.Add("Instantané");
            if (data.HasKeyword(Keyword.Provocation)) parts.Add("Provocation");
            if (data.HasKeyword(Keyword.Eveille)) parts.Add("Éveillé");
            if (data.HasKeyword(Keyword.Rituel)) parts.Add($"Rituel ({data.RitualValue})");
            if (data.HasKeyword(Keyword.Invincible)) parts.Add("Invincible");
            if (data.HasKeyword(Keyword.Rampant)) parts.Add("Rampant");

            return parts.Count > 0 ? "Mots-clés : " + string.Join(", ", parts) : "";
        }
    }
}