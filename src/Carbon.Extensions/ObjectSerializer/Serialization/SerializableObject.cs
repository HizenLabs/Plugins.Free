using Facepunch;
using HizenLabs.Extensions.ObjectSerializer.Enums;
using HizenLabs.Extensions.ObjectSerializer.Extensions;
using System;
using System.Collections.Generic;
using System.IO;

namespace HizenLabs.Extensions.ObjectSerializer.Serialization;

/// <summary>
/// Represents an object that can be written to and read from a stream.
/// </summary>
public class SerializableObject : Pool.IPooled
{
    /// <summary>
    /// The index for this object, relative to the context. Mainly used for connections like parent, entitylinks, etc.
    /// </summary>
    public int Index { get; private set; }

    /// <summary>
    /// The type of the object.
    /// </summary>
    public ObjectType Type { get; private set; }

    /// <summary>
    /// The actual game object reference.
    /// </summary>
    public object GameObject { get; private set; }

    /// <summary>
    /// The properties of the object.
    /// </summary>
    public Dictionary<string, object> Properties => _properties;
    private Dictionary<string, object> _properties;

    /// <summary>
    /// Initializes the object for serialization.
    /// </summary>
    /// <param name="index">The index of the object.</param>
    public void Init(object source, int index = -1)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source), "Source object cannot be null.");
        }

        GameObject = source;
        Type = source.GetType().GetObjectType();

        if (index > -1)
        {
            Index = index;
        }
    }

    /// <summary>
    /// Reads from the <see cref="BinaryReader"/> stream into the <see cref="SerializableObject"/>.
    /// </summary>
    /// <param name="reader">The <see cref="BinaryReader"/> to read from.</param>
    public void Read(BinaryReader reader)
    {
        Index = reader.ReadInt32();
        Type = reader.ReadEnum<ObjectType>();
        reader.ReadDictionary(_properties);
    }

    /// <summary>
    /// Writes the <see cref="SerializableObject"/> to the <see cref="BinaryWriter"/> stream.
    /// </summary>
    /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
    public void Write(BinaryWriter writer)
    {
        writer.Write(Index);
        writer.WriteEnum(Type);
        writer.WriteDictionary(_properties);
    }

    /// <summary>
    /// Resets the object when it returns to the pool.
    /// </summary>
    public void EnterPool()
    {
        Index = -1;
        Type = default;

        _properties.DisposeValues();
        Pool.FreeUnmanaged(ref _properties);
    }

    /// <summary>
    /// Prepares the object for use when leaving the pool.
    /// </summary>
    public void LeavePool()
    {
        Index = -1;
        _properties = Pool.Get<Dictionary<string, object>>();
    }
}