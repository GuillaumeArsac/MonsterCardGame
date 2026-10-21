using System.Collections.Generic;
using UnityEngine;
using MonsterCardGame.Core;
using MonsterCardGame.Gameplay.Encounters;

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

        [Header("Combats")]
        [SerializeField, Tooltip("Combats du scénario, uniques, dans l'ordre : chacun est disponible quand les précédents sont gagnés")]
        private List<EncounterData> _scenarioEncounters = new();

        [SerializeField, Tooltip("Combats libres, rejouables, pour s'entraîner et progresser")]
        private List<EncounterData> _freeEncounters = new();

        [SerializeField, Tooltip("Boss de la zone : un combat du scénario. Le vaincre compte pour le déblocage des Ruines")]
        private EncounterData _bossEncounter;

        [Header("Déblocage")]
        [SerializeField, Tooltip("Si true, la zone reste verrouillée tant que le boss de chaque autre zone n'est pas vaincu")]
        private bool _requiresAllOtherBossesDefeated;

        public string                       ZoneName                       => _zoneName;
        public string                       Description                    => _description;
        public Sprite                       Artwork                        => _artwork;
        public Vector2                      MapPosition                    => _mapPosition;
        public IReadOnlyList<EncounterData> ScenarioEncounters             => _scenarioEncounters;
        public IReadOnlyList<EncounterData> FreeEncounters                 => _freeEncounters;
        public EncounterData                BossEncounter                  => _bossEncounter;
        public bool                         RequiresAllOtherBossesDefeated => _requiresAllOtherBossesDefeated;

        /// <summary>Identité de la zone. Pour les scripts d'éditeur et les tests.</summary>
        public void InitIdentity(string zoneName, string description, Vector2 mapPosition, bool requiresAllOtherBossesDefeated)
        {
            _zoneName = zoneName;
            _description = description;
            _mapPosition = mapPosition;
            _requiresAllOtherBossesDefeated = requiresAllOtherBossesDefeated;
        }

        /// <summary>Combats de la zone. Pour les scripts d'éditeur et les tests.</summary>
        public void SetEncounters(IEnumerable<EncounterData> scenario, IEnumerable<EncounterData> free, EncounterData boss)
        {
            _scenarioEncounters = new List<EncounterData>(scenario);
            _freeEncounters     = new List<EncounterData>(free);
            _bossEncounter      = boss;
            Validate();
        }

        private void OnValidate() => Validate();

        private void Validate()
        {
            foreach (var encounter in _scenarioEncounters)
                if (encounter != null && encounter.Type != EncounterType.Scenario)
                    GameLog.Warning("ZoneData", $"{name} : {encounter.name} est dans les combats du scénario mais n'est pas de type Scenario");
            foreach (var encounter in _freeEncounters)
                if (encounter != null && encounter.Type != EncounterType.Free)
                    GameLog.Warning("ZoneData", $"{name} : {encounter.name} est dans les combats libres mais n'est pas de type Free");
            if (_bossEncounter != null && !_scenarioEncounters.Contains(_bossEncounter))
                GameLog.Warning("ZoneData", $"{name} : le boss {_bossEncounter.name} doit faire partie des combats du scénario");
        }
    }
}
