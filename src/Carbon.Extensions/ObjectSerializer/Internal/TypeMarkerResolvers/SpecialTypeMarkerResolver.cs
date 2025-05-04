using HizenLabs.Extensions.ObjectSerializer.Enums;
using System;

namespace HizenLabs.Extensions.ObjectSerializer.Internal.TypeMarkerResolvers;

internal class SpecialTypeMarkerResolver : ITypeMarkerResolver
{
    const string SystemRuntimeType = "System.RuntimeType";

    public bool TryResolve(Type type, out TypeMarker marker)
    {
        if (type.FullName == SystemRuntimeType)
        {
            marker = TypeMarker.Type;
            return true;
        }

        marker = default;
        return false;
    }
}
