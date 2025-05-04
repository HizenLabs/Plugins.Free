using HizenLabs.Extensions.ObjectSerializer.Enums;
using HizenLabs.Extensions.ObjectSerializer.Internal.TypeMarkerResolvers;
using System;

namespace HizenLabs.Extensions.ObjectSerializer.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Type"/> class.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Gets the <see cref="TypeMarker"/> for the given type.
    /// </summary>
    /// <param name="type">The type to get the marker for.</param>
    /// <returns>The <see cref="TypeMarker"/> for the given type.</returns>
    public static TypeMarker GetTypeMarker(this Type type)
    {
        return TypeMarkerResolver.Resolve(type);
    }

    /// <summary>
    /// Determines whether the specified type can be null.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><c>true</c> if the type can be null; otherwise, <c>false</c>.</returns>
    public static bool CanBeNull(this Type type)
    {
        if (!type.IsValueType)
            return true;

        if (Nullable.GetUnderlyingType(type) != null)
            return true;

        return false;
    }
}