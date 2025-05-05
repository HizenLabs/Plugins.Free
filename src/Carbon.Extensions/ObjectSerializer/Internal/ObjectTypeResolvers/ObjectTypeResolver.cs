using HizenLabs.Extensions.ObjectSerializer.Enums;
using HizenLabs.Extensions.ObjectSerializer.Exceptions;
using System;
using System.Linq;

namespace HizenLabs.Extensions.ObjectSerializer.Internal.ObjectTypeResolvers;

/// <summary>
/// Class to resolve the object types.
/// </summary>
internal static class ObjectTypeResolver
{
    /// <summary>
    /// Array of resolvers to resolve the object types.
    /// </summary>
    private static readonly IObjectTypeResolver[] _resolvers = new IObjectTypeResolver[]
    {
        new DirectObjectTypeResolver(),
        new BaseEntityFallbackResolver()
    };

    /// <summary>
    /// Resolves the object type for the given type.
    /// </summary>
    /// <param name="type">The type to resolve.</param>
    /// <returns>The resolved object type.</returns>
    /// <exception cref="ObjectTypeResolverException">Thrown when the type cannot be resolved into an <see cref="ObjectType"/>.</exception>
    public static ObjectType Resolve(Type type)
    {
        ObjectType objectType = default;
        var resolver = _resolvers.FirstOrDefault(r => r.TryResolve(type, out objectType));
        return resolver != null ? objectType : throw new ObjectTypeResolverException(type);
    }
}
