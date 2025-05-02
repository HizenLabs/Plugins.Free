using HizenLabs.Extensions.ObjectSerializer.Internal.Delegates;
using System;
using System.IO;

namespace HizenLabs.Extensions.ObjectSerializer.Internal;

/// <summary>
/// Provides methods for reading generic instances of <typeparamref name="T"/> arrays from a <see cref="BinaryReader"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
internal class GenericArrayReader<T>
{
    /// <summary>
    /// The delegate that reads an array of <typeparamref name="T"/> from the <see cref="BinaryReader"/>.
    /// </summary>
    public static Action<BinaryReader, T[], int, int> Read { get; }

    /// <summary>
    /// Initializes the <see cref="GenericArrayReader{T}"/> class.
    /// </summary>
    static GenericArrayReader()
    {
        var elementType = typeof(T);
        if (elementType.IsArray) throw new NotSupportedException("Multi-dimensional arrays are not supported.");
        else if (elementType == typeof(byte)) Read = (r, v, index, count) =>
        {
            if (v is not byte[] buffer) throw new ArgumentException("Buffer must be a byte array.", nameof(v));

            r.Read(buffer, index, count);
        };
        else
        {
            var elementReader = GenericDelegateFactory.BuildProperty<Func<BinaryReader, T>>(
                genericTypeDef: typeof(GenericReader<>),
                typeArgs: elementType,
                name: nameof(GenericReader<int>.Read)
            );

            Read = (r, buffer, index, count) =>
            {
                for (int i = 0; i < count; i++)
                {
                    buffer[index + i] = elementReader(r);
                }
            };
        }
    }
}
