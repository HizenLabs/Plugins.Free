using Facepunch;
using HizenLabs.Extensions.ObjectSerializer.Enums;
using HizenLabs.Extensions.ObjectSerializer.Mappers;
using System;
using System.Collections.Generic;
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
