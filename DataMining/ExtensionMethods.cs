using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMiningIndividual
{
    /// <summary>
    /// A collection of my own helper-extension methods.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Used to perform an action upon a given enumeration where
        /// the index is also needed. This is useful at the end of
        /// a series of LINQ calls.
        /// </summary>
        /// <typeparam name="T">The type of object in the enumeration.</typeparam>
        /// <param name="list">The enumeration it is called on.</param>
        /// <param name="action">The action to perform on each element of the enumeration. 
        /// Taking one object and its index.</param>
        public static void ForEach<T>(this IEnumerable<T> list, System.Action<T,int> action)
        {
            int i = 0;
            foreach (T item in list)
            {
                action(item, i);
                i++;
            }
        }

        /// <summary>
        /// Used to perform an action upon a given enumeration. This is useful at the end of
        /// a series of LINQ calls.
        /// </summary>
        /// <typeparam name="T">The type of object in the enumeration.</typeparam>
        /// <param name="list">The enumeration it is called on.</param>
        /// <param name="action">The action to perform on each element of the enumeration,
        /// taking one object.</param>
        public static void ForEach<T>(this IEnumerable<T> list, System.Action<T> action)
        {
            foreach (T item in list)
            {
                action(item);
            }
        }

        /// <summary>
        /// Shuffles the content of the list using the seed for the randomization.
        /// </summary>
        /// <typeparam name="T">The type of object in the list.</typeparam>
        /// <param name="list">The list it is called on.</param>
        /// <param name="seed">The seed used in the generation of random numbers
        /// for the shuffling.</param>
        public static IList<T> Shuffle<T>(this IList<T> list, int seed)
        {
            Random rng = new Random(seed);
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }
    }
}
