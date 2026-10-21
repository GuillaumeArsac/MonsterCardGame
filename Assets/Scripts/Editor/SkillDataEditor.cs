using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using MonsterCardGame.Gameplay.Skills;
using MonsterCardGame.Gameplay.Skills.Effects;

namespace MonsterCardGame.Editor
{
    /// <summary>
    /// Inspecteur de SkillData : ajoute un bouton pour créer une brique d'effet,
    /// car Unity ne propose pas de choisir le type d'un élément [SerializeReference].
    /// </summary>
    [CustomEditor(typeof(SkillData))]
    public class SkillDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "_effects");

            var effects = serializedObject.FindProperty("_effects");
            EditorGUILayout.LabelField("Effets", EditorStyles.boldLabel);

            for (int i = 0; i < effects.arraySize; i++)
            {
                var effect = effects.GetArrayElementAtIndex(i);
                string typeName = effect.managedReferenceValue?.GetType().Name ?? "(vide)";

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField($"{i + 1}. {typeName}", EditorStyles.miniBoldLabel);
                    if (GUILayout.Button("Retirer", GUILayout.Width(70)))
                    {
                        effects.DeleteArrayElementAtIndex(i);
                        break;
                    }
                }

                EditorGUI.indentLevel++;
                foreach (var child in GetChildren(effect))
                    EditorGUILayout.PropertyField(child, true);
                EditorGUI.indentLevel--;
            }

            if (GUILayout.Button("Ajouter un effet"))
                ShowAddMenu();

            serializedObject.ApplyModifiedProperties();
        }

        private void ShowAddMenu()
        {
            var menu = new GenericMenu();
            var types = TypeCache.GetTypesDerivedFrom<SkillEffect>()
                                 .Where(t => !t.IsAbstract)
                                 .OrderBy(t => t.Name);

            foreach (var type in types)
                menu.AddItem(new GUIContent(type.Name), false, () => AddEffect(type));

            menu.ShowAsContext();
        }

        private void AddEffect(Type type)
        {
            serializedObject.Update();
            var effects = serializedObject.FindProperty("_effects");
            effects.arraySize++;
            effects.GetArrayElementAtIndex(effects.arraySize - 1).managedReferenceValue = Activator.CreateInstance(type);
            serializedObject.ApplyModifiedProperties();
        }

        private static IEnumerable<SerializedProperty> GetChildren(SerializedProperty property)
        {
            var current = property.Copy();
            var end     = property.GetEndProperty();
            if (!current.NextVisible(true)) yield break;

            while (!SerializedProperty.EqualContents(current, end))
            {
                yield return current.Copy();
                if (!current.NextVisible(false)) yield break;
            }
        }
    }
}
