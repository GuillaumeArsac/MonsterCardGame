using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using MonsterCardGame.Core;
using MonsterCardGame.Core.Services;
using MonsterCardGame.Gameplay.Cards;
using MonsterCardGame.Gameplay.Inventory;
using MonsterCardGame.UI.Combat;

namespace MonsterCardGame.UI.DeckBuilder
{
    [RequireComponent(typeof(UIDocument))]
    public class DeckBuilderUI : MonoBehaviour
    {
        private IPlayerInventory _inventory;

        private VisualElement   _collectionList;
        private VisualElement   _deckList;
        private Label           _cardsCountLabel;
        private Label           _weightLabel;
        private CardDetailPopup _cardDetailPopup;

        private void OnEnable()
        {
            _inventory = Services.Get<IPlayerInventory>();

            var root = GetComponent<UIDocument>().rootVisualElement;

            _collectionList  = root.Q<VisualElement>("collection-list");
            _deckList        = root.Q<VisualElement>("deck-list");
            _cardsCountLabel = root.Q<Label>("cards-count-label");
            _weightLabel     = root.Q<Label>("weight-label");

            root.Q<Button>("worldmap-btn").clicked += () => SceneManager.LoadScene("WorldMap");
            root.Q<Button>("forge-btn").clicked    += () => SceneManager.LoadScene("Forge");

            _cardDetailPopup = new CardDetailPopup();
            root.Add(_cardDetailPopup);
        }

        private void Start()
        {
            Refresh();
        }

        // ── Refresh global ────────────────────────────────────────────────

        private void Refresh()
        {
            BuildCollectionList();
            BuildDeckList();
            RefreshStats();
        }

        // ── Panneau collection ────────────────────────────────────────────

        private void BuildCollectionList()
        {
            _collectionList.Clear();

            if (_inventory.OwnedCards.Count == 0)
            {
                var empty = new Label("Aucune carte dans votre collection.");
                empty.AddToClassList("empty-label");
                _collectionList.Add(empty);
                return;
            }

            // Tri : type puis nom
            var sorted = new List<KeyValuePair<CardData, int>>(_inventory.OwnedCards);
            sorted.Sort((a, b) =>
            {
                int typeComp = a.Key.CardType.CompareTo(b.Key.CardType);
                return typeComp != 0 ? typeComp : string.Compare(a.Key.CardName, b.Key.CardName);
            });

            foreach (var (card, owned) in sorted)
            {
                int inDeck    = _inventory.GetDeckCardCount(card);
                int maxCopies = GetMaxCopies(card.Rarity);

                var row = new VisualElement();
                row.AddToClassList("collection-row");

                var nameLabel = new Label(card.CardName);
                nameLabel.AddToClassList("col-name");

                var typeLabel = new Label($"{card.CardType}  ·  {card.Rarity}");
                typeLabel.AddToClassList("col-type");

                var copiesLabel = new Label($"{inDeck} / {maxCopies}  ({owned}×)");
                copiesLabel.AddToClassList("col-copies");
                if (inDeck >= maxCopies) copiesLabel.AddToClassList("col-copies--full");

                var addBtn = new Button { text = "+" };
                addBtn.AddToClassList("deck-action-btn");
                addBtn.SetEnabled(inDeck < maxCopies && inDeck < owned && _inventory.ActiveDeck.Count < GameRules.DeckSize);

                var removeBtn = new Button { text = "−" };
                removeBtn.AddToClassList("deck-action-btn");
                removeBtn.SetEnabled(inDeck > 0);

                addBtn.clicked += () =>
                {
                    _inventory.AddCardToDeck(card);
                    GameLog.Info("DeckBuilder", $"+1 {card.CardName} dans le deck");
                    Refresh();
                };

                removeBtn.clicked += () =>
                {
                    _inventory.RemoveCardFromDeck(card);
                    GameLog.Info("DeckBuilder", $"-1 {card.CardName} du deck");
                    Refresh();
                };

                row.Add(nameLabel);
                row.Add(typeLabel);
                row.Add(copiesLabel);
                row.Add(removeBtn);
                row.Add(addBtn);

                row.RegisterCallback<PointerDownEvent>(evt =>
                {
                    if (evt.button == 1) { _cardDetailPopup.Show(card); evt.StopPropagation(); }
                });

                _collectionList.Add(row);
            }
        }

        // ── Panneau deck ──────────────────────────────────────────────────

        private void BuildDeckList()
        {
            _deckList.Clear();

            if (_inventory.ActiveDeck.Count == 0)
            {
                var empty = new Label("Deck vide — ajoute des cartes depuis ta collection.");
                empty.AddToClassList("empty-label");
                _deckList.Add(empty);
                return;
            }

            // Regrouper par carte
            var grouped = new Dictionary<CardData, int>();
            foreach (var card in _inventory.ActiveDeck)
            {
                grouped.TryGetValue(card, out int count);
                grouped[card] = count + 1;
            }

            // Tri : type puis nom
            var sorted = new List<KeyValuePair<CardData, int>>(grouped);
            sorted.Sort((a, b) =>
            {
                int typeComp = a.Key.CardType.CompareTo(b.Key.CardType);
                return typeComp != 0 ? typeComp : string.Compare(a.Key.CardName, b.Key.CardName);
            });

            foreach (var (card, count) in sorted)
            {
                var row = new VisualElement();
                row.AddToClassList("deck-row");

                var countLabel = new Label($"×{count}");
                countLabel.AddToClassList("deck-count");

                var nameLabel = new Label(card.CardName);
                nameLabel.AddToClassList("deck-name");

                var removeBtn = new Button { text = "−" };
                removeBtn.AddToClassList("deck-action-btn");
                removeBtn.clicked += () =>
                {
                    _inventory.RemoveCardFromDeck(card);
                    Refresh();
                };

                row.Add(countLabel);
                row.Add(nameLabel);
                row.Add(removeBtn);

                row.RegisterCallback<PointerDownEvent>(evt =>
                {
                    if (evt.button == 1) { _cardDetailPopup.Show(card); evt.StopPropagation(); }
                });

                _deckList.Add(row);
            }
        }

        // ── Stats & validation ────────────────────────────────────────────

        private void RefreshStats()
        {
            int  total  = _inventory.ActiveDeck.Count;
            int  weight = ComputeTotalWeight();
            bool valid  = total == GameRules.DeckSize
                       && weight <= GameRules.MaxDeckWeight
                       && CopyLimitsRespected();

            _cardsCountLabel.text = $"{total} / {GameRules.DeckSize} cartes";
            _cardsCountLabel.RemoveFromClassList("stat--ok");
            _cardsCountLabel.RemoveFromClassList("stat--warn");
            _cardsCountLabel.AddToClassList(total == GameRules.DeckSize ? "stat--ok" : "stat--warn");

            _weightLabel.text = $"Poids : {weight} / {GameRules.MaxDeckWeight}";
            _weightLabel.RemoveFromClassList("stat--ok");
            _weightLabel.RemoveFromClassList("stat--error");
            _weightLabel.AddToClassList(weight <= GameRules.MaxDeckWeight ? "stat--ok" : "stat--error");

        }

        private int ComputeTotalWeight()
        {
            int total = 0;
            foreach (var card in _inventory.ActiveDeck)
                total += card.Weight;
            return total;
        }

        private bool CopyLimitsRespected()
        {
            var counts = new Dictionary<CardData, int>();
            foreach (var card in _inventory.ActiveDeck)
            {
                counts.TryGetValue(card, out int c);
                counts[card] = c + 1;
                if (counts[card] > GetMaxCopies(card.Rarity)) return false;
            }
            return true;
        }

        private static int GetMaxCopies(CardRarity rarity) => rarity switch
        {
            CardRarity.Commune    => GameRules.MaxCopiesCommon,
            CardRarity.Rare       => GameRules.MaxCopiesRare,
            CardRarity.Legendaire => GameRules.MaxCopiesLegendary,
            CardRarity.Unique     => GameRules.MaxCopiesUnique,
            _                     => GameRules.MaxCopiesCommon
        };
    }
}
