using System;
using System.IO;
using System.Runtime.InteropServices;

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
        else if (elementType.IsPrimitive)
        {
            int typeSize = Marshal.SizeOf<T>();
            Read = (r, v, index, count) =>
            {
                if (v is not T[] array) throw new ArgumentException("Expected array.", nameof(v));

                var size = r.ReadInt32();
                if (size != (count * typeSize)) throw new ArgumentException($"Expected length {count}, but got {size}.");

                using var buffer = new PooledBuffer();
                int read = 0;
                while (read < size)
                {
                    int remaining = size - read;
                    int chunkBytes = Math.Min(buffer.Length, remaining);

                    // Read chunk
                    var readBytes = r.Read(buffer, 0, chunkBytes);
                    if (readBytes != chunkBytes)
                    {
                        throw new EndOfStreamException($"Expected {chunkBytes} bytes, but got {readBytes}.");
                    }

                    // Copy partial block
                    Buffer.BlockCopy(buffer, 0, array, index * typeSize + read, chunkBytes);
                    read += chunkBytes;
                }
            };
        }
        else
        {
            Read = (r, v, index, count) =>
            {
                if (v is not T[] array) throw new ArgumentException("Expected array.", nameof(v));

                var size = r.ReadInt32();
                if (size != count) throw new ArgumentException($"Expected length {count}, but got {size}.");

                for (int i = 0; i < count; i++)
                {
                    var item = GenericReader<T>.Read(r);
                    array[index + i] = item;
                }
            };
        }
    }
}
