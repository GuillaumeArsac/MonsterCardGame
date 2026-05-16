using System.Collections.Generic;
using UnityEngine;
using MonsterCardGame.Gameplay.Cards;
using MonsterCardGame.Gameplay.Inventory;

namespace MonsterCardGame.Gameplay.Combat.Data
{
    [CreateAssetMenu(fileName = "MonsterData", menuName = "MonsterCardGame/Monster Data")]
    public class MonsterData : ScriptableObject
    {
        [Header("Identité")]
        [SerializeField, Tooltip("Nom du monstre affiché en combat")]
        private string _monsterName = "Monstre";

        [SerializeField, Tooltip("Portrait affiché sur la carte du monde")]
        private Sprite _portrait;

        [Header("Combat")]
        [SerializeField, Tooltip("Points de vie de départ du monstre")]
        private int _startingHP = 30;

        [SerializeField, Tooltip("Deck scripté du monstre — joué dans l'ordre")]
        private List<CardData> _deck = new();

        [Header("Loot")]
        [SerializeField, Tooltip("Matériaux que ce monstre peut droper à la victoire, avec leur chance individuelle")]
        private List<LootEntry> _lootTable = new();

        [Header("Boss Passive — Epic 5-7")]
        [SerializeField, Tooltip("Passive boss (null = aucune). Implémentation dans Epics 5-7.")]
        private ScriptableObject _bossPassive = null;

        public string                   MonsterName => _monsterName;
        public Sprite                   Portrait    => _portrait;
        public int                      StartingHP  => _startingHP;
        public IReadOnlyList<CardData>  Deck        => _deck;
        public IReadOnlyList<LootEntry> LootTable   => _lootTable;
        public IBossPassive             BossPassive => _bossPassive as IBossPassive;
    }
}
