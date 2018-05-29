using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEV.Common
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Sets a dictionary element regardless of the key already existing in the dictionary
        /// </summary>
        /// <typeparam name="K">Type of the key.</typeparam>
        /// <typeparam name="V">Type of the value.</typeparam>
        /// <param name="dict">The dictionary.</param>
        /// <param name="key">Key to be set. If the key already exists in the dictionary, the value gets overwritten.</param>
        /// <param name="value">Value to be set.</param>
        /// <returns>Returns <code>true</code> if the key was already in the dictionary.</returns>
        public static bool Set<K, V>(this Dictionary<K, V> dict, K key, V value)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = value;
                return true;
            }
            else
            {
                dict.Add(key, value);
                return false;
            }
        }

    }
}
