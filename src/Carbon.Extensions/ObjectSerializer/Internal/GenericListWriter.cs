using HizenLabs.Extensions.ObjectSerializer.Enums;
using HizenLabs.Extensions.ObjectSerializer.Extensions;
using System;
using System.Collections.Generic;
using System.IO;

namespace HizenLabs.Extensions.ObjectSerializer.Internal;

/// <summary>
/// Provides methods for writing generic instances of <typeparamref name="T"/> to a <see cref="BinaryWriter"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
internal class GenericListWriter<T>
{
    /// <summary>
    /// The delegate that writes a list of <typeparamref name="T"/> to the <see cref="BinaryWriter"/>.
    /// </summary>
    public static Action<BinaryWriter, List<T>> Write { get; }

    /// <summary>
    /// Initializes the <see cref="GenericListWriter{T}"/> class.
    /// </summary>
    static GenericListWriter()
    {
        Write = (writer, list) =>
        {
            writer.Write(list.Count);
            foreach (var item in list)
            {
                if (item is null)
                {
                    writer.WriteEnum(TypeMarker.Null);
                }
                else
                {
                    writer.WriteEnum(item.GetType().GetTypeMarker());
                    GenericWriter<T>.Write(writer, item);
                }
            }
        };
    }
}
