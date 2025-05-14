namespace Code.Scripts.Common
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public class SerializableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<TKey> m_keys = new();
        [SerializeField] private List<TValue> m_values = new();

        private Dictionary<TKey, TValue> m_dictionary = new();

        // Sync serialized data into dictionary
        public void OnAfterDeserialize()
        {
            m_dictionary.Clear();

            if (m_keys.Count != m_values.Count)
            {
                Debug.LogError(
                    $"[SerializableDictionary] Deserialization error: keys and values count mismatch. Keys: {m_keys.Count}, Values: {m_values.Count}");
                return;
            }

            for (int i = 0; i < m_keys.Count; i++)
            {
                if (m_keys[i] != null && !m_dictionary.ContainsKey(m_keys[i]))
                    m_dictionary.Add(m_keys[i], m_values[i]);
            }
        }

        // Sync dictionary into serialized data
        public void OnBeforeSerialize()
        {
            m_keys.Clear();
            m_values.Clear();

            foreach (var kvp in m_dictionary)
            {
                m_keys.Add(kvp.Key);
                m_values.Add(kvp.Value);
            }
        }

        #region IDictionary Implementation

        public TValue this[TKey key]
        {
            get => m_dictionary[key];
            set
            {
                m_dictionary[key] = value;
                SyncLists();
            }
        }

        public ICollection<TKey> Keys => m_dictionary.Keys;
        public ICollection<TValue> Values => m_dictionary.Values;
        public int Count => m_dictionary.Count;
        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value)
        {
            m_dictionary.Add(key, value);
            SyncLists();
        }

        public bool ContainsKey(TKey key) => m_dictionary.ContainsKey(key);

        public bool Remove(TKey key)
        {
            if (m_dictionary.Remove(key))
            {
                SyncLists();
                return true;
            }

            return false;
        }

        public bool TryGetValue(TKey key, out TValue value) => m_dictionary.TryGetValue(key, out value);

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            m_dictionary.Add(item.Key, item.Value);
            SyncLists();
        }

        public void Clear()
        {
            m_dictionary.Clear();
            m_keys.Clear();
            m_values.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) => m_dictionary.ContainsKey(item.Key) &&
                                                                 EqualityComparer<TValue>.Default.Equals(
                                                                     m_dictionary[item.Key], item.Value);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            foreach (var kvp in m_dictionary)
            {
                array[arrayIndex++] = kvp;
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (Contains(item))
            {
                return Remove(item.Key);
            }

            return false;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => m_dictionary.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => m_dictionary.GetEnumerator();

        #endregion

        private void SyncLists()
        {
            m_keys.Clear();
            m_values.Clear();
            foreach (var kvp in m_dictionary)
            {
                m_keys.Add(kvp.Key);
                m_values.Add(kvp.Value);
            }
        }
    }
}