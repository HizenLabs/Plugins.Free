using HizenLabs.Extensions.ObjectSerializer.Enums;
using System;
using System.Collections.Generic;

namespace HizenLabs.Extensions.ObjectSerializer.Internal.TypeMarkerResolvers;

internal class GenericTypeMarkerResolver : ITypeMarkerResolver
{
    private static readonly Dictionary<Type, TypeMarker> _cache = new()
    {
        { typeof(List<>), TypeMarker.List },
        { typeof(Dictionary<,>), TypeMarker.Dictionary }
    };

    public bool TryResolve(Type type, out TypeMarker marker)
    {
        if (type.IsGenericType)
        {
            return _cache.TryGetValue(type.GetGenericTypeDefinition(), out marker);
        }

        marker = default;
        return false;
    }
}
