using System;
using System.Linq;
using System.Reflection;

namespace HizenLabs.Extensions.ObjectSerializer.Internal.Delegates;

/// <summary>
/// A factory class for creating delegates from static fields, properties, or methods of generic types.
/// </summary>
internal static class GenericDelegateFactory
{
    #region Generic Classes

    /// <summary>
    /// Creates a delegate from a static field of a generic type definition.
    /// </summary>
    /// <typeparam name="TDelegate">The type of the delegate to create.</typeparam>
    /// <param name="genericTypeDef">The generic type definition.</param>
    /// <param name="name">The name of the field.</param>
    /// <param name="typeArgs">The type arguments to use for the generic type.</param>
    /// <returns>The created delegate.</returns>
    public static TDelegate BuildField<TDelegate>(Type genericTypeDef, string name, params Type[] typeArgs)
        where TDelegate : Delegate =>
        BuildFrom<TDelegate>(genericTypeDef, name, typeArgs, MemberTypes.Field);

    /// <summary>
    /// Creates a delegate from a static property of a generic type definition.
    /// </summary>
    /// <typeparam name="TDelegate">The type of the delegate to create.</typeparam>
    /// <param name="genericTypeDef">The generic type definition.</param>
    /// <param name="name">The name of the property.</param>
    /// <param name="typeArgs">The type arguments to use for the generic type.</param>
    /// <returns>The created delegate.</returns>
    public static TDelegate BuildProperty<TDelegate>(Type genericTypeDef, string name, params Type[] typeArgs)
        where TDelegate : Delegate =>
        BuildFrom<TDelegate>(genericTypeDef, name, typeArgs, MemberTypes.Property);

    /// <summary>
    /// Creates a delegate from a static method of a generic type definition.
    /// </summary>
    /// <typeparam name="TDelegate">The type of the delegate to create.</typeparam>
    /// <param name="genericTypeDef">The generic type definition.</param>
    /// <param name="name">The name of the method.</param>
    /// <param name="typeArgs">The type arguments to use for the generic type.</param>
    /// <returns>The created delegate.</returns>
    public static TDelegate BuildMethod<TDelegate>(Type genericTypeDef, string name, params Type[] typeArgs)
        where TDelegate : Delegate
    {
        var method = GenericTypeResolver.GetMethod(genericTypeDef, name, typeArgs);
        return (TDelegate)method.CreateDelegate(typeof(TDelegate));
    }

    /// <summary>
    /// Creates a delegate from a static member of a generic type definition.
    /// </summary>
    /// <typeparam name="TDelegate">The type of the delegate to create.</typeparam>
    /// <param name="genericTypeDef">The generic type definition.</param>
    /// <param name="name">The name of the member.</param>
    /// <param name="typeArgs">The type arguments to use for the generic type.</param>
    /// <param name="kind">The type of member (field, property, etc.).</param>
    /// <returns>The created delegate.</returns>
    private static TDelegate BuildFrom<TDelegate>(
        Type genericTypeDef,
        string name,
        Type[] typeArgs,
        MemberTypes kind)
        where TDelegate : Delegate
    {
        var member = GenericTypeResolver.GetMember(genericTypeDef, name, kind, typeArgs);
        var staticDelegate = DelegateExtractor.Extract(member);
        return DelegateAdapter.Adapt<TDelegate>(staticDelegate);
    }

    #endregion

    #region Generic Members

    /// <summary>
    /// Creates a delegate from a static field of a generic type.
    /// </summary>
    /// <typeparam name="TDelegate">The type of the delegate to create.</typeparam>
    /// <param name="staticType">The static type containing the field.</param>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="fieldTypeArgs">The type arguments to use for the field.</param>
    /// <returns>The created delegate.</returns>
    public static TDelegate BuildGenericMethod<TDelegate>(Type staticType, string methodName, params Type[] methodTypeArgs)
    where TDelegate : Delegate
    {
        var method = staticType
            .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .FirstOrDefault(m => m.Name == methodName && m.IsGenericMethodDefinition)
            ?? throw new MissingMethodException(staticType.FullName, methodName);
        var closedMethod = method.MakeGenericMethod(methodTypeArgs);

        return (TDelegate)Delegate.CreateDelegate(typeof(TDelegate), closedMethod);
    }

    #endregion
}

