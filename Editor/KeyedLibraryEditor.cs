// ArchNet · Library Core — inspector
// One inspector for every KeyedLibrary: pick the key type, then add a value per key.
// Duplicate keys are flagged.
// Docs: https://archnet-plateform.fr/#LibraryCore

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArchNet.Library.Editor
{
    [CustomEditor(typeof(KeyedLibrary), true)]
    public class KeyedLibraryEditor : UnityEditor.Editor
    {
        SerializedProperty _keyType, _enumPath, _entries;

        void OnEnable()
        {
            _keyType = serializedObject.FindProperty("_keyType");
            _enumPath = serializedObject.FindProperty("_enumPath");
            _entries = serializedObject.FindProperty("_entries");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var library = (KeyedLibrary)target;

            EditorGUILayout.PropertyField(_keyType, new GUIContent("Key Type"));
            var keyType = (LibraryKeyType)_keyType.enumValueIndex;
            if (keyType == LibraryKeyType.Enum) EditorGUILayout.PropertyField(_enumPath, new GUIContent("Enum (full name)"));

            EditorGUILayout.Space();
            if (KeyGUI.CheckEnum(keyType, _enumPath.stringValue))
            {
                Type enumType = keyType == LibraryKeyType.Enum ? EnumTypes.Find(_enumPath.stringValue) : null;
                DrawEntries(enumType);
            }
            else if (keyType == LibraryKeyType.None)
            {
                EditorGUILayout.HelpBox("Pick a key type: Enum or Int.", MessageType.Info);
            }

            if (serializedObject.ApplyModifiedProperties()) library.ClearCache();
        }

        void DrawEntries(Type enumType)
        {
            EditorGUILayout.LabelField("Values", EditorStyles.boldLabel);
            var seen = new HashSet<int>();
            var duplicates = new List<string>();

            for (int i = 0; i < _entries.arraySize; i++)
            {
                SerializedProperty entry = _entries.GetArrayElementAtIndex(i);
                SerializedProperty key = entry.FindPropertyRelative("key");
                using (new EditorGUILayout.HorizontalScope())
                {
                    key.intValue = KeyGUI.KeyField(GUIContent.none, key.intValue, enumType, GUILayout.Width(160));
                    EditorGUILayout.PropertyField(entry.FindPropertyRelative("value"), GUIContent.none);
                    if (GUILayout.Button("−", GUILayout.Width(22)))
                    {
                        _entries.DeleteArrayElementAtIndex(i);
                        break;
                    }
                }
                if (!seen.Add(key.intValue)) duplicates.Add(((KeyedLibrary)target).KeyLabel(key.intValue));
            }

            if (duplicates.Count > 0)
                EditorGUILayout.HelpBox($"Used twice: {string.Join(", ", duplicates.Distinct())}. Only the first value is used.", MessageType.Warning);

            if (GUILayout.Button("Add"))
            {
                _entries.arraySize++;
                SerializedProperty added = _entries.GetArrayElementAtIndex(_entries.arraySize - 1);
                added.FindPropertyRelative("key").intValue = NextFreeKey(seen, enumType);
            }
        }

        static int NextFreeKey(HashSet<int> used, Type enumType)
        {
            if (enumType != null)
            {
                foreach (int value in (int[])Enum.GetValues(enumType))
                    if (!used.Contains(value)) return value;
            }
            int key = 0;
            while (used.Contains(key)) key++;
            return key;
        }
    }
}
