using HizenLabs.Extensions.ObjectSerializer.Mappers.Abstractions;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System;
using Facepunch;
using System.Linq;

namespace HizenLabs.Extensions.ObjectSerializer.Mappers;

/// <summary>
/// Represents a field or property mapping for a specific object type.
/// </summary>
/// <typeparam name="TObject">The type of the object.</typeparam>
/// <typeparam name="TMemberType">The type of the member.</typeparam>
internal readonly struct ObjectDataMapping<TObject, TMemberType> : IObjectDataMapping<TObject>
    where TObject : class
{
    /// <summary>
    /// The name of the member.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// The accessor function to get the member value.
    /// </summary>
    public Func<TObject, TMemberType> Getter { get; init; }

    /// <summary>
    /// The setter function to set the member value.
    /// </summary>
    public Action<TObject, TMemberType> Setter { get; init; }

    /// <summary>
    /// The condition to check if the member should be read.
    /// </summary>
    public Func<TObject, bool> Condition { get; init; }

    /// <summary>
    /// Creates a simplified member mapping based on the member name.
    /// </summary>
    /// <param name="memberExpression">The expression to get the member value.</param>
    public ObjectDataMapping(Expression<Func<TObject, TMemberType>> memberExpression) : this(memberExpression, null)
    {
    }

    /// <summary>
    /// Creates a simplified member mapping based on the member name and a condition.
    /// </summary>
    /// <param name="memberExpression">The expression to get the member value.</param>
    /// <param name="condition">The condition to check if the member should be read.</param>
    public ObjectDataMapping(Expression<Func<TObject, TMemberType>> memberExpression, Func<TObject, bool> condition)
    {
        Name = GetExpressionName(memberExpression);
        Getter = memberExpression.Compile();
        Setter = CreateSetter(memberExpression);
        Condition = condition;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectDataMapping{TObject, TMemberType}"/> struct.
    /// </summary>
    /// <param name="name">The name of the member.</param>
    /// <param name="getter">The accessor function to get the member value.</param>
    /// <param name="setter">The setter function to set the member value.</param>
    /// <param name="condition">The condition to check if the member should be read.</param>
    public ObjectDataMapping(
        string name,
        Func<TObject, TMemberType> getter,
        Action<TObject, TMemberType> setter,
        Func<TObject, bool> condition = null)
    {
        Name = name;
        Getter = getter;
        Setter = setter;
        Condition = condition;
    }

    /// <summary>
    /// Attempts to read the member value from the object and add it to the dictionary.
    /// </summary>
    /// <param name="obj">The object to read from.</param>
    /// <param name="dict">The dictionary to add the member value to.</param>
    void IObjectDataMapping.TryRead(object obj, Dictionary<string, object> dict)
    {
        if (obj is TObject target)
        {
            TryRead(target, dict);
        }
    }

    /// <summary>
    /// Attempts to write the member value to the object from the dictionary.
    /// </summary>
    /// <param name="obj">The object to write to.</param>
    /// <param name="dict">The dictionary to read the member value from.</param>
    void IObjectDataMapping.TryWrite(object obj, Dictionary<string, object> dict)
    {
        if (obj is TObject target)
        {
            TryWrite(target, dict);
        }
    }

    /// <summary>
    /// Reads the member value from the object and adds it to the dictionary.
    /// </summary>
    /// <param name="obj">The object to read from.</param>
    /// <param name="dict">The dictionary to add the member value to.</param>
    public void TryRead(TObject obj, Dictionary<string, object> dict)
    {
        if (obj is TObject target)
        {
            if (Condition == null || Condition(target))
            {
                var value = Getter(target);
                if (value is not null && !value.Equals(default(TMemberType)))
                {
                    dict[Name] = value;
                }
            }
        }
    }

    /// <summary>
    /// Writes the member value to the object from the dictionary.
    /// </summary>
    /// <param name="obj">The object to write to.</param>
    /// <param name="dict">The dictionary to read the property value from.</param>
    public void TryWrite(TObject obj, Dictionary<string, object> dict)
    {
        if (obj is TObject target)
        {
            if (dict.TryGetValue(Name, out var value) && value is TMemberType member)
            {
                Setter(target, member);
            }
        }
    }

    /// <summary>
    /// Gets the name of the member from the expression.
    /// </summary>
    /// <param name="dataExpression">The member data expression.</param>
    /// <returns>The name of the member.</returns>
    private static string GetExpressionName(Expression<Func<TObject, TMemberType>> dataExpression)
    {
        // Similar to above but also include type information
        using var memberNames = Pool.Get<PooledList<string>>();
        using var memberTypes = Pool.Get<PooledList<string>>();
        Expression expression = dataExpression.Body;

        while (expression is MemberExpression memberExpression)
        {
            memberNames.Add(memberExpression.Member.Name);
            memberTypes.Add(memberExpression.Type.Name);
            expression = memberExpression.Expression;
        }

        memberNames.Reverse();
        memberTypes.Reverse();

        // Create a path that includes type information
        var path = new List<string>();
        for (int i = 0; i < memberNames.Count; i++)
        {
            path.Add($"{memberNames[i]}:{memberTypes[i]}");
        }

        return $"{typeof(TObject).Name}.{string.Join(".", path)}";
    }

    /// <summary>
    /// Creates a setter from a property or field expression, properly handling nested properties.
    /// </summary>
    private static Action<TObject, TMemberType> CreateSetter(Expression<Func<TObject, TMemberType>> propertyExpression)
    {
        // The property expression needs to be a MemberExpression
        if (propertyExpression.Body is not MemberExpression memberExpression)
        {
            return null;
        }

        // Create parameter expressions for the lambda
        var instanceParam = Expression.Parameter(typeof(TObject), "instance");
        var valueParam = Expression.Parameter(typeof(TMemberType), "value");

        // Collect all member expressions in the chain
        var memberExpressions = new List<MemberExpression>();
        Expression currentExpression = memberExpression;

        // Build the chain of member expressions
        while (currentExpression is MemberExpression currentMemberExpression)
        {
            memberExpressions.Add(currentMemberExpression);
            currentExpression = currentMemberExpression.Expression;
        }

        // Reverse to start from the root object
        memberExpressions.Reverse();

        // The parameter should be at the base of the expression
        if (currentExpression is not ParameterExpression)
        {
            return null;
        }

        // Build the property access chain starting from the instance parameter
        Expression propertyAccess = instanceParam;

        // Build all but the last property access
        for (int i = 0; i < memberExpressions.Count - 1; i++)
        {
            var member = memberExpressions[i].Member;
            if (member is PropertyInfo property)
            {
                propertyAccess = Expression.Property(propertyAccess, property);
            }
            else if (member is FieldInfo field)
            {
                propertyAccess = Expression.Field(propertyAccess, field);
            }
            else
            {
                // Unsupported member type
                return null;
            }
        }

        // Get the last member info (the one we want to set)
        var lastMember = memberExpressions.Last().Member;

        // Create the property access and assignment
        Expression lastAccess;
        if (lastMember is PropertyInfo lastProperty)
        {
            if (lastProperty.SetMethod == null || !lastProperty.SetMethod.IsPublic)
                return null; // No public setter available

            lastAccess = Expression.Property(propertyAccess, lastProperty);
        }
        else if (lastMember is FieldInfo lastField)
        {
            if (!lastField.IsPublic)
                return null; // Not a public field

            lastAccess = Expression.Field(propertyAccess, lastField);
        }
        else
        {
            // Unsupported member type
            return null;
        }

        var assignExpression = Expression.Assign(lastAccess, valueParam);

        // Create and compile the lambda expression
        var lambda = Expression.Lambda<Action<TObject, TMemberType>>(
            assignExpression, instanceParam, valueParam);

        return lambda.Compile();
    }
}