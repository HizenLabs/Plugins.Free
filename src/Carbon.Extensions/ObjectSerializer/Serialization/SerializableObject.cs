using Facepunch;
using HizenLabs.Extensions.ObjectSerializer.Extensions;
using System.Collections.Generic;

namespace HizenLabs.Extensions.ObjectSerializer.Serialization;

/// <summary>
/// Represents an object that can be written to and read from a stream.
/// </summary>
public class SerializableObject : Pool.IPooled
{
    public Dictionary<string, object> Properties => _properties;
    private Dictionary<string, object> _properties;

    public void EnterPool()
    {
        _properties.DisposeValues();
        Pool.FreeUnmanaged(ref _properties);
    }

    public void LeavePool()
    {
        _properties = Pool.Get<Dictionary<string, object>>();
    }
}
