using System;
using System.Collections.Generic;
using System.Linq;

namespace DamnORM.Helpers
{
    /// <summary>
    /// Extension methods for IEnumerables
    /// </summary>
    public static class IEnumerableHelper
    {
        /// <summary>
        /// Returns all the items that are equall to default(T).
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="source">The source.</param>
        /// <returns>Items that are equal to default(T)</returns>
        public static IEnumerable<T> IsDefault<T>(this IEnumerable<T> source)
        {
            return source.Where(item => object.Equals(item, default(T)));
        }

        /// <summary>
        /// Returns all the items that are not equall to default(T).
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="source">The source.</param>
        /// <returns>Items that are not equal to default(T)</returns>
        public static IEnumerable<T> IsNotDefault<T>(this IEnumerable<T> source)
        {
            return source.Where(item => !object.Equals(item, default(T)));
        }

        /// <summary>
        /// Executes a void function on each elements in the enumerable.<para />
        /// Each iteration passes the current item to the function.
        /// </summary>
        /// <typeparam name="T">The type of items in the enumerable</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="action">A function that accepts an item of type T.</param>
        /// <returns>THe original enumerable</returns>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
            }

            return source;
        }

        /// <summary>
        /// Executes a value returning function on each elements in the enumerable.<para />
        /// Each iteration passes the current item to the function.
        /// </summary>
        /// <typeparam name="T">The type of items in the enumerable</typeparam>
        /// <typeparam name="R">The return type of the function</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="action">A function that accepts an item of type T, and returns value of type R.</param>
        /// <returns>An enumerable of type R</returns>
        public static IEnumerable<R> ForEach<T, R>(this IEnumerable<T> source, Func<T, R> action)
        {
            foreach (var item in source)
            {
                yield return action(item);
            }

            yield break;
        }

        /// <summary>
        /// Executes a value returning function on each elements in the enumerable.<para />
        /// Each iteration passes the current item and it's index to the function.
        /// </summary>
        /// <typeparam name="T">The type of items in the enumerable</typeparam>
        /// <typeparam name="R">The return type of the function</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="action">A function that accepts an item of type T, and an int, and returns value of type R.</param>
        /// <returns>An enumerable of type R</returns>
        public static IEnumerable<R> ForEach<T, R>(this IEnumerable<T> source, Func<T, int, R> action)
        {
            var index = 0;

            foreach (var item in source)
            {
                yield return action(item, index);
                index++;
            }

            yield break;
        }
    }
}