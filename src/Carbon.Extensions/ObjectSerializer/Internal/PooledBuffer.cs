using Facepunch;
using System;

namespace HizenLabs.Extensions.ObjectSerializer.Internal;

/// <summary>
/// This is a wrapper around <see cref="ArrayPool{T}"/> that automatically returns itself once disposed of.
/// </summary>
internal class PooledBuffer : IDisposable
{
    /// <summary>
    /// The maximum size of the buffer. This is the maximum size that can be rented from the pool.
    /// </summary>
    public const int MaxSize = 8192;

    /// <summary>
    /// The standard length of the buffer. This is the default size that will be used if no size is specified.
    /// </summary>
    public const int StandardLength = 8192;

    /// <summary>
    /// The array pool that is used to rent and return byte arrays. This is a static property that is shared across all instances of <see cref="PooledBuffer"/>.
    /// </summary>
    public static ArrayPool<byte> Pool { get; } = new(MaxSize);

    /// <summary>
    /// The byte array that is rented from the pool. This is the actual buffer that will be used.
    /// </summary>
    public byte[] Bytes { get; }

    /// <summary>
    /// The length of the buffer that was rented from the pool.
    /// </summary>
    public int Length => Bytes.Length;

    /// <summary>
    /// Initializes a new instance of the <see cref="PooledBuffer"/> class with the specified maximum size.
    /// </summary>
    /// <param name="maxSize">The maximum size of the buffer. This must be between 1 and <see cref="MaxSize"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="maxSize"/> is less than 1 or greater than <see cref="MaxSize"/>.</exception>
    public PooledBuffer(int maxSize = StandardLength)
    {
        if (maxSize <= 0 || maxSize > MaxSize)
        {
            throw new ArgumentOutOfRangeException(nameof(maxSize), $"Max size must be between 1 and {MaxSize}.");
        }

        Bytes = Pool.Rent(maxSize);
    }

    /// <summary>
    /// Implicitly converts the <see cref="PooledBuffer"/> to a byte array. This allows the buffer to be used as a byte array without needing to explicitly call the property.
    /// </summary>
    /// <param name="buffer">The <see cref="PooledBuffer"/> to convert.</param>
    public static implicit operator byte[](PooledBuffer buffer) => buffer.Bytes;

    /// <summary>
    /// Releases the buffer back to the pool. This is called when the <see cref="PooledBuffer"/> is disposed of.
    /// </summary>
    public void Dispose()
    {
        if (Bytes != null)
        {
            Pool.Return(Bytes);
        }
    }
}
