using Facepunch;
using HizenLabs.Extensions.ObjectSerializer.Enums;
using HizenLabs.Extensions.ObjectSerializer.Extensions;
using System;
using System.Collections.Generic;
using System.IO;

namespace HizenLabs.Extensions.ObjectSerializer.Internal;

/// <summary>
/// Provides methods for reading generic instances of <see cref="Dictionary{TKey, TValue}"/> from a <see cref="BinaryReader"/>.
/// </summary>
/// typeparam name="TKey">The type of the key in the dictionary.</typeparam>
/// typeparam name="TValue">The type of the value in the dictionary.</typeparam>
internal class GenericDictionaryReader<TKey, TValue>
{
    /// <summary>
    /// The delegate that reads the <see cref="Dictionary{TKey, TValue}"/> from the <see cref="BinaryReader"/>.
    /// </summary>
    public static Func<BinaryReader, Dictionary<TKey, TValue>, Dictionary<TKey, TValue>> Read { get; }

    /// <summary>
    /// Initializes the <see cref="GenericDictionaryReader{TKey, TValue}"/> class.
    /// </summary>
    static GenericDictionaryReader()
    {
        if (typeof(TValue).CanBeNull())
        {
            Read = (reader, dict) =>
            {
                dict ??= Pool.Get<Dictionary<TKey, TValue>>();

                var count = reader.ReadInt32();
                for (var i = 0; i < count; i++)
                {
                    var key = GenericReader<TKey>.Read(reader);
                    var valueMarker = reader.ReadEnum<TypeMarker>();
                    if (valueMarker == TypeMarker.Null)
                    {
                        dict.Add(key, default!);
                    }
                    else
                    {
                        var value = GenericReader<TValue>.Read(reader);
                        dict.Add(key, value);
                    }
                }

                return dict;
            };
        }
        else // skip TypeMarker for non-nullable types
        {
            Read = (reader, dict) =>
            {
                dict ??= Pool.Get<Dictionary<TKey, TValue>>();
                var count = reader.ReadInt32();
                for (var i = 0; i < count; i++)
                {
                    var key = GenericReader<TKey>.Read(reader);
                    var value = GenericReader<TValue>.Read(reader);
                    dict.Add(key, value);
                }
                return dict;
            };
        }
    }
}
