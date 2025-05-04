using HizenLabs.Extensions.ObjectSerializer.Enums;
using System;

namespace HizenLabs.Extensions.ObjectSerializer.Internal.TypeMarkerResolvers;

internal class NullTypeMarkerResolver : ITypeMarkerResolver
{
    public bool TryResolve(Type type, out TypeMarker typeMarker)
    {
        typeMarker = TypeMarker.Null;
        return type is null;
    }
}
