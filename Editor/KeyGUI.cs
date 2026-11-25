// ArchNet · Library Core — key field
// Draws a key as a number, or as a dropdown of enum values when the library uses an enum.
// The dropdown stores the enum's real value, so enums like { Common = 0, Rare = 10 } work.
// Docs: https://archnet-plateform.fr/#LibraryCore

using System;
using UnityEditor;
using UnityEngine;

namespace ArchNet.Library.Editor
{
    public static class KeyGUI
    {
        /// <summary>Key field in a layout: enum dropdown when enumType is set, int field otherwise.</summary>
        public static int KeyField(GUIContent label, int key, Type enumType, params GUILayoutOption[] options)
        {
            if (enumType == null) return EditorGUILayout.IntField(label, key, options);

            string[] names = Enum.GetNames(enumType);
            int[] values = (int[])Enum.GetValues(enumType);
            int index = Array.IndexOf(values, key);
            string[] shown = names;
            if (index < 0)
            {
                // Keep showing a key the enum no longer has, instead of silently changing it.
                shown = new string[names.Length + 1];
                names.CopyTo(shown, 0);
                shown[names.Length] = $"(missing: {key})";
                index = names.Length;
            }
            int picked = EditorGUILayout.Popup(label, index, Array.ConvertAll(shown, n => new GUIContent(n)), options);
            return picked < values.Length ? values[picked] : key;
        }

        /// <summary>Key field for a library: uses the library's enum when it has one.</summary>
        public static int KeyField(GUIContent label, int key, KeyedLibrary library, params GUILayoutOption[] options) =>
            KeyField(label, key, library != null ? library.EnumType : null, options);

        /// <summary>Help box when the enum name does not match any enum. Returns true when the key setup is usable.</summary>
        public static bool CheckEnum(LibraryKeyType keyType, string enumPath)
        {
            if (keyType != LibraryKeyType.Enum || EnumTypes.Find(enumPath) != null) return keyType != LibraryKeyType.None;
            EditorGUILayout.HelpBox("Type the full name of an enum, namespace included (e.g. MyGame.Theme).", MessageType.Warning);
            return false;
        }
    }
}
