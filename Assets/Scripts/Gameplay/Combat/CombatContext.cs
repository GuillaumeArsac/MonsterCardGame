using System;
using System.Collections.Generic;
using MonsterCardGame.Gameplay.Cards;
using MonsterCardGame.Gameplay.Combat.Data;
using MonsterCardGame.Gameplay.Inventory;

namespace MonsterCardGame.Gameplay.Combat
{
    /// <summary>
    /// État mutable partagé entre tous les états du combat.
    /// Classe C# pure — jamais sérialisée (ADR-001).
    /// </summary>
    public class CombatContext
    {
        // --- Joueur ---
        public List<CardData>       PlayerDeck     { get; } = new();
        public List<CardData>       PlayerHand     { get; } = new();
        public List<CardData>       PlayerCemetery { get; } = new();
        public List<AlliedInstance> PlayerAllies   { get; } = new();
        public int                  PlayerHP       { get; set; }
        public int                  PlayerMana     { get; set; }

        // --- Monstre ---
        public MonsterData          MonsterData      { get; }
        public List<CardData>       MonsterDeck      { get; } = new();
        public List<CardData>       MonsterHand      { get; } = new();
        public List<AlliedInstance> MonsterAllies    { get; } = new();
        public List<CardData>       MonsterCemetery  { get; } = new();
        public int                  MonsterHP        { get; set; }

        // --- État tour ---
        public int                  Turn           { get; set; } = 1;
        public bool                 IsPlayerTurn   { get; set; } = true;

        // --- Action monstre en attente de résolution (Blocage possible) ---
        /// <summary>Carte Action du monstre annoncée mais pas encore résolue. Null si aucune.</summary>
        public CardData             PendingMonsterAction { get; set; } = null;
        /// <summary>Allié joueur ciblé par PendingMonsterAction. Null = attaque directe au joueur.</summary>
        public AlliedInstance       PendingMonsterTarget { get; set; } = null;

        // --- Résultat ---
        public CombatResult         Result           { get; set; } = CombatResult.None;
        /// <summary>Matériaux droppés lors de cette victoire. Vide si défaite ou combat en cours.</summary>
        public List<MaterialData>   DroppedMaterials { get; } = new();

        public CombatContext(MonsterData monsterData, IReadOnlyList<CardData> playerDeckSource)
        {
            MonsterData = monsterData ?? throw new ArgumentNullException(nameof(monsterData));
            MonsterHP   = monsterData.StartingHP;
            PlayerHP    = Core.GameRules.PlayerStartingHP;
            PlayerMana  = 0;

            // Copier les decks — jamais modifier les assets SO
            foreach (var card in playerDeckSource) PlayerDeck.Add(card);
            foreach (var card in monsterData.Deck)  MonsterDeck.Add(card);
        }
    }

    public enum CombatResult { None, PlayerWin, PlayerLose }
}
