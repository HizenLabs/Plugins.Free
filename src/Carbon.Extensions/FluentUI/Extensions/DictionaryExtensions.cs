using System;
using System.Collections.Generic;

namespace HizenLabs.FluentUI.Extensions;

internal static class DictionaryExtensions
{
    public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TValue> valueFactory)
    {
        if (!dict.TryGetValue(key, out var value))
        {
            value = valueFactory();
            dict[key] = value;
        }

        return value;
    }

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
