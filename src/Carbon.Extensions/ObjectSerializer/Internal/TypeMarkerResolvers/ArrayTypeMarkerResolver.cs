using HizenLabs.Extensions.ObjectSerializer.Enums;
using System;

namespace HizenLabs.Extensions.ObjectSerializer.Internal.TypeMarkerResolvers;

internal class ArrayTypeMarkerResolver : ITypeMarkerResolver
{
    public bool TryResolve(Type type, out TypeMarker marker)
    {
        if (type.IsArray)
        {
            marker = TypeMarker.Array;
            return true;
        }

        marker = default;
        return false;
    }
}
