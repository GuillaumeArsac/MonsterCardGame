using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using MonsterCardGame.Gameplay.Cards;
using MonsterCardGame.Gameplay.Deck;

namespace MonsterCardGame.Tests.EditMode
{
    public class DeckValidatorTests
    {
        // Crée une CardData avec une rareté précise via SerializedObject (pattern Edit Mode)
        private static CardData CreateCard(CardRarity rarity = CardRarity.Commune)
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            var so = new SerializedObject(card);
            so.FindProperty("_rarity").enumValueIndex = (int)rarity;
            so.ApplyModifiedPropertiesWithoutUndo();
            return card;
        }

        // Crée un DeckData peuplé avec le nombre demandé de cartes par rareté
        private static DeckData CreateDeck(int communes = 0, int rares = 0, int legendaires = 0, int uniques = 0)
        {
            var deck = ScriptableObject.CreateInstance<DeckData>();
            for (int i = 0; i < communes;    i++) deck.AddCard(CreateCard(CardRarity.Commune));
            for (int i = 0; i < rares;       i++) deck.AddCard(CreateCard(CardRarity.Rare));
            for (int i = 0; i < legendaires; i++) deck.AddCard(CreateCard(CardRarity.Legendaire));
            for (int i = 0; i < uniques;     i++) deck.AddCard(CreateCard(CardRarity.Unique));
            return deck;
        }

        // --- Taille du deck ---

        [Test]
        public void Validate_ExactlyFortyCommonCards_ReturnsTrue()
        {
            var deck = CreateDeck(communes: 40);
            Assert.IsTrue(DeckValidator.Validate(deck));
        }

        [Test]
        public void HasCorrectSize_ThirtyNineCards_ReturnsFalse()
        {
            var deck = CreateDeck(communes: 39);
            Assert.IsFalse(DeckValidator.HasCorrectSize(deck));
        }

        [Test]
        public void HasCorrectSize_FortyOneCards_ReturnsFalse()
        {
            var deck = CreateDeck(communes: 41);
            Assert.IsFalse(DeckValidator.HasCorrectSize(deck));
        }

        // --- Poids du deck ---

        [Test]
        public void IsWithinWeightLimit_FortyCommonCards_ReturnsTrue()
        {
            // Communes poids=0 → total=0 ≤ 15
            var deck = CreateDeck(communes: 40);
            Assert.IsTrue(DeckValidator.IsWithinWeightLimit(deck));
        }

        [Test]
        public void IsWithinWeightLimit_FifteenRareCards_ReturnsTrue()
        {
            // 15 Rares × poids 1 = 15 = limite exacte
            var deck = CreateDeck(communes: 25, rares: 15);
            Assert.IsTrue(DeckValidator.IsWithinWeightLimit(deck));
        }

        [Test]
        public void IsWithinWeightLimit_SixteenRareCards_ReturnsFalse()
        {
            // 16 Rares × poids 1 = 16 > 15
            var deck = CreateDeck(communes: 24, rares: 16);
            Assert.IsFalse(DeckValidator.IsWithinWeightLimit(deck));
        }

        [Test]
        public void GetTotalWeight_TenRareCards_ReturnsTen()
        {
            var deck = CreateDeck(communes: 30, rares: 10);
            Assert.AreEqual(10, DeckValidator.GetTotalWeight(deck));
        }

        // --- Copies max ---

        [Test]
        public void RespectsCopyLimits_ThreeCopiesOfSameCommon_ReturnsTrue()
        {
            var deck = ScriptableObject.CreateInstance<DeckData>();
            var card = CreateCard(CardRarity.Commune);
            for (int i = 0; i < 3;  i++) deck.AddCard(card);
            for (int i = 0; i < 37; i++) deck.AddCard(CreateCard());
            Assert.IsTrue(DeckValidator.RespectsCopyLimits(deck));
        }

        [Test]
        public void RespectsCopyLimits_FourCopiesOfSameCommon_ReturnsFalse()
        {
            var deck = ScriptableObject.CreateInstance<DeckData>();
            var card = CreateCard(CardRarity.Commune);
            for (int i = 0; i < 4;  i++) deck.AddCard(card);
            for (int i = 0; i < 36; i++) deck.AddCard(CreateCard());
            Assert.IsFalse(DeckValidator.RespectsCopyLimits(deck));
        }

        [Test]
        public void RespectsCopyLimits_TwoCopiesOfLegendary_ReturnsFalse()
        {
            var deck = ScriptableObject.CreateInstance<DeckData>();
            var legendary = CreateCard(CardRarity.Legendaire);
            deck.AddCard(legendary);
            deck.AddCard(legendary);
            for (int i = 0; i < 38; i++) deck.AddCard(CreateCard());
            Assert.IsFalse(DeckValidator.RespectsCopyLimits(deck));
        }

        [Test]
        public void RespectsCopyLimits_OneCopyOfLegendary_ReturnsTrue()
        {
            var deck = ScriptableObject.CreateInstance<DeckData>();
            deck.AddCard(CreateCard(CardRarity.Legendaire));
            for (int i = 0; i < 39; i++) deck.AddCard(CreateCard());
            Assert.IsTrue(DeckValidator.RespectsCopyLimits(deck));
        }
    }
}
