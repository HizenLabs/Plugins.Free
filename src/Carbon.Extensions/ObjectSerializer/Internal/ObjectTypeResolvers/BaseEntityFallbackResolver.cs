using HizenLabs.Extensions.ObjectSerializer.Enums;
using System;

namespace HizenLabs.Extensions.ObjectSerializer.Internal.ObjectTypeResolvers;

/// <summary>
/// If all other resolvers fail, this resolver tests for BaseEntity implementation.
/// </summary>
internal class BaseEntityFallbackResolver : IObjectTypeResolver
{
    /// <summary>
    /// Checks if the type is a subclass of BaseEntity and sets the object type accordingly.
    /// </summary>
    /// <param name="type">The type to resolve.</param>
    /// <param name="objectType">The resolved object type.</param>
    /// <returns>True if the type was resolved; otherwise, false.</returns>
    public bool TryResolve(Type type, out ObjectType objectType)
    {
        if (type.IsSubclassOf(typeof(BaseEntity)))
        {
            objectType = ObjectType.BaseEntity;
            return true;
        }
        objectType = default;
        return false;
    }
}
