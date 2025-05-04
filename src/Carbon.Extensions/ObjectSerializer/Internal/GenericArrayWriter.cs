using System.IO;
using System;
using System.Runtime.InteropServices;

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

    /// <summary>
    /// Initializes the <see cref="GenericArrayWriter{T}"/> class.
    /// </summary>
    /// <exception cref="NotSupportedException">Thrown for multi-dimensional arrays.</exception>
    static GenericArrayWriter()
    {
        var elementType = typeof(T);
        
        if (elementType.IsArray) throw new NotSupportedException("Multi-dimensional arrays are not supported.");
        else if (elementType == typeof(byte)) Write = (w, v, index, count) =>
        {
            if (v is not byte[] buffer) throw new ArgumentException("Buffer must be a byte array.", nameof(v));

            w.Write(buffer, index, count);
        };
        else if (elementType.IsPrimitive)
        {
            int typeSize = Marshal.SizeOf<T>();
            Write = (w, v, index, count) =>
            {
                if (v is not T[] array) throw new ArgumentException("Expected array.", nameof(v));

                int totalBytes = typeSize * count;
                w.Write(totalBytes);

                var buffer = SerializationBuffers.StreamPool.Rent(SerializationBuffers.StreamBufferSize);
                try
                {
                    int written = 0;
                    while (written < totalBytes)
                    {
                        int remaining = totalBytes - written;
                        int chunkBytes = Math.Min(SerializationBuffers.StreamBufferSize, remaining);

                        // Copy partial block
                        Buffer.BlockCopy(array, index * typeSize + written, buffer, 0, chunkBytes);

                        w.Write(buffer, 0, chunkBytes);
                        written += chunkBytes;
                    }
                }
                finally
                {
                    SerializationBuffers.StreamPool.Return(buffer);
                }
            };
        }
        else
        {
            Write = (w, v, index, count) =>
            {
                if (v is not T[] array) throw new ArgumentException("Expected array.", nameof(v));

                w.Write(count);
                for (int i = 0; i < count; i++)
                {
                    GenericWriter<T>.Write(w, array[i]);
                }
            };
        }
    }
}