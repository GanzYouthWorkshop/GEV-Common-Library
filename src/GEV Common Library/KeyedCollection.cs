using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEV.Common
{
    /// <summary>
    /// Enables the storage of keyed objects without extra mechanisms.
    /// </summary>
    /// <typeparam name="T">Type for contained objects.</typeparam>
    public class KeyedCollection<T> : List<T>
    {
        private Func<T, IComparable> m_ItemKeyPredicate;

        /// <summary>
        /// Created a keyed collection.
        /// </summary>
        /// <param name="itemKey">Predicate describing the key - example: <code>item => item.Name</code>.</param>
        public KeyedCollection(Func<T, IComparable> itemKey)
        {
            this.m_ItemKeyPredicate = itemKey;
        }

        /// <summary>
        /// Returns the object with the given key.
        /// </summary>
        /// <param name="o">Key </param>
        /// <returns></returns>
        public T this[object o]
        {
            get
            {
                return this.FirstOrDefault(item => this.m_ItemKeyPredicate(item) == o);
            }

            set
            {
                try
                {
                    T item = this[o];

                    //If item is not in the list, throws an exception
                    this.Remove(item);
                }
                finally
                {
                    //Once the older item is removed (or exception is handled) we add the new item
                    this.Add(value);
                }
            }
        }
    }
}
