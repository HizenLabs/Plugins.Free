using HizenLabs.Extensions.ObjectSerializer.Mappers.Abstractions;
using HizenLabs.Extensions.ObjectSerializer.Serialization;
using System;
using UnityEngine;

namespace HizenLabs.Extensions.ObjectSerializer.Mappers;

public sealed class BaseEntityMapper : BaseEntityMapper<BaseEntity> { }

public class BaseEntityMapper<TOriginal> : BaseObjectMapper<TOriginal>
    where TOriginal : BaseEntity
{
    private const string _keyPrefabName = "_prefabName";
    private const string _keyServerPosition = "_serverPosition";
    private const string _keyServerRotation = "_serverRotation";

    public BaseEntityMapper()
    {
    }

    protected override TOriginal CreateInstance(SerializableObject source)
    {
        var prefabName = source.Properties[_keyPrefabName] as string ?? throw new Exception($"Failed to find '{_keyPrefabName}'");
        var position = source.Properties[_keyServerPosition] as Vector3? ?? throw new Exception($"Failed to find '{_keyServerPosition}'");
        var rotation = source.Properties[_keyServerRotation] as Quaternion? ?? throw new Exception($"Failed to find '{_keyServerRotation}'");

        return (TOriginal)GameManager.server.CreateEntity(prefabName, position, rotation);
    }

    protected override void OnSerializeSelf(TOriginal source, SerializableObject target)
    {
        target.Properties[_keyPrefabName] = source.PrefabName;
        target.Properties[_keyServerPosition] = source.ServerPosition;
        target.Properties[_keyServerRotation] = source.ServerRotation;

        base.OnSerializeSelf(source, target);
    }
}
