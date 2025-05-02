using System;
using System.Reflection;

namespace HizenLabs.Extensions.ObjectSerializer.Internal.Delegates;

/// <summary>
/// A utility class for extracting delegates from static fields or properties.
/// </summary>
internal static class DelegateExtractor
{
    /// <summary>
    /// Extracts a delegate from a static field or property of a generic type.
    /// </summary>
    /// <param name="member">The member info of the field or property.</param>
    /// <returns>The extracted delegate.</returns>
    /// <exception cref="InvalidCastException">Thrown when the member is not a delegate.</exception>
    /// <exception cref="NotSupportedException">Thrown when the member type is not supported.</exception>
    public static Delegate Extract(MemberInfo member) =>
        member switch
        {
            FieldInfo f => f.GetValue(null) as Delegate
                ?? throw new InvalidCastException($"Field '{member.Name}' is not a delegate."),

            PropertyInfo p => SafeGetPropertyValue(p),
            _ => throw new NotSupportedException($"Unsupported member type: {member.MemberType}")
        };

    /// <summary>
    /// Safely gets the value of a property, ensuring it is a delegate.
    /// </summary>
    /// <param name="prop">The property info.</param>
    /// <returns>The delegate value of the property.</returns>
    /// <exception cref="InvalidCastException">Thrown when the property value is not a delegate.</exception>
    private static Delegate SafeGetPropertyValue(PropertyInfo prop)
    {
        try
        {
            return prop.GetValue(null) as Delegate
                ?? throw new InvalidCastException($"Property '{prop.Name}' is not a delegate.");
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            throw ex.InnerException;
        }
    }
}
