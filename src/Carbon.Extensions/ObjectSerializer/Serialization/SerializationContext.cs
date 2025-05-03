using Facepunch;
using System.Collections.Generic;

namespace HizenLabs.Extensions.ObjectSerializer.Serialization;

/// <summary>
/// Represents the context for serialization and deserialization.
/// </summary>
public class SerializationContext : Pool.IPooled
{
    public List<SerializableObject> Objects => _objects;
    private List<SerializableObject> _objects;

    public void EnterPool()
    {
        Pool.Free(ref _objects, true);
    }

    public void LeavePool()
    {
        _objects = Pool.Get<List<SerializableObject>>();
    }
}
