using HizenLabs.Extensions.ObjectSerializer.Exceptions;
using HizenLabs.Extensions.ObjectSerializer.Structs;
using System;
using System.IO;
using UnityEngine;

namespace HizenLabs.Extensions.ObjectSerializer.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="BinaryWriter"/> class.
/// </summary>
public static class BinaryWriterExtensions
{
    #region System

    /// <summary>
    /// Writes a <see cref="Guid"/> value to the current stream and advances the stream position by sixteen bytes.
    /// </summary>
    /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
    /// <param name="guid">The <see cref="Guid"/> value to write.</param>
    /// <remarks>
    /// This method is unsafe and uses a <see cref="GuidParts"/> struct to access the individual bytes of the <see cref="Guid"/> value.
    /// This works around the need to allocate 16 bytes of memory for the <see cref="Guid.ToByteArray"/> method.
    /// </remarks>
    public static unsafe void Write(this BinaryWriter writer, Guid guid)
    {
        System.Diagnostics.Debug.Assert(sizeof(Guid) == sizeof(GuidParts), $"{nameof(Guid)} and {nameof(GuidParts)} size mismatch!");

        GuidParts parts = *(GuidParts*)&guid;
        writer.Write((byte)parts._a);
        writer.Write((byte)(parts._a >> 8));
        writer.Write((byte)(parts._a >> 16));
        writer.Write((byte)(parts._a >> 24));
        writer.Write((byte)parts._b);
        writer.Write((byte)(parts._b >> 8));
        writer.Write((byte)parts._c);
        writer.Write((byte)(parts._c >> 8));
        writer.Write(parts._d);
        writer.Write(parts._e);
        writer.Write(parts._f);
        writer.Write(parts._g);
        writer.Write(parts._h);
        writer.Write(parts._i);
        writer.Write(parts._j);
        writer.Write(parts._k);
    }

    /// <summary>
    /// Writes a <see cref="DateTime"/> value to the current stream and advances the stream position by eight bytes.
    /// </summary>
    /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
    /// <param name="dateTime">The <see cref="DateTime"/> value to write.</param>
    public static void Write(this BinaryWriter writer, DateTime dateTime)
    {
        long binary = dateTime.ToBinary();
        writer.Write(binary);
    }

    /// <summary>
    /// Writes a <see cref="TimeSpan"/> value to the current stream and advances the stream position by eight bytes.
    /// </summary>
    /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
    /// <param name="timeSpan">The <see cref="TimeSpan"/> value to write.</param>
    public static void Write(this BinaryWriter writer, TimeSpan timeSpan)
    {
        long ticks = timeSpan.Ticks;
        writer.Write(ticks);
    }

    /// <summary>
    /// Writes a <see cref="Type"/> value to the current stream and advances the stream position by the length of the type name.
    /// </summary>
    /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
    /// <param name="type">The <see cref="Type"/> value to write.</param>
    /// <exception cref="TypeSerializationException">Thrown when the type cannot be serialized.</exception>
    public static void Write(this BinaryWriter writer, Type type)
    {
        if (type?.AssemblyQualifiedName is not string typeName)
            throw new TypeSerializationException(type);

        writer.Write(typeName);
    }

    #endregion

    #region UnityEngine

    /// <summary>
    /// Writes a <see cref="Vector2"/> value to the current stream and advances the stream position by eight bytes.
    /// </summary>
    /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
    /// <param name="vector">The <see cref="Vector2"/> value to write.</param>
    public static void Write(this BinaryWriter writer, Vector2 vector)
    {
        writer.Write(vector.x);
        writer.Write(vector.y);
    }

    /// <summary>
    /// Writes a <see cref="Vector3"/> value to the current stream and advances the stream position by twelve bytes.
    /// </summary>
    /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
    /// <param name="vector">The <see cref="Vector3"/> value to write.</param>
    public static void Write(this BinaryWriter writer, Vector3 vector)
    {
        writer.Write(vector.x);
        writer.Write(vector.y);
        writer.Write(vector.z);
    }

    /// <summary>
    /// Writes a <see cref="Vector4"/> value to the current stream and advances the stream position by sixteen bytes.
    /// </summary>
    /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
    /// <param name="vector">The <see cref="Vector4"/> value to write.</param>
    public static void Write(this BinaryWriter writer, Vector4 vector)
    {
        writer.Write(vector.x);
        writer.Write(vector.y);
        writer.Write(vector.z);
        writer.Write(vector.w);
    }

    #endregion
}
