using HizenLabs.Extensions.ObjectSerializer.Internal.Delegates;
using System.IO;
using System;
using HizenLabs.Extensions.ObjectSerializer.Extensions;

namespace HizenLabs.Extensions.ObjectSerializer.Internal;

/// <summary>
/// Provides methods for writing generic instances of <typeparamref name="T"/> arrays to a <see cref="BinaryWriter"/>.
/// </summary>
/// typeparam name="T">The type of the object to write.</typeparam>
internal class GenericArrayWriter<T>
{
    /// <summary>
    /// The delegate that writes an array of <typeparamref name="T"/> to the <see cref="BinaryWriter"/>.
    /// </summary>
    public static Action<BinaryWriter, T[], int, int> Write { get; }

    static GenericArrayWriter()
    {
        var elementType = typeof(T);

        if (elementType.IsArray) throw new NotSupportedException("Multi-dimensional arrays are not supported.");
        else if (elementType == typeof(byte)) Write = (w, v, index, count) =>
        {
            if (v is not byte[] buffer) throw new ArgumentException("Buffer must be a byte array.", nameof(v));
            w.Write(buffer, index, count);
        };
        else
        {
            var elementWriter = GenericDelegateFactory.BuildProperty<Action<BinaryWriter, T>>(
                genericTypeDef: typeof(GenericWriter<>),
                typeArgs: elementType,
                name: nameof(GenericWriter<int>.Write)
            );

            Write = (w, v, index, count) =>
            {
                if (v is not T[] array) throw new ArgumentException("Expected array.", nameof(v));

                w.Write(elementType);
                w.Write(count);

                for (int i = 0; i < count; i++)
                {
                    elementWriter(w, array[index + i]);
                }
            };
        }
    }
}