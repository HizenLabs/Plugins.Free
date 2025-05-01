using System;

namespace HizenLabs.Extensions.ObjectSerializer.Exceptions;

/// <summary>
/// Exception thrown when a <see cref="Type"> cannot be serialized.
/// </summary>
public class TypeSerializationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TypeSerializationException"/> class.
    /// </summary>
    /// <param name="type">The type that failed to serialize.</param>
    public TypeSerializationException(Type type)
        : base(CreateMessage(type))
    {
        ProblemType = type;
    }

    /// <summary>
    /// Gets the type that could not be serialized.
    /// </summary>
    public Type ProblemType { get; }

    private static string CreateMessage(Type type)
    {
        if (type is null)
            return "Cannot serialize null type.";

        if (type.IsGenericParameter)
            return $"Cannot serialize generic type parameter '{type.Name}'.";

        return $"Cannot serialize type '{type.FullName}' because it does not have an assembly qualified name.";
    }
}
