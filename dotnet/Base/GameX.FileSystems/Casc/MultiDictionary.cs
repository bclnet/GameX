﻿using System.Collections.Generic;

namespace GameX.FileSystems.Casc
{
    public class MultiDictionary<K, V> : Dictionary<K, List<V>>
    {
        public void Add(K key, V value)
        {
            if (TryGetValue(key, out List<V> hset))
            {
                hset.Add(value);
            }
            else
            {
                hset = new List<V>
                {
                    value
                };
                base[key] = hset;
            }
        }

        public new void Clear()
        {
            foreach (var kv in this)
            {
                kv.Value.Clear();
            }

            base.Clear();
        }
    }
}
