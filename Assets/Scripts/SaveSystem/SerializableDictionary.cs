using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerializableDictionary<Tkey, TValue> : Dictionary<Tkey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField] private List<Tkey> keys = new List<Tkey>();
    [SerializeField] private List<TValue> values = new List<TValue>();


    public void OnAfterDeserialize()
    {
        this.Clear();

        if (keys.Count != values.Count)
        {
            Debug.LogError("Mismatched keys and values count after deserialization. Keys count: " + keys.Count + ", Values count: " + values.Count);
            return;
        }

        for (int i = 0; i < keys.Count; i++)
        {
            this.Add(keys[i], values[i]);
        }

    }


    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();

        foreach (KeyValuePair<Tkey, TValue> pairs in this)
        {
            keys.Add(pairs.Key);
            values.Add(pairs.Value);
        }
    }
}
