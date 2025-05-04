using HizenLabs.Extensions.ObjectSerializer.Enums;
using HizenLabs.Extensions.ObjectSerializer.Extensions;
using System;
using System.Collections.Generic;
using System.IO;

namespace HizenLabs.Extensions.ObjectSerializer.Internal;

/// <summary>
/// Provides methods for writing generic instances of <see cref="Dictionary{TKey, TValue}"/> to a <see cref="BinaryWriter"/>.
/// </summary>
/// typeparam name="TKey">The type of the key in the dictionary.</typeparam>
/// typeparam name="TValue">The type of the value in the dictionary.</typeparam>
internal class GenericDictionaryWriter<TKey, TValue>
{
    /// <summary>
    /// The delegate that writes the <see cref="Dictionary{TKey, TValue}"/> to the <see cref="BinaryWriter"/>.
    /// </summary>
    public static Action<BinaryWriter, Dictionary<TKey, TValue>> Write { get; }

    /// <summary>
    /// Initializes the <see cref="GenericDictionaryWriter{TKey, TValue}"/> class.
    /// </summary>
    static GenericDictionaryWriter()
    {
        if (typeof(TValue).CanBeNull())
        {
            Write = (writer, dict) =>
            {
                writer.Write(dict.Count);
                foreach (var item in dict)
                {
                    // Dictionary keys can't be null
                    GenericWriter<TKey>.Write(writer, item.Key);

                    if (item.Value is null)
                    {
                        writer.WriteEnum(TypeMarker.Null);
                    }
                    else
                    {
                        writer.WriteEnum(item.Value.GetType().GetTypeMarker());
                        GenericWriter<TValue>.Write(writer, item.Value);
                    }
                }
            };
        }
        else // skip TypeMarker for non-nullable types
        {
            Write = (writer, dict) =>
            {
                writer.Write(dict.Count);
                foreach (var item in dict)
                {
                    GenericWriter<TKey>.Write(writer, item.Key);
                    GenericWriter<TValue>.Write(writer, item.Value);
                }
            };
        }
    }
}
