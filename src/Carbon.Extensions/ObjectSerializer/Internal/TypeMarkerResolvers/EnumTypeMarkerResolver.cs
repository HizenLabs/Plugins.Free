using HizenLabs.Extensions.ObjectSerializer.Enums;
using System;

namespace HizenLabs.Extensions.ObjectSerializer.Internal.TypeMarkerResolvers;

internal class EnumTypeMarkerResolver : ITypeMarkerResolver
{
    public bool TryResolve(Type type, out TypeMarker marker)
    {
        if (type.IsEnum)
        {
            marker = TypeMarker.Enum;
            return true;
        }

        marker = default;
        return false;
    }
}
