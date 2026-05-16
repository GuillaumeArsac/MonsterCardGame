using System.Collections.Generic;
using UnityEngine;
using MonsterCardGame.Gameplay.Combat.Data;

namespace MonsterCardGame.Gameplay.World
{
    [CreateAssetMenu(menuName = "MonsterCardGame/World/Zone Data")]
    public class ZoneData : ScriptableObject
    {
        [Header("Identité")]
        [SerializeField] private string _zoneName;
        [SerializeField] private string _description;
        [SerializeField] private Sprite _artwork;

        [Header("Monstres")]
        [SerializeField, Tooltip("Monstres normaux de la zone (hors boss)")]
        private List<MonsterData> _monsters = new();

        [SerializeField, Tooltip("Boss de la zone — doit être vaincu pour débloquer les Ruines")]
        private MonsterData _boss;

        [Header("Carte du Monde")]
        [SerializeField, Tooltip("Position normalisée (0–1) du marqueur sur la carte du monde")]
        private Vector2 _mapPosition = new Vector2(0.5f, 0.5f);

        [Header("Déblocage")]
        [SerializeField, Tooltip("Si true, nécessite que le boss de chaque autre zone soit vaincu")]
        private bool _requiresAllOtherBossesDefeated;

        public string                        ZoneName                       => _zoneName;
        public string                        Description                    => _description;
        public Sprite                        Artwork                        => _artwork;
        public Vector2                       MapPosition                    => _mapPosition;
        public IReadOnlyList<MonsterData>    Monsters                       => _monsters;
        public MonsterData                   Boss                           => _boss;
        public bool                          RequiresAllOtherBossesDefeated => _requiresAllOtherBossesDefeated;
    }
}
