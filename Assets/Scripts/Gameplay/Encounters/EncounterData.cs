using System.Collections.Generic;
using UnityEngine;
using MonsterCardGame.Core;
using MonsterCardGame.Gameplay.Monsters;

namespace MonsterCardGame.Gameplay.Encounters
{
    /// <summary>Type d'un combat. Référence : docs/progression.md.</summary>
    public enum EncounterType
    {
        /// <summary>Combat du scénario : unique.</summary>
        Scenario,
        /// <summary>Combat libre : rejouable, pour s'entraîner et progresser.</summary>
        Free
    }

    /// <summary>Combat contre une équipe adverse de 5 monstres. Référence : docs/combats-scenario.md.</summary>
    [CreateAssetMenu(menuName = "MonsterCardGame/Encounters/Encounter Data")]
    public class EncounterData : ScriptableObject
    {
        [Header("Identité")]
        [SerializeField, Tooltip("Identifiant unique, utilisé par la sauvegarde. Ne plus le modifier une fois le combat publié")]
        private string _encounterId;

        [SerializeField, Tooltip("Nom affiché")]
        private string _displayName;

        [SerializeField] private EncounterType _type;

        [Header("Équipe adverse")]
        [SerializeField, Tooltip("5 monstres en général ; de 1 à 5 (Vaelisse combat seule)")]
        private List<MonsterCardData> _monsters = new();

        [Header("Dépeçage")]
        [SerializeField, Tooltip("Monstre toujours dépecé après une victoire (le chef d'un boss ou d'un semi-boss). Vide : tiré au hasard dans l'équipe")]
        private MonsterCardData _skinnedMonster;

        public string                         EncounterId    => _encounterId;
        public string                         DisplayName    => _displayName;
        public EncounterType                  Type           => _type;
        public IReadOnlyList<MonsterCardData> Monsters       => _monsters;
        /// <summary>Monstre toujours dépecé, ou null pour un tirage au hasard.</summary>
        public MonsterCardData                SkinnedMonster => _skinnedMonster;

        public bool IsRepeatable => _type == EncounterType.Free;

        /// <summary>Remplit le combat. Pour les scripts d'éditeur et les tests.</summary>
        public void Init(string encounterId, string displayName, EncounterType type, IEnumerable<MonsterCardData> monsters,
                         MonsterCardData skinnedMonster = null)
        {
            _encounterId    = encounterId;
            _displayName    = displayName;
            _type           = type;
            _monsters       = new List<MonsterCardData>(monsters);
            _skinnedMonster = skinnedMonster;
            Validate();
        }

        private void OnValidate() => Validate();

        private void Validate()
        {
            if (string.IsNullOrEmpty(_encounterId))
                GameLog.Warning("EncounterData", $"{name} : identifiant manquant");

            if (_monsters.Count < 1 || _monsters.Count > GameRules.TeamSize)
                GameLog.Warning("EncounterData", $"{name} : {_monsters.Count} monstres, il en faut de 1 à {GameRules.TeamSize}");

            if (_skinnedMonster != null && !_monsters.Contains(_skinnedMonster))
                GameLog.Warning("EncounterData", $"{name} : le monstre dépecé {_skinnedMonster.MonsterName} n'est pas dans l'équipe");
        }
    }
}
