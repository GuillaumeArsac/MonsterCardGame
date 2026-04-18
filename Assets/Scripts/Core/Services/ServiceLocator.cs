using System;
using System.Collections.Generic;

namespace MonsterCardGame.Core.Services
{
    public static class Services
    {
        private static readonly Dictionary<Type, IService> _registry = new();

        public static void Register<T>(T service) where T : IService
        {
            _registry[typeof(T)] = service;
            GameLog.Info("Services", $"Enregistré : {typeof(T).Name}");
        }

        public static T Get<T>() where T : IService
        {
            if (_registry.TryGetValue(typeof(T), out var service))
                return (T)service;

            GameLog.Error("Services", $"Service non enregistré : {typeof(T).Name}");
            return default;
        }

        public static bool Has<T>() where T : IService
            => _registry.ContainsKey(typeof(T));

        public static void Clear() => _registry.Clear();
    }
}
