using System;

namespace HizenLabs.Extensions.ObjectSerializer.Exceptions;

/// <summary>
/// Exception thrown when an enum type cannot be serialized.
/// </summary>
public class EnumSerializationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EnumSerializationException"/> class.
    /// </summary>
    /// <param name="enumType">The enum type that failed to serialize.</param>
    public EnumSerializationException(Type enumType)
        : base($"The type {enumType} is not a valid enum type for serialization.")
    {
        EnumType = enumType;
    }

    /// <summary>
    /// Gets the enum type that could not be serialized.
    /// </summary>
    public Type EnumType { get; }
}
