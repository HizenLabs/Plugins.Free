using Facepunch;
using HizenLabs.Extensions.ObjectSerializer.Enums;
using HizenLabs.Extensions.ObjectSerializer.Extensions;
using System;
using System.Collections.Generic;
using System.IO;

namespace HizenLabs.Extensions.ObjectSerializer.Internal;

/// <summary>
/// Provides methods for reading generic instances of <typeparamref name="T"/> from a <see cref="BinaryReader"/>.
/// </summary>
/// <typeparam name="T">The type of the object to read.</typeparam>
internal class GenericListReader<T>
{
    /// <summary>
    /// The delegate that reads a list of <typeparamref name="T"/> from the <see cref="BinaryReader"/>.
    /// </summary>
    public static Func<BinaryReader, List<T>, List<T>> Read { get; }

    /// <summary>
    /// Initializes the <see cref="GenericListReader{T}"/> class.
    /// </summary>
    static GenericListReader()
    {
        var type = typeof(T);

        Read = (reader, list) =>
        {
            list ??= Pool.Get<List<T>>();

            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var marker = reader.ReadEnum<TypeMarker>();
                if (marker == TypeMarker.Null)
                {
                    list.Add(default!);
                    continue;
                }

                var item = GenericReader<T>.Read(reader);
                list.Add(item);
            }

            return list;
        };
    }
}
