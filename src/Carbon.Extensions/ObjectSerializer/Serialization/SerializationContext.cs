using Facepunch;
using HizenLabs.Extensions.ObjectSerializer.Enums;
using HizenLabs.Extensions.ObjectSerializer.Extensions;
using HizenLabs.Extensions.ObjectSerializer.Mappers;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace HizenLabs.Extensions.ObjectSerializer.Serialization;

/// <summary>
/// Represents the context for serialization and deserialization.
/// </summary>
public class SerializationContext : Pool.IPooled, IDisposable
{
    /// <summary>
    /// The list of serializable objects.
    /// </summary>
    public IReadOnlyList<SerializableObject> Objects => _objects;
    private List<SerializableObject> _objects;

    /// <summary>
    /// The index of the next object to be added.
    /// </summary>
    public int NextObjectIndex { get; private set; }

    /// <summary>
    /// Creates a new <see cref="SerializableObject"/> and writes the <paramref name="source"/> to it before being added to the context.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="source">The object to serialize.</param>
    public void AddObject<T>(T source)
    {
        var obj = Pool.Get<SerializableObject>();
        obj.Init(source, NextObjectIndex++);

        var mapper = ObjectMapperFactory.GetMapper<T>();
        mapper.SerializeSelf(source, obj);

        _objects.Add(obj);
    }

    /// <summary>
    /// Gets all objects of a specific type from the context.
    /// </summary>
    /// <param name="type">The type of objects to get.</param>
    /// <returns>An enumerable of objects of the specified type.</returns>
    public IEnumerable<SerializableObject> Find(params ObjectType[] types)
    {
        return Objects.Where(obj => types.Any(t => obj.Type == t));
    }

    /// <summary>
    /// Tries to find an object by its index.
    /// </summary>
    /// <param name="index">The index of the object to find.</param>
    /// <param name="obj">The found object, if any.</param>
    /// <returns>True if the object was found; otherwise, false.</returns>
    public bool TryFindByIndex(int index, out SerializableObject obj)
    {
        if (index < 0 || index >= NextObjectIndex)
        {
            obj = null;
            return false;
        }

        obj = _objects.FirstOrDefault(o => o.Index == index);
        return obj != null;
    }

    /// <summary>
    /// Loads the context from a file, optionally using GZip compression.
    /// </summary>
    /// <param name="filename">The name of the file to load from.</param>
    /// <param name="dataFormat">The format of the data in the file.</param>
    public void Load(string filename, DataFormat dataFormat = DataFormat.Binary)
    {
        using var stream = File.OpenRead(filename);
        Load(stream, dataFormat);
    }

    /// <summary>
    /// Loads the context from a stream.
    /// </summary>
    /// <param name="stream">The stream to load from.</param>
    /// <param name="dataFormat">The format of the data in the stream.</param>
    /// <exception cref="NotImplementedException">Thrown when the specified format is not implemented.</exception>
    public void Load(Stream stream, DataFormat dataFormat = DataFormat.Binary)
    {
        switch (dataFormat)
        {
            case DataFormat.Binary:
                ReadBinary(stream);
                break;

            case DataFormat.Gzip:
                ReadGzip(stream);
                break;

            default:
                throw new NotImplementedException($"Format {dataFormat} is not implemented yet.");
        }
    }

    /// <summary>
    /// Loads the context from a compressed stream using GZip compression.
    /// </summary>
    /// <param name="stream">The compressed stream to load from.</param>
    internal void ReadGzip(Stream stream)
    {
        using var gzipStream = new GZipStream(stream, CompressionMode.Decompress);
        ReadBinary(gzipStream);
    }

    /// <summary>
    /// Reads the context from a stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    internal void ReadBinary(Stream stream)
    {
        using var reader = new BinaryReader(stream);

        _objects = reader.ReadList<SerializableObject>();

        NextObjectIndex = _objects.Count > 0 ? _objects.Max(o => o.Index) + 1 : 0;

        foreach (var obj in _objects)
        {
            var mapper = ObjectMapperFactory.GetMapperByObjectType(obj.Type);
            var instance = mapper.DeserializeSelf(obj);
            obj.Init(instance);
        }

        foreach (var obj in _objects)
        {
            var mapper = ObjectMapperFactory.GetMapperByObjectType(obj.Type);
            mapper.DeserializeComplete(obj, obj.GameObject, this);
        }
    }

    /// <summary>
    /// Saves the context to a file, optionally using GZip compression.
    /// </summary>
    /// <param name="fileName">The name of the file to save to.</param>
    /// <param name="dataFormat">The format of the data in the file.</param>
    public void Save(string fileName, DataFormat dataFormat = DataFormat.Binary)
    {
        using var stream = File.Create(fileName);
        Save(stream);
    }

    /// <summary>
    /// Saves the context to a stream.
    /// </summary>
    /// <param name="stream">The stream to save to.</param>
    /// <param name="dataFormat">The format of the data in the stream.</param>
    /// <exception cref="NotImplementedException">Thrown when the specified format is not implemented.</exception>
    public void Save(Stream stream, DataFormat dataFormat = DataFormat.Binary)
    {
        switch (dataFormat)
        {
            case DataFormat.Binary:
                WriteBinary(stream);
                break;

            case DataFormat.Gzip:
                WriteGzip(stream);
                break;

            default:
                throw new NotImplementedException($"Format {dataFormat} is not implemented yet.");
        }
    }

    /// <summary>
    /// Saves the context to a compressed stream using GZip compression.
    /// </summary>
    /// <param name="stream">The compressed stream to save to.</param>
    internal void WriteGzip(Stream stream)
    {
        using var gzipStream = new GZipStream(stream, CompressionMode.Compress);
        WriteBinary(gzipStream);
    }

    /// <summary>
    /// Writes the context to a stream.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    internal void WriteBinary(Stream stream)
    {
        foreach (var obj in _objects)
        {
            var mapper = ObjectMapperFactory.GetMapperByObjectType(obj.Type);
            mapper.SerializeComplete(obj.GameObject, obj, this);
        }

        using var writer = new BinaryWriter(stream);
        writer.WriteList(_objects);
    }

    /// <summary>
    /// Returns to the pool and clears the context and its members.
    /// </summary>
    public void EnterPool()
    {
        NextObjectIndex = 0;

        Pool.Free(ref _objects, true);
    }

    /// <summary>
    /// Leaves the pool and prepares the context.
    /// </summary>
    public void LeavePool()
    {
        NextObjectIndex = 0;

        _objects = Pool.Get<List<SerializableObject>>();
    }

    /// <summary>
    /// Releases self back into the pool.
    /// </summary>
    public void Dispose()
    {
        var obj = this;
        Pool.Free(ref obj);
    }
}
