using System.Collections.Generic;
using UnityEngine;

namespace MonsterCardGame.Core.Events
{
    [CreateAssetMenu(menuName = "MonsterCardGame/Events/Game Event")]
    public class GameEvent : ScriptableObject
    {
        [Header("Debug")]
        [SerializeField, Tooltip("Loguer chaque déclenchement dans l'éditeur")]
        private bool _debugLog = false;

        private readonly List<GameEventListener> _listeners = new();

        public void Raise()
        {
            if (_debugLog)
                GameLog.Info("GameEvent", $"{name} déclenché ({_listeners.Count} listeners)");

            for (int i = _listeners.Count - 1; i >= 0; i--)
                _listeners[i].OnEventRaised();
        }

        public void RegisterListener(GameEventListener listener)
        {
            if (!_listeners.Contains(listener))
                _listeners.Add(listener);
        }

        public void UnregisterListener(GameEventListener listener)
            => _listeners.Remove(listener);
    }
}
