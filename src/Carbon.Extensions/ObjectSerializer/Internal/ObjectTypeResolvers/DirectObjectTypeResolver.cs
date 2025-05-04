using HizenLabs.Extensions.ObjectSerializer.Enums;
using System;
using System.Collections.Generic;

namespace HizenLabs.Extensions.ObjectSerializer.Internal.ObjectTypeResolvers;

internal class DirectObjectTypeResolver : IObjectTypeResolver
{
    private static readonly Dictionary<Type, ObjectType> _cache = new()
    {
        { typeof(Item), ObjectType.Item },
        { typeof(BaseEntity), ObjectType.BaseEntity },
        { typeof(BasePlayer), ObjectType.BasePlayer },
    };

    public bool TryResolve(Type type, out ObjectType objType) => _cache.TryGetValue(type, out objType);
}