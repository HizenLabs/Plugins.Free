using System;
using System.Collections.Generic;

namespace HizenLabs.Extensions.ObjectSerializer.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Dictionary{TKey, TValue}"/> class.
/// </summary>
internal static class DictionaryExtensions
{
    /// <summary>
    /// Disposes all values in the dictionary that implement <see cref="IDisposable"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary whose values will be disposed.</param>
    public static void DisposeValues<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
    {
        foreach (var value in dictionary.Values)
        {
            if (value is null) continue;
            if (value is IDisposable disposable)
            {
                disposable?.Dispose();
            }
        }
    }
}
