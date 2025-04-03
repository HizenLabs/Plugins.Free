using System;
using System.Collections.Generic;

namespace HizenLabs.FluentUI.Extensions;

/// <summary>
/// Extension methods for <see cref="Dictionary{TKey, TValue}"/>.
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Gets the value associated with the specified key, or adds it using the provided factory function if it doesn't exist.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="dict">The dictionary to operate on.</param>
    /// <param name="key">The key to look for.</param>
    /// <param name="valueFactory">A function to create a value if the key does not exist.</param>
    /// <returns>The value associated with the specified key.</returns>
    public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TValue> valueFactory)
    {
        if (!dict.TryGetValue(key, out var value))
        {
            value = valueFactory();
            dict[key] = value;
        }

        return value;
    }

    /// <summary>
    /// Tries to remove the value associated with the specified key from the dictionary, and returns the value if it was found.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="dict">The dictionary to operate on.</param>
    /// <param name="key">The key to look for.</param>
    /// <param name="value">The value associated with the key if it was found; otherwise, the default value of <typeparamref name="TValue"/>.</param>
    /// <returns><c>true</c> if the key was found and removed; otherwise, <c>false</c>.</returns>
    public static bool TryRemove<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, out TValue value)
    {
        if (dict.TryGetValue(key, out value))
        {
            dict.Remove(key);
            return true;
        }

        value = default;
        return false;
    }
}
