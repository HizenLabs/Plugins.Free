using HizenLabs.Extensions.ObjectSerializer.Enums;
using HizenLabs.Extensions.ObjectSerializer.Internal.TypeMarkerResolvers;
using System;

namespace HizenLabs.Extensions.ObjectSerializer.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Type"/> class.
/// </summary>
public static class TypeExtensions
{
    public static TypeMarker GetTypeMarker(this Type type)
    {
        return TypeMarkerResolver.Resolve(type);
    }
}