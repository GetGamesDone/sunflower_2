using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtueSky.Inspector;
using VirtueSky.Misc;


namespace VirtueSky.DataType
{
    [Serializable]
    [DeclareHorizontalGroup("dictionaryCustomNewEntry")]
    public class DictionaryCustom<TKey, TValue> : ISerializationCallbackReceiver, IDictionary, IDictionary<TKey, TValue>
    {
        [TableList(HideAddButton = true), ValidateInput(nameof(ValidateUniqueKeys)), SerializeField]
        private List<DictionaryCustomData<TKey, TValue>> dictionaryData = new List<DictionaryCustomData<TKey, TValue>>();

        [Group("dictionaryCustomNewEntry"), LabelText("Key"), SerializeField]
        private TKey newEntryKey;

        [Group("dictionaryCustomNewEntry"), LabelText("Value"), SerializeField]
        private TValue newEntryValue;

        [Button("Add"), DisableIf(nameof(CannotAddNewEntry))]
        [InfoBox("Key already exists", TriMessageType.Error, nameof(IsNewEntryKeyDuplicate))]
        [InfoBox("$" + nameof(NewEntryValueError), TriMessageType.Error, nameof(HasNewEntryValueError))]
        private void AddNewEntry()
        {
            if (CannotAddNewEntry()) return;

            dictionaryData.Add(new DictionaryCustomData<TKey, TValue>(newEntryKey, newEntryValue));
            UpdateDict();

            newEntryKey = default;
            newEntryValue = default;
        }

        private bool CannotAddNewEntry()
        {
            if (!typeof(TKey).IsValueType && newEntryKey == null) return true;
            if (IsNewEntryKeyDuplicate()) return true;

            return HasNewEntryValueError();
        }

        private bool IsNewEntryKeyDuplicate()
        {
            if (dictionaryData == null) return false;

            foreach (var data in dictionaryData)
            {
                if (data.key != null && EqualityComparer<TKey>.Default.Equals(data.key, newEntryKey))
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasNewEntryValueError() => !IsValueAllowed(newEntryValue, out _);

        private string NewEntryValueError()
        {
            IsValueAllowed(newEntryValue, out var error);
            return error ?? string.Empty;
        }

        // Null passes here since the value field is optional; subclasses only need to reject non-null bad values.
        protected virtual bool IsValueAllowed(TValue value, out string error)
        {
            error = null;
            return true;
        }

        private TriValidationResult ValidateUniqueKeys()
        {
            if (dictionaryData == null || dictionaryData.Count < 2) return TriValidationResult.Valid;

            var seenKeys = new HashSet<TKey>();
            var duplicateKeys = new HashSet<TKey>();

            foreach (var data in dictionaryData)
            {
                if (data.key == null) continue;
                if (!seenKeys.Add(data.key))
                {
                    duplicateKeys.Add(data.key);
                }
            }

            if (duplicateKeys.Count > 0)
            {
                return TriValidationResult.Error($"Duplicate key(s) not allowed: {string.Join(", ", duplicateKeys)}");
            }

            return TriValidationResult.Valid;
        }

        [NonSerialized] private Dictionary<TKey, TValue> m_dict = new Dictionary<TKey, TValue>();
        public Dictionary<TKey, TValue> GetDict => m_dict;

        public void OnAfterDeserialize()
        {
            UpdateDict();
        }

        public void OnBeforeSerialize()
        {
            if (!Application.isPlaying) return;
            UpdateList();
        }

        private void UpdateDict()
        {
            if (dictionaryData is { Count: > 0 })
            {
                if (m_dict is { Count: > 0 })
                {
                    m_dict.Clear();
                }

                foreach (var data in dictionaryData)
                {
                    if (data.key == null || data.value == null) continue;

                    if (m_dict.ContainsKey(data.key))
                    {
                        Debug.LogError($"[DictionaryCustom] Duplicate key '{data.key}' ignored.");
                        continue;
                    }

                    if (!IsValueAllowed(data.value, out var error))
                    {
                        Debug.LogError($"[DictionaryCustom] Invalid value for key '{data.key}': {error}");
                        continue;
                    }

                    m_dict[data.key] = data.value;
                }
            }
        }

        private void UpdateList()
        {
            if (!dictionaryData.IsNullOrEmpty()) {
                dictionaryData.Clear();
            }
            if (m_dict is { Count: > 0 })
            {
                foreach (var kvp in m_dict)
                {
                    dictionaryData.Add(new DictionaryCustomData<TKey, TValue>(kvp.Key, kvp.Value));
                }
            }
        }

        public void Add(object key, object value)
        {
            m_dict.Add((TKey)key, (TValue)value);
            UpdateList();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            m_dict.Add(item.Key, item.Value);
            UpdateList();
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            m_dict.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((IDictionary)m_dict).Contains((TKey)item.Key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((IDictionary<TKey, TValue>)m_dict).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            var result = ((IDictionary<TKey, TValue>)m_dict).Remove(item);
            if (result) UpdateList();
            return result;
        }

        public int Count => m_dict.Count;

        public bool IsReadOnly => ((IDictionary)m_dict).IsReadOnly;


        void IDictionary.Clear()
        {
            ((IDictionary<TKey, TValue>)m_dict).Clear();
        }

        public bool Contains(object key)
        {
            return ((IDictionary<TKey, TValue>)m_dict).Contains((KeyValuePair<TKey, TValue>)key);
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return ((IDictionary<TKey, TValue>)m_dict).GetEnumerator();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return ((IDictionary)m_dict).GetEnumerator();
        }

        public void Remove(object key)
        {
            m_dict.Remove((TKey)key);
            UpdateList();
        }

        public bool IsFixedSize => ((IDictionary)m_dict).IsFixedSize;


        public object this[object key]
        {
            get => ((IDictionary)m_dict)[key];
            set => ((IDictionary)m_dict)[key] = value;
        }

        ICollection IDictionary.Keys => ((IDictionary)m_dict).Keys;

        ICollection<TValue> IDictionary<TKey, TValue>.Values => (ICollection<TValue>)((IDictionary)m_dict).Values;

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => (ICollection<TKey>)((IDictionary)m_dict).Keys;

        ICollection IDictionary.Values => ((IDictionary)m_dict).Values;

        public void Add(TKey key, TValue value)
        {
            m_dict.Add(key, value);
            UpdateList();
        }

        public void Clear()
        {
            m_dict.Clear();
        }

        public bool ContainsKey(TKey key)
        {
            return m_dict.ContainsKey(key);
        }

        public bool ContainsValue(TValue value)
        {
            return m_dict.ContainsValue(value);
        }

        public bool Remove(TKey key)
        {
            var result = m_dict.Remove(key);
            if (result) UpdateList();
            return result;
        }

        public bool Remove(TKey key, out TValue value)
        {
            var result = m_dict.Remove(key, out value);
            if (result) UpdateList();
            return result;
        }

        public int EnsureCapacity(int capacity)
        {
            return m_dict.EnsureCapacity(capacity);
        }

        public virtual bool Equals(object? obj)
        {
            return m_dict.Equals(obj);
        }

        public System.Collections.Generic.Dictionary<TKey, TValue>.Enumerator GetEnumerator()
        {
            return m_dict.GetEnumerator();
        }

        public virtual int GetHashCode()
        {
            return m_dict.GetHashCode();
        }

        public Type GetType()
        {
            return m_dict.GetType();
        }

        public virtual string? ToString()
        {
            return m_dict.ToString();
        }

        public void TrimExcess()
        {
            m_dict.TrimExcess();
        }

        public void TrimExcess(int capacity)
        {
            m_dict.TrimExcess(capacity);
        }

        public bool TryAdd(TKey key, TValue value)
        {
            var result = m_dict.TryAdd(key, value);
            if (result) UpdateList();
            return result;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return m_dict.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get => ((IDictionary<TKey, TValue>)m_dict)[key];
            set => ((IDictionary<TKey, TValue>)m_dict)[key] = value;
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IDictionary)m_dict).GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            ((IDictionary)m_dict).CopyTo(array, index);
        }


        public bool IsSynchronized => ((IDictionary)m_dict).IsSynchronized;
        public object SyncRoot => ((IDictionary)m_dict).SyncRoot;
        public System.Collections.Generic.Dictionary<TKey, TValue>.KeyCollection Keys => m_dict.Keys;
        public System.Collections.Generic.Dictionary<TKey, TValue>.ValueCollection Values => m_dict.Values;
    }

    [Serializable]
    public class DictionaryCustomData<TKey, TValue>
    {
        public TKey key;
        public TValue value;

        public DictionaryCustomData(TKey _key, TValue _value)
        {
            key = _key;
            value = _value;
        }
    }
}