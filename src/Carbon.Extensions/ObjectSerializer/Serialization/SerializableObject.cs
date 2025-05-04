using Facepunch;
using HizenLabs.Extensions.ObjectSerializer.Enums;
using HizenLabs.Extensions.ObjectSerializer.Extensions;
using System.Collections.Generic;
using System.IO;

namespace HizenLabs.Extensions.ObjectSerializer.Serialization;

/// <summary>
/// Represents an object that can be written to and read from a stream.
/// </summary>
public class SerializableObject : Pool.IPooled
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SerializableObject"/> class.
    /// </summary>
    public ObjectType Type { get; private set; }

    /// <summary>
    /// The properties of the object.
    /// </summary>
    public Dictionary<string, object> Properties => _properties;
    private Dictionary<string, object> _properties;

    /// <summary>
    /// Reads from the <see cref="BinaryReader"/> stream into the <see cref="SerializableObject"/>.
    /// </summary>
    /// <param name="reader">The <see cref="BinaryReader"/> to read from.</param>
    public void Read(BinaryReader reader)
    {
        Type = reader.ReadEnum<ObjectType>();
        reader.ReadDictionary(_properties);
    }

    /// <summary>
    /// Writes the <see cref="SerializableObject"/> to the <see cref="BinaryWriter"/> stream.
    /// </summary>
    /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
    public void Write(BinaryWriter writer)
    {
        writer.WriteEnum(Type);
        writer.WriteDictionary(_properties);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SerializableObject"/> class.
    /// </summary>
    public void EnterPool()
    {
        Type = default;

        _properties.DisposeValues();
        Pool.FreeUnmanaged(ref _properties);
    }

    /// <summary>
    /// Leaves the pool and resets the object for reuse.
    /// </summary>
    public void LeavePool()
    {
        _properties = Pool.Get<Dictionary<string, object>>();
    }
}
