using HizenLabs.Extensions.ObjectSerializer.Enums;
using System;

namespace HizenLabs.Extensions.ObjectSerializer.Exceptions;

/// <summary>
/// Exception thrown when a type cannot be resolved into an <see cref="ObjectType"/>.
/// </summary>
internal class ObjectTypeResolverException : NotSupportedException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectTypeResolverException "/> class with a specified type.
    /// </summary>
    /// <param name="type">The type that could not be resolved into an <see cref="ObjectType"/>.</param>
    public ObjectTypeResolverException(Type type) : base($"The type '{type}' could not be resolved into an {nameof(ObjectType)}.")
    {
        Type = type;
    }

    /// <summary>
    /// The type that could not be resolved into a <see cref="ObjectType"/>.
    /// </summary>
    public Type Type { get; }
}
