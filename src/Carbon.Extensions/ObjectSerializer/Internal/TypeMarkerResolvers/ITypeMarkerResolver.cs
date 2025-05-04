using HizenLabs.Extensions.ObjectSerializer.Enums;
using System;

namespace HizenLabs.Extensions.ObjectSerializer.Internal.TypeMarkerResolvers;

internal interface ITypeMarkerResolver
{
    bool TryResolve(Type type, out TypeMarker typeMarker);
}
