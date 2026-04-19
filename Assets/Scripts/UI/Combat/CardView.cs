using UnityEngine;
using UnityEngine.UIElements;
using MonsterCardGame.Gameplay.Cards;
using MonsterCardGame.Gameplay.Combat;

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
        private readonly Label _chargeLabel;

        private string _currentTypeClass;

        public CardView(CardData data)
        {
            AddToClassList("card-view");

            _nameLabel = new Label { name = "card-name" };
            _artworkImage = new VisualElement { name = "card-artwork" };
            _chargeLabel = new Label { name = "card-charges" };
            _chargeLabel.AddToClassList("card-charges");
            _chargeLabel.AddToClassList("hidden");

            Add(_nameLabel);
            Add(_artworkImage);
            Add(_chargeLabel);

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

        public void RefreshInstance(AlliedInstance instance)
        {
            if (instance.Data.CardType != CardType.Equipement)
            {
                _chargeLabel.AddToClassList("hidden");
                return;
            }

            _chargeLabel.RemoveFromClassList("hidden");
            _chargeLabel.text = $"{instance.DEF}";
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