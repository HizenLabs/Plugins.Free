using HizenLabs.Extensions.ObjectSerializer.Exceptions;
using HizenLabs.Extensions.ObjectSerializer.Internal;
using HizenLabs.Extensions.ObjectSerializer.Serialization;
using HizenLabs.Extensions.ObjectSerializer.Structs;
using System;
using System.Collections.Generic;
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
    /// Writes an <see cref="Enum"/> value to the current stream and advances the stream position by the size of the underlying type.
    /// </summary>
    /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
    /// <param name="value">The <see cref="Enum"/> value to write.</param>
    /// <typeparam name="TEnum">The type of the enum.</typeparam>
    /// <exception cref="EnumSerializationException">Thrown when the enum type is not a valid enum type for serialization.</exception>
    public static void WriteEnum<TEnum>(this BinaryWriter writer, TEnum value)
        where TEnum : unmanaged, Enum
    {
        EnumWriter<TEnum>.Write(writer, value);
    }

    /// <summary>
    /// Writes a <see cref="Guid"/> value to the current stream and advances the stream position by sixteen bytes.
    /// </summary>
    /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
    /// <param name="guid">The <see cref="Guid"/> value to write.</param>
    /// <remarks>
    /// This method is unsafe and uses a <see cref="GuidParts"/> struct to access the individual bytes of the <see cref="Guid"/> value.
    /// This works around the need to allocate 16 bytes of memory for the <see cref="Guid.ToByteArray"/> method.
    /// </remarks>
    public static void Write(this BinaryWriter writer, Guid guid)
    {
        var buffer = SerializationBuffers.Guid.Rent(16);
        try
        {
            unsafe
            {
                fixed (byte* ptr = buffer)
                {
                    *(Guid*)ptr = guid;
                }
            }

            writer.Write(buffer, 0, 16);
        }
        finally
        {
            SerializationBuffers.Guid.Return(buffer);
        }
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

    /// <summary>
    /// Writes a <see cref="Quaternion"/> value to the current stream and advances the stream position by sixteen bytes.
    /// </summary>
    /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
    /// <param name="quaternion">The <see cref="Quaternion"/> value to write.</param>
    public static void Write(this BinaryWriter writer, Quaternion quaternion)
    {
        writer.Write(quaternion.x);
        writer.Write(quaternion.y);
        writer.Write(quaternion.z);
        writer.Write(quaternion.w);
    }

    /// <summary>
    /// Writes a <see cref="Color"/> value to the current stream and advances the stream position by sixteen bytes.
    /// </summary>
    /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
    /// <param name="color">The <see cref="Color"/> value to write.</param>
    public static void Write(this BinaryWriter writer, Color color)
    {
        writer.Write(color.r);
        writer.Write(color.g);
        writer.Write(color.b);
        writer.Write(color.a);
    }

    #endregion

    #region Collections

    /// <summary>
    /// Writes an array of <see cref="byte"/> values to the current stream and advances 
    /// the stream position by the length of the <paramref name="count"/> or the array length.
    /// </summary>
    /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
    /// <param name="array">The array of <see cref="byte"/> values to write.</param>
    /// <param name="index">The offset in the array to start writing from.</param>
    /// <param name="count">The number of elements to write from the array.</param>
    /// <typeparam name="T">The type of the array elements.</typeparam>
    public static void WriteArray<T>(this BinaryWriter writer, T[] array, int index = 0, int count = -1)
    {
        if (count < 0) count = array.Length;
        if (count < array.Length) throw new ArgumentOutOfRangeException(nameof(count), "Count cannot be less than the array length.");
        if (index < 0 || index >= array.Length) throw new ArgumentOutOfRangeException(nameof(index), "Index cannot be less than 0 or greater than the array length.");
        if (count + index > array.Length) throw new ArgumentOutOfRangeException(nameof(count), "Count and index cannot exceed the array length.");

        GenericArrayWriter<T>.Write(writer, array, index, count);
    }

    /// <summary>
    /// Writes a list of <typeparamref name="T"/> values to the current stream and advances the stream position by the length of the list.
    /// </summary>
    /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
    /// <param name="list">The list of <typeparamref name="T"/> values to write.</param>
    /// <typeparam name="T">The type of the list elements.</typeparam>
    public static void WriteList<T>(this BinaryWriter writer, List<T> list)
    {
        GenericListWriter<T>.Write(writer, list);
    }

    /// <summary>
    /// Writes a dictionary of <typeparamref name="TKey"/> and <typeparamref name="TValue"/> values to the current stream and advances the stream position by the length of the dictionary.
    /// </summary>
    /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
    /// <param name="dict">The dictionary of <typeparamref name="TKey"/> and <typeparamref name="TValue"/> values to write.</param>
    /// <typeparam name="TKey">The type of the dictionary keys.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary values.</typeparam>
    public static void WriteDictionary<TKey, TValue>(this BinaryWriter writer, Dictionary<TKey, TValue> dict)
    {
        GenericDictionaryWriter<TKey, TValue>.Write(writer, dict);
    }

    #endregion

    #region Custom Types

    /// <summary>
    /// Writes a <see cref="SerializableObject"/> to the current stream and advances the stream position by the length of the object.
    /// </summary>
    /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
    /// <param name="obj">The <see cref="SerializableObject"/> to write.</param>
    public static void Write(this BinaryWriter writer, SerializableObject obj)
    {
        obj.Write(writer);
    }

    #endregion
}
