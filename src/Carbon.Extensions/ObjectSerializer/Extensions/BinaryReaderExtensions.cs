using Facepunch;
using HizenLabs.Extensions.ObjectSerializer.Exceptions;
using HizenLabs.Extensions.ObjectSerializer.Internal;
using HizenLabs.Extensions.ObjectSerializer.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace HizenLabs.Extensions.ObjectSerializer.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="BinaryReader"/> class.
/// </summary>
public static class BinaryReaderExtensions
{
    #region System

    /// <summary>
    /// Reads an <see cref="Enum"/> value from the current stream and advances the stream position by the size of the underlying type.
    /// </summary>
    /// <param name="reader">The <see cref="BinaryReader"/> to read from.</param>
    /// <typeparam name="TEnum">The enum type to read.</typeparam>
    /// <returns>The <see cref="Enum"/> value read from the stream.</returns>
    /// <exception cref="EnumSerializationException">Thrown when the enum type is not a valid enum type for deserialization.</exception>
    public static TEnum ReadEnum<TEnum>(this BinaryReader reader)
        where TEnum : unmanaged, Enum
    {
        return EnumReader<TEnum>.Read(reader);
    }

    /// <summary>
    /// Reads a <see cref="Guid"/> value from the current stream and advances the stream position by sixteen bytes.
    /// </summary>
    /// <param name="reader">The <see cref="BinaryReader"/> to read from.</param>
    /// <returns>The <see cref="Guid"/> value read from the stream.</returns>
    /// <remarks>
    /// This method is unsafe and uses a <see cref="GuidParts"/> struct to access the individual bytes of the <see cref="Guid"/> value.
    /// </remarks>
    public static Guid ReadGuid(this BinaryReader reader)
    {
        var buffer = SerializationBuffers.GuidPool.Rent(16);
        try
        {
            reader.Read(buffer, 0, 16);
            return new(buffer);
        }
        finally
        {
            SerializationBuffers.GuidPool.Return(buffer);
        }
    }

    /// <summary>
    /// Reads a <see cref="DateTime"/> value from the current stream and advances the stream position by eight bytes.
    /// </summary>
    /// <param name="reader">The <see cref="BinaryReader"/> to read from.</param>
    /// <returns>The <see cref="DateTime"/> value read from the stream.</returns>
    public static DateTime ReadDateTime(this BinaryReader reader)
    {
        long binary = reader.ReadInt64();
        return DateTime.FromBinary(binary);
    }

    /// <summary>
    /// Reads a <see cref="TimeSpan"/> value from the current stream and advances the stream position by eight bytes.
    /// </summary>
    /// <param name="reader">The <see cref="BinaryReader"/> to read from.</param>
    /// <returns>The <see cref="TimeSpan"/> value read from the stream.</returns>
    public static TimeSpan ReadTimeSpan(this BinaryReader reader)
    {
        long ticks = reader.ReadInt64();
        return TimeSpan.FromTicks(ticks);
    }

    /// <summary>
    /// Reads a <see cref="Type"/> value from the current stream and advances the stream position by the length of the type name.
    /// </summary>
    /// <param name="reader">The <see cref="BinaryReader"/> to read from.</param>
    /// <returns>The <see cref="Type"/> value read from the stream.</returns>
    /// <exception cref="TypeLoadException">Thrown when the type cannot be loaded.</exception>
    public static Type ReadType(this BinaryReader reader)
    {
        string typeName = reader.ReadString();
        return Type.GetType(typeName) ?? throw new TypeLoadException($"Could not load type '{typeName}'.");
    }

    #endregion

    #region UnityEngine

    /// <summary>
    /// Reads a <see cref="Vector2"/> value from the current stream and advances the stream position by eight bytes.
    /// </summary>
    /// <param name="reader">The <see cref="BinaryReader"/> to read from.</param>
    /// <returns>The <see cref="Vector2"/> value read from the stream.</returns>
    public static Vector2 ReadVector2(this BinaryReader reader)
    {
        Vector2 vector;
        vector.x = reader.ReadSingle();
        vector.y = reader.ReadSingle();
        return vector;
    }

    /// <summary>
    /// Reads a <see cref="Vector3"/> value from the current stream and advances the stream position by twelve bytes.
    /// </summary>
    /// <param name="reader">The <see cref="BinaryReader"/> to read from.</param>
    /// <returns>The <see cref="Vector3"/> value read from the stream.</returns>
    public static Vector3 ReadVector3(this BinaryReader reader)
    {
        Vector3 vector;
        vector.x = reader.ReadSingle();
        vector.y = reader.ReadSingle();
        vector.z = reader.ReadSingle();
        return vector;
    }

    /// <summary>
    /// Reads a <see cref="Vector4"/> value from the current stream and advances the stream position by sixteen bytes.
    /// </summary>
    /// <param name="reader">The <see cref="BinaryReader"/> to read from.</param>
    /// <returns>The <see cref="Vector4"/> value read from the stream.</returns>
    public static Vector4 ReadVector4(this BinaryReader reader)
    {
        Vector4 vector;
        vector.x = reader.ReadSingle();
        vector.y = reader.ReadSingle();
        vector.z = reader.ReadSingle();
        vector.w = reader.ReadSingle();
        return vector;
    }

    /// <summary>
    /// Reads a <see cref="Quaternion"/> value from the current stream and advances the stream position by sixteen bytes.
    /// </summary>
    /// <param name="reader">The <see cref="BinaryReader"/> to read from.</param>
    /// <returns>The <see cref="Quaternion"/> value read from the stream.</returns>
    public static Quaternion ReadQuaternion(this BinaryReader reader)
    {
        Quaternion quaternion;
        quaternion.x = reader.ReadSingle();
        quaternion.y = reader.ReadSingle();
        quaternion.z = reader.ReadSingle();
        quaternion.w = reader.ReadSingle();
        return quaternion;
    }

    /// <summary>
    /// Reads a <see cref="Color"/> value from the current stream and advances the stream position by sixteen bytes.
    /// </summary>
    /// <param name="reader">The <see cref="BinaryReader"/> to read from.</param>
    /// <returns>The <see cref="Color"/> value read from the stream.</returns>
    public static Color ReadColor(this BinaryReader reader)
    {
        Color color;
        color.r = reader.ReadSingle();
        color.g = reader.ReadSingle();
        color.b = reader.ReadSingle();
        color.a = reader.ReadSingle();
        return color;
    }

    #endregion

    #region Collections

    /// <summary>
    /// Reads an array of type <typeparamref name="T"/> from the current stream and advances the stream position by the size of the array.
    /// </summary>
    /// <typeparam name="T">The type of the array elements.</typeparam>
    /// <param name="reader">The <see cref="BinaryReader"/> to read from.</param>
    /// <param name="buffer">The buffer to store the read values.</param>
    /// <param name="index">The offset in the buffer to start writing to.</param>
    /// <param name="count">The number of elements to read. If -1, reads the entire buffer.</param>
    public static void ReadArray<T>(this BinaryReader reader, T[] buffer, int index = 0, int count = -1)
    {
        if (count < 0) count = buffer.Length - index;
        if ((index + count) > buffer.Length) throw new ArgumentOutOfRangeException(nameof(count), "Index + count cannot be greater than the buffer length.");

        GenericArrayReader<T>.Read(reader, buffer, index, count);
    }

    /// <summary>
    /// Reads a list of type <typeparamref name="T"/> from the current stream and advances the stream position by the size of the list.
    /// </summary>
    /// <typeparam name="T">The type of the list elements.</typeparam>
    /// <param name="reader">The <see cref="BinaryReader"/> to read from.</param>
    /// <param name="list">The list to store the read values. If null, a new list will be created.</param>
    /// <returns>The list of type <typeparamref name="T"/> read from the stream.</returns>
    public static List<T> ReadList<T>(this BinaryReader reader) =>
        ReadList<T>(reader, null);

    /// <summary>
    /// Reads a list of type <typeparamref name="T"/> from the current stream and advances the stream position by the size of the list.
    /// </summary>
    /// <typeparam name="T">The type of the list elements.</typeparam>
    /// <param name="reader">The <see cref="BinaryReader"/> to read from.</param>
    /// <param name="list">The list to store the read values. If null, a new list will be created.</param>
    /// <returns>The list of type <typeparamref name="T"/> read from the stream.</returns>
    public static List<T> ReadList<T>(this BinaryReader reader, List<T> list)
    {
        list ??= Pool.Get<List<T>>();

        GenericListReader<T>.Read(reader, list);

        return list;
    }

    /// <summary>
    /// Reads a dictionary of type <typeparamref name="TKey"/> and <typeparamref name="TValue"/> from the current stream and advances the stream position by the size of the dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary keys.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary values.</typeparam>
    /// <param name="reader">The <see cref="BinaryReader"/> to read from.</param>
    /// <param name="dictionary">The dictionary to store the read values. If null, a new dictionary will be created.</param>
    /// <returns>The dictionary of type <typeparamref name="TKey"/> and <typeparamref name="TValue"/> read from the stream.</returns>
    public static Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(this BinaryReader reader) =>
        ReadDictionary<TKey, TValue>(reader, null);

    /// <summary>
    /// Reads a dictionary of type <typeparamref name="TKey"/> and <typeparamref name="TValue"/> from the current stream and advances the stream position by the size of the dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary keys.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary values.</typeparam>
    /// <param name="reader">The <see cref="BinaryReader"/> to read from.</param>
    /// <param name="dictionary">The dictionary to store the read values. If null, a new dictionary will be created.</param>
    /// <returns>The dictionary of type <typeparamref name="TKey"/> and <typeparamref name="TValue"/> read from the stream.</returns>
    public static Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(this BinaryReader reader, Dictionary<TKey, TValue> dictionary)
    {
        dictionary ??= Pool.Get<Dictionary<TKey, TValue>>();

        dictionary = GenericDictionaryReader<TKey, TValue>.Read(reader, dictionary);

        return dictionary;
    }

    /// <summary>
    /// Reads a <see cref="SerializableObject"/> from the current stream and advances the stream position by the size of the object.
    /// </summary>
    /// <param name="reader">The <see cref="BinaryReader"/> to read from.</param>
    /// <returns>The <see cref="SerializableObject"/> read from the stream.</returns>
    public static SerializableObject ReadSerializableObject(this BinaryReader reader) =>
        ReadSerializableObject(reader, null);

    /// <summary>
    /// Reads a <see cref="SerializableObject"/> from the current stream and advances the stream position by the size of the object.
    /// </summary>
    /// <param name="reader">The <see cref="BinaryReader"/> to read from.</param>
    /// <param name="obj">The <see cref="SerializableObject"/> to read into. If null, a new object will be created.</param>
    /// <returns>The <see cref="SerializableObject"/> read from the stream.</returns>
    public static SerializableObject ReadSerializableObject(this BinaryReader reader, SerializableObject obj)
    {
        obj ??= Pool.Get<SerializableObject>();
        
        obj.Read(reader);

        return obj;
    }

    #endregion
}
