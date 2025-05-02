using System;
using System.Reflection;

namespace HizenLabs.Extensions.ObjectSerializer.Internal.Delegates;

/// <summary>
/// A utility class for resolving members of generic type definitions.
/// </summary>
internal static class GenericTypeResolver
{
    /// <summary>
    /// Gets a member of a generic type definition.
    /// </summary>
    /// <param name="genericTypeDef">The generic type definition.</param>
    /// <param name="name">The name of the member.</param>
    /// <param name="kind">The type of member (field, property, etc.).</param>
    /// <param name="args">The type arguments to use for the generic type.</param>
    /// <returns>The member info for the specified member.</returns>
    /// <exception cref="MissingMemberException">Thrown when the member is not found.</exception>
    /// <exception cref="NotSupportedException">Thrown when the member type is not supported.</exception>
    public static MemberInfo GetMember(Type genericTypeDef, string name, MemberTypes kind, Type[] args)
    {
        var type = genericTypeDef.MakeGenericType(args);
        return kind switch
        {
            MemberTypes.Field => type.GetField(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                ?? throw new MissingMemberException(type.FullName, name),

            MemberTypes.Property => type.GetProperty(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                ?? throw new MissingMemberException(type.FullName, name),

            _ => throw new NotSupportedException($"Unsupported member type: {kind}")
        };
    }

    /// <summary>
    /// Gets a method of a generic type definition.
    /// </summary>
    /// <param name="genericTypeDef">The generic type definition.</param>
    /// <param name="name">The name of the method.</param>
    /// <param name="args">The type arguments to use for the generic type.</param>
    /// <returns>The method info for the specified method.</returns>
    /// <exception cref="MissingMethodException">Thrown when the method is not found.</exception>
    public static MethodInfo GetMethod(Type genericTypeDef, string name, Type[] args)
    {
        var type = genericTypeDef.MakeGenericType(args);
        return type.GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new MissingMethodException(type.FullName, name);
    }
}
