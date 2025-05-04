using HizenLabs.Extensions.ObjectSerializer.Enums;
using System;

namespace HizenLabs.Extensions.ObjectSerializer.Exceptions;

/// <summary>
/// Exception thrown when a type cannot be resolved into a <see cref="TypeMarker"/>.
/// </summary>
internal class TypeMarkerResolverException : NotSupportedException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TypeMarkerResolverException"/> class with a specified type.
    /// </summary>
    /// <param name="type">The type that could not be resolved into a <see cref="TypeMarker"/>.</param>
    public TypeMarkerResolverException(Type type) : base($"The type '{type}' could not be resolved into a {nameof(TypeMarker)}.")
    {
        Type = type;
    }

    /// <summary>
    /// The type that could not be resolved into a <see cref="TypeMarker"/>.
    /// </summary>
    public Type Type { get; }
}
