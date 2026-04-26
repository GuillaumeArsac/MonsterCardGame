using System.Collections.Generic;
using UnityEngine;
using MonsterCardGame.Gameplay.Cards;
using MonsterCardGame.Gameplay.Combat.Data;
using MonsterCardGame.Gameplay.Inventory;

namespace MonsterCardGame.Gameplay.World
{
    /// <summary>
    /// Référentiel de tous les assets SO du jeu.
    /// Placé dans Assets/Resources/ et chargé via Resources.Load pour la résolution des sauvegardes.
    /// </summary>
    [CreateAssetMenu(menuName = "MonsterCardGame/World/Game Registry")]
    public class GameRegistry : ScriptableObject
    {
        [SerializeField] private List<CardData>     _allCards     = new();
        [SerializeField] private List<MaterialData> _allMaterials = new();
        [SerializeField] private List<MonsterData>  _allMonsters  = new();

        public IReadOnlyList<CardData>     AllCards     => _allCards;
        public IReadOnlyList<MaterialData> AllMaterials => _allMaterials;
        public IReadOnlyList<MonsterData>  AllMonsters  => _allMonsters;
    }
}
