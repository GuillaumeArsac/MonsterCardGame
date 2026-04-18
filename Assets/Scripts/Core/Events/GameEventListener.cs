using UnityEngine;
using UnityEngine.Events;

namespace MonsterCardGame.Core.Events
{
    public class GameEventListener : MonoBehaviour
    {
        [Header("Event Channel")]
        [SerializeField, Tooltip("L'événement SO à écouter")]
        private GameEvent _event;

        [Header("Response")]
        [SerializeField, Tooltip("Actions déclenchées à la réception de l'événement")]
        private UnityEvent _response;

        private void OnEnable() => _event?.RegisterListener(this);
        private void OnDisable() => _event?.UnregisterListener(this);

        public void OnEventRaised() => _response?.Invoke();
    }
}
