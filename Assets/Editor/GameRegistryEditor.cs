using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using MonsterCardGame.Gameplay.Cards;
using MonsterCardGame.Gameplay.Combat.Data;
using MonsterCardGame.Gameplay.Inventory;
using MonsterCardGame.Gameplay.World;

namespace MonsterCardGame.Editor
{
    [CustomEditor(typeof(GameRegistry))]
    public class GameRegistryEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Outils", EditorStyles.boldLabel);

            if (GUILayout.Button("Auto-remplir depuis le projet"))
                AutoFill();

            if (GUILayout.Button("Vérifier les oublis"))
                CheckMissing();
        }

        private void AutoFill()
        {
            var registry = (GameRegistry)target;

            var cards     = FindAll<CardData>();
            var materials = FindAll<MaterialData>();
            var monsters  = FindAll<MonsterData>();

            var so = serializedObject;
            so.Update();
            SetList(so.FindProperty("_allCards"),     cards);
            SetList(so.FindProperty("_allMaterials"), materials);
            SetList(so.FindProperty("_allMonsters"),  monsters);
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();

            Debug.Log($"[GameRegistry] Auto-remplissage terminé : {cards.Count} carte(s), {materials.Count} matériau(x), {monsters.Count} monstre(s).");
        }

        private void CheckMissing()
        {
            var registry = (GameRegistry)target;

            var missing = new List<string>();
            CheckList(FindAll<CardData>(),     registry.AllCards,     "CardData",     missing);
            CheckList(FindAll<MaterialData>(), registry.AllMaterials, "MaterialData", missing);
            CheckList(FindAll<MonsterData>(),  registry.AllMonsters,  "MonsterData",  missing);

            if (missing.Count == 0)
                Debug.Log("[GameRegistry] Aucun oubli — tous les assets sont référencés.");
            else
                Debug.LogWarning($"[GameRegistry] {missing.Count} asset(s) non référencé(s) :\n" + string.Join("\n", missing));
        }

        // ── Helpers ────────────────────────────────────────────────────────

        private static List<T> FindAll<T>() where T : ScriptableObject
        {
            var results = new List<T>();
            var guids   = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            foreach (var guid in guids)
            {
                var path  = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null) results.Add(asset);
            }
            return results;
        }

        private static void SetList<T>(SerializedProperty prop, List<T> items) where T : ScriptableObject
        {
            prop.ClearArray();
            prop.arraySize = items.Count;
            for (int i = 0; i < items.Count; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        }

        private static void CheckList<T>(List<T> inProject, System.Collections.Generic.IReadOnlyList<T> inRegistry, string typeName, List<string> missing)
            where T : ScriptableObject
        {
            var registrySet = new HashSet<T>(inRegistry);
            foreach (var asset in inProject)
                if (!registrySet.Contains(asset))
                    missing.Add($"  [{typeName}] {asset.name}  ({AssetDatabase.GetAssetPath(asset)})");
        }
    }
}
