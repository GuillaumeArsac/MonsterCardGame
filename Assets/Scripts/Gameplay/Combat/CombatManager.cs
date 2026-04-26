using System;
using System.Collections.Generic;
using UnityEngine;
using MonsterCardGame.Core.Services;
using MonsterCardGame.Gameplay.Cards;
using MonsterCardGame.Gameplay.Deck;
using MonsterCardGame.Gameplay.Combat.Data;
using MonsterCardGame.Gameplay.Combat.MonsterAI;
using MonsterCardGame.Gameplay.Combat.States;
using MonsterCardGame.Gameplay.Inventory;
using MonsterCardGame.Gameplay.World;

namespace MonsterCardGame.Gameplay.Combat
{
    public class CombatManager : MonoBehaviour
    {
        [Header("Combat Setup")]
        [SerializeField, Tooltip("Deck joueur hardcodé — remplacé par navigation monde en Epic 4")]
        private DeckData _playerDeck;

        [SerializeField, Tooltip("Monstre de test — remplacé par navigation monde en Epic 4")]
        private MonsterData _monsterData;

        private CombatContext _ctx;
        private ICombatState _currentState;

        private DrawState _drawState;
        private SacrificeState _sacrificeState;
        private PlayState _playState;
        private ReactiveWindowState _reactiveState;
        private MonsterTurnState _monsterTurnState;
        private CombatEndState _endState;

        private readonly MonsterAIController _ai = new();

        public CombatContext Context => _ctx;
        public SacrificeState SacrificeState => _sacrificeState;
        public PlayState PlayState => _playState;
        public ReactiveWindowState ReactiveState => _reactiveState;
        public ICombatState CurrentState => _currentState;

        private void Start()
        {
            if (_playerDeck == null || _monsterData == null)
            {
                Core.GameLog.Error("CombatManager", "PlayerDeck ou MonsterData non assigné dans l'Inspector");
                return;
            }

            InitStates();
            var inventory = Services.Get<IPlayerInventory>();
            SeedStartingInventory(inventory);

            var setup         = Services.Get<ICombatSetupService>();
            var monsterToFight = setup?.PendingMonster ?? _monsterData;

            if (monsterToFight == null)
            {
                Core.GameLog.Error("CombatManager", "Aucun monstre sélectionné — vérifier ICombatSetupService ou _monsterData");
                return;
            }

            IReadOnlyList<CardData> deckSource =
                inventory != null && inventory.ActiveDeck.Count > 0
                    ? inventory.ActiveDeck
                    : _playerDeck.Cards;
            _ctx = new CombatContext(monsterToFight, deckSource);
            Shuffle(_ctx.PlayerDeck);
            Shuffle(_ctx.MonsterDeck);
            TransitionTo(_drawState);
        }

        private void InitStates()
        {
            _endState = new CombatEndState();

            _reactiveState = new ReactiveWindowState(TransitionTo, null);
            _monsterTurnState = new MonsterTurnState(TransitionTo, _reactiveState, null, _endState, _ai);
            _playState = new PlayState(TransitionTo, _monsterTurnState);
            _sacrificeState = new SacrificeState(TransitionTo, _playState);
            _drawState = new DrawState(TransitionTo, _sacrificeState);

            _reactiveState.SetAfterReactive(_monsterTurnState);
            _monsterTurnState.SetDrawState(_drawState);
        }

        /// <summary>
        /// Ajoute les cartes du deck de départ à l'inventaire une seule fois (session vierge).
        /// Remplacé par un vrai système de sauvegarde en Epic 4.
        /// </summary>
        private void SeedStartingInventory(IPlayerInventory inventory)
        {
            if (inventory == null || inventory.OwnedCards.Count > 0) return;

            foreach (var card in _playerDeck.Cards)
                inventory.AddCard(card);

            Core.GameLog.Info("CombatManager",
                $"Inventaire initialisé avec {_playerDeck.Cards.Count} carte(s) du deck de départ");
        }

        private static void Shuffle<T>(List<T> list)
        {
            var rng = new System.Random();
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        private void Update()
        {
            if (_ctx == null) return;

            _currentState?.Update(_ctx);

            if (_ctx.Result != CombatResult.None && _currentState is not CombatEndState)
                TransitionTo(_endState);
        }

        public void TransitionTo(ICombatState next)
        {
            _currentState?.Exit(_ctx);
            _currentState = next;
            _currentState?.Enter(_ctx);
            Core.GameLog.Info("CombatManager", $"→ {next?.GetType().Name ?? "null"}");
        }
    }
}