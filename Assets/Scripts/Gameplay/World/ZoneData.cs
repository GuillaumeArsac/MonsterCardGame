using UnityEngine;

namespace MonsterCardGame.Gameplay.World
{
    [CreateAssetMenu(menuName = "MonsterCardGame/World/Zone Data")]
    public class ZoneData : ScriptableObject
    {
        [Header("Identité")]
        [SerializeField] private string _zoneName;
        [SerializeField] private string _description;
        [SerializeField] private Sprite _artwork;

        [Header("Carte du Monde")]
        [SerializeField, Tooltip("Position normalisée (0–1) du marqueur sur la carte du monde")]
        private Vector2 _mapPosition = new Vector2(0.5f, 0.5f);

        [Header("Déblocage")]
        [SerializeField, Tooltip("Si true, la zone reste verrouillée tant que les autres zones ne sont pas terminées")]
        private bool _requiresAllOtherBossesDefeated;

        public string  ZoneName                       => _zoneName;
        public string  Description                    => _description;
        public Sprite  Artwork                        => _artwork;
        public Vector2 MapPosition                    => _mapPosition;
        public bool    RequiresAllOtherBossesDefeated => _requiresAllOtherBossesDefeated;
    }
}
