using UnityEngine;
using UnityEngine.UIElements;
using MonsterCardGame.Gameplay.Cards;

namespace MonsterCardGame.UI.Combat
{
    /// <summary>
    /// VisualElement compact : affiche uniquement le nom, l'illustration et le bord coloré par type.
    /// Les détails complets sont disponibles via la popup (clic droit).
    /// </summary>
    public class CardView : VisualElement
    {
        public CardData Data { get; private set; }

        private readonly Label _nameLabel;
        private readonly VisualElement _artworkImage;

        private string _currentTypeClass;

        public CardView(CardData data)
        {
            AddToClassList("card-view");

            _nameLabel = new Label { name = "card-name" };
            _artworkImage = new VisualElement { name = "card-artwork" };

            Add(_nameLabel);
            Add(_artworkImage);

            Refresh(data);
        }

        public void Refresh(CardData data)
        {
            Data = data;

            _nameLabel.text = data.CardName;

            // Illustration
            if (data.Artwork != null)
            {
                _artworkImage.style.backgroundImage = new StyleBackground(data.Artwork);
                _artworkImage.style.display = DisplayStyle.Flex;
            }
            else
            {
                _artworkImage.style.display = DisplayStyle.None;
            }

            // Bord coloré par type
            if (_currentTypeClass != null)
                RemoveFromClassList(_currentTypeClass);

            _currentTypeClass = GetRegionClass(data.Region);
            AddToClassList(_currentTypeClass);
        }

        private static string GetRegionClass(Region region) => region switch
        {
            Region.Montagne => "region--montagne",
            Region.Marais => "region--marais",
            Region.Plaines => "region--plaines",
            Region.Ruines => "region--ruines",
            Region.Foret => "region--foret",
            Region.Ocean => "region--ocean",
            _ => "region--aucune"
        };
    }
}