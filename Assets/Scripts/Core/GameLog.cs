namespace MonsterCardGame.Core
{
    public static class GameLog
    {
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Info(string system, string message)
            => UnityEngine.Debug.Log($"[{system}] {message}");

        public static void Warning(string system, string message)
            => UnityEngine.Debug.LogWarning($"[{system}] {message}");

        public static void Error(string system, string message)
            => UnityEngine.Debug.LogError($"[{system}] {message}");
    }
}
