using HizenLabs.Extensions.ObjectSerializer.Enums;
using HizenLabs.Extensions.ObjectSerializer.Exceptions;
using System;
using System.Linq;

namespace HizenLabs.Extensions.ObjectSerializer.Internal.TypeMarkerResolvers;

/// <summary>
/// Class to resolve the type markers.
/// </summary>
internal static class TypeMarkerResolver
{
    /// <summary>
    /// Array of resolvers to resolve the type markers.
    /// </summary>
    private static readonly ITypeMarkerResolver[] _resolvers = new ITypeMarkerResolver[]
    {
        new NullTypeMarkerResolver(),
        new DirectTypeMarkerResolver(),
        new GenericTypeMarkerResolver(),
        new EnumTypeMarkerResolver(),
        new ArrayTypeMarkerResolver(),
        new SpecialTypeMarkerResolver()
    };

    /// <summary>
    /// Resolves the type marker for the given type.
    /// </summary>
    /// <param name="type">The type to resolve.</param>
    /// <returns>The resolved type marker.</returns>
    public static TypeMarker Resolve(Type type)
    {
        TypeMarker marker = default;
        var resolver = _resolvers.FirstOrDefault(r => r.TryResolve(type, out marker));
        return resolver != null ? marker : throw new TypeMarkerResolverException(type);
    }
}
