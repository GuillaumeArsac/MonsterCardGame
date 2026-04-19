using System.Collections.Generic;
using UnityEngine;
using MonsterCardGame.Core;
using MonsterCardGame.Gameplay.Cards.Effects;

namespace MonsterCardGame.Gameplay.Cards
{
    [CreateAssetMenu(menuName = "MonsterCardGame/Cards/Card Data")]
    public class CardData : ScriptableObject
    {
        [Header("Identité")]
        [SerializeField, Tooltip("Nom affiché sur la carte")]
        private string _cardName;

        [SerializeField, Tooltip("Région d'origine de la carte (détermine la couleur de bord)")]
        private Region _region;

        [SerializeField, Tooltip("Illustration de la carte")]
        private Sprite _artwork;

        [SerializeField, Tooltip("Description de l'effet de la carte")]
        [TextArea(2, 4)]
        private string _description;

        [Header("Coûts")]
        [SerializeField, Tooltip("Coût en mana pour jouer la carte")]
        private int _manaCost;

        [SerializeField, Tooltip("Mana généré si la carte est sacrifiée")]
        private int _manaGenerated;

        [Header("Classification")]
        [SerializeField, Tooltip("Type de carte")]
        private CardType _cardType;

        [SerializeField, Tooltip("Rareté de la carte (détermine le poids dans le deck)")]
        private CardRarity _rarity;

        [Header("Mots-clés")]
        [SerializeField, Tooltip("Liste des mots-clés actifs sur la carte")]
        private List<Keyword> _keywords = new();

        [SerializeField, Tooltip("Valeur X du mot-clé Rituel(X). Ignoré si Rituel absent.")]
        private int _ritualValue;

        [Header("Combat")]
        [SerializeField, Tooltip("Points d'attaque")]
        private int _attack;

        [SerializeField, Tooltip("Points de défense")]
        private int _defense;

        [Header("Effets")]
        [SerializeField, Tooltip("Effets déclenchés quand la carte est jouée depuis la main")]
        private List<CardEffect> _onPlayEffects = new();

        [SerializeField, Tooltip("Effets déclenchés quand la carte ou l'allié attaque")]
        private List<CardEffect> _onAttackEffects = new();

        // Propriétés publiques
        public string CardName => _cardName;

        public Region Region => _region;
        public Sprite Artwork => _artwork;
        public string Description => _description;
        public int ManaCost => _manaCost;
        public int ManaGenerated => _manaGenerated;
        public CardType CardType => _cardType;
        public CardRarity Rarity => _rarity;
        public IReadOnlyList<Keyword> Keywords => _keywords;
        public int RitualValue => _ritualValue;
        public int Attack => _attack;
        public int Defense => _defense;
        public IReadOnlyList<CardEffect> OnPlayEffects   => _onPlayEffects;
        public IReadOnlyList<CardEffect> OnAttackEffects => _onAttackEffects;

        public bool HasKeyword(Keyword keyword) => _keywords != null && _keywords.Contains(keyword);

        /// <summary>Poids de la carte dans le deck, dérivé de sa rareté.</summary>
        public int Weight => _rarity switch
        {
            CardRarity.Commune => GameRules.WeightCommon,
            CardRarity.Rare => GameRules.WeightRare,
            CardRarity.Legendaire => GameRules.WeightLegendary,
            CardRarity.Unique => GameRules.WeightUnique,
            _ => 0
        };
    }
}