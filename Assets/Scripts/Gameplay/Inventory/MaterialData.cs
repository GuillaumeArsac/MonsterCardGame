using UnityEngine;
using MonsterCardGame.Gameplay.Cards;

namespace MonsterCardGame.Gameplay.Inventory
{
    [CreateAssetMenu(fileName = "MaterialData", menuName = "MonsterCardGame/Inventory/Material Data")]
    public class MaterialData : ScriptableObject
    {
        [Header("Identité")]
        [SerializeField, Tooltip("Nom affiché du matériau")]
        private string _materialName;

        [SerializeField, Tooltip("Rareté du matériau")]
        private MaterialRarity _rarity;

        [SerializeField, Tooltip("Région d'origine du matériau")]
        private Region _region;

        [SerializeField, Tooltip("Icône affichée dans l'inventaire et la Forge")]
        private Sprite _icon;

        public string         MaterialName => _materialName;
        public MaterialRarity Rarity       => _rarity;
        public Region         Region       => _region;
        public Sprite         Icon         => _icon;
    }
}
