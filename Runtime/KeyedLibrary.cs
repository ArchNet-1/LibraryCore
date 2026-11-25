// ArchNet · Library Core
// The base of every ArchNet library: a ScriptableObject that stores values by key, the key
// being an int or an enum value. ColorLibrary, MaterialLibrary, PrefabLibrary and
// AudioLibrary are KeyedLibrary<Color>, <Material>, <GameObject> and <AudioClip>.
// Make your own in one line:  public class FontLibrary : KeyedLibrary<Font> { }
// Docs: https://archnet-plateform.fr/#LibraryCore

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ArchNet.Library
{
    /// <summary>What a library's values are looked up by.</summary>
    public enum LibraryKeyType { None, Enum, Int }

    /// <summary>Finds enums by their full name (namespace included), with a cache.</summary>
    public static class EnumTypes
    {
        static readonly Dictionary<string, Type> _cache = new Dictionary<string, Type>();

        public static Type Find(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return null;
            if (_cache.TryGetValue(fullName, out Type cached)) return cached;
            Type found = null;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(fullName);
                if (type != null && type.IsEnum) { found = type; break; }
            }
            if (found != null) _cache[fullName] = found; // keep looking next time while not found
            return found;
        }
    }

    /// <summary>Key settings shared by every library; inspectors and components use this side.</summary>
    public abstract class KeyedLibrary : ScriptableObject
    {
        [SerializeField] LibraryKeyType _keyType = LibraryKeyType.None;
        [Tooltip("Full name of the enum, namespace included (e.g. MyGame.Theme).")]
        [SerializeField] string _enumPath;

        public LibraryKeyType KeyType => _keyType;

        /// <summary>The enum used as key, or null when the library is not keyed by an enum.</summary>
        public Type EnumType => _keyType == LibraryKeyType.Enum ? EnumTypes.Find(_enumPath) : null;

        /// <summary>True when the library is keyed by this enum type.</summary>
        public bool IsKeyedBy(Type enumType) => enumType != null && enumType == EnumType;

        /// <summary>Every key that has a value.</summary>
        public abstract IEnumerable<int> Keys { get; }

        /// <summary>Readable name of a key: the enum value name, or the number.</summary>
        public string KeyLabel(int key)
        {
            Type enumType = EnumType;
            return enumType != null && Enum.IsDefined(enumType, key) ? Enum.GetName(enumType, key) : key.ToString();
        }

        /// <summary>Forgets cached lookups; called after any change to the entries.</summary>
        public abstract void ClearCache();

        protected virtual void OnValidate() => ClearCache();
    }

    /// <summary>One value and the key it answers to.</summary>
    [Serializable]
    public class KeyedEntry<TValue>
    {
        [FormerlySerializedAs("_colorKey"), FormerlySerializedAs("MaterialKey"), FormerlySerializedAs("PrefabKey")]
        public int key;
        [FormerlySerializedAs("_colorValue"), FormerlySerializedAs("MaterialValue"), FormerlySerializedAs("PrefabValue")]
        public TValue value;

        public KeyedEntry(int key, TValue value)
        {
            this.key = key;
            this.value = value;
        }
    }

    /// <summary>A library of <typeparamref name="TValue"/> looked up by an int or an enum value.</summary>
    public abstract class KeyedLibrary<TValue> : KeyedLibrary
    {
        [FormerlySerializedAs("_colorList"), FormerlySerializedAs("_materialList"), FormerlySerializedAs("_prefabList"), FormerlySerializedAs("database")]
        [SerializeField] List<KeyedEntry<TValue>> _entries = new List<KeyedEntry<TValue>>();

        Dictionary<int, TValue> _lookup;

        public IReadOnlyList<KeyedEntry<TValue>> Entries => _entries;
        public override IEnumerable<int> Keys => Lookup().Keys;

        /// <summary>Value for an int key, or default (with a warning) when none is set.</summary>
        public TValue Get(int key)
        {
            if (TryGet(key, out TValue value)) return value;
            Debug.LogWarning($"[{name}] Nothing stored for key {KeyLabel(key)}.", this);
            return default;
        }

        /// <summary>Value for an enum key, or default (with a warning) when none is set.</summary>
        public TValue Get(Enum key)
        {
            if (key.GetType() != EnumType)
            {
                Debug.LogWarning($"[{name}] Asked with {key.GetType().Name}, but this library is keyed by {(EnumType?.Name ?? KeyType.ToString())}.", this);
            }
            return Get(Convert.ToInt32(key));
        }

        public bool TryGet(int key, out TValue value) => Lookup().TryGetValue(key, out value);

        public bool Contains(int key) => Lookup().ContainsKey(key);

        /// <summary>Stores a value for a key, replacing the previous one (not saved in builds).</summary>
        public void Set(int key, TValue value)
        {
            int index = _entries.FindIndex(e => e.key == key);
            if (index >= 0) _entries[index].value = value;
            else _entries.Add(new KeyedEntry<TValue>(key, value));
            ClearCache();
        }

        public override void ClearCache() => _lookup = null;

        Dictionary<int, TValue> Lookup()
        {
            if (_lookup != null) return _lookup;
            _lookup = new Dictionary<int, TValue>();
            foreach (KeyedEntry<TValue> entry in _entries)
            {
                if (!_lookup.TryAdd(entry.key, entry.value))
                {
                    Debug.LogWarning($"[{name}] Key {KeyLabel(entry.key)} is set twice; the first value is used.", this);
                }
            }
            return _lookup;
        }
    }
}
