using Facepunch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Carbon.Plugins;

public partial class AutoBuildSnapshot
{
    internal interface IPropertyMapping
    {
        void TryReadProperty(object obj, Dictionary<string, object> dict);
        void TryWriteProperty(object obj, Dictionary<string, object> dict);
    }

    /// <summary>
    /// Interface for property mapping.
    /// </summary>
    internal interface IPropertyMapping<in TObject> : IPropertyMapping
        where TObject : class
    {
        void TryReadProperty(TObject obj, Dictionary<string, object> dict);
        void TryWriteProperty(TObject obj, Dictionary<string, object> dict);
    }

    /// <summary>
    /// Represents a property mapping for a specific entity type.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    internal readonly struct PropertyMapping<TObject, TProperty> : IPropertyMapping<TObject>
        where TObject : class
    {
        /// <summary>
        /// The name of the property.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// The accessor function to get the property value.
        /// </summary>
        public Func<TObject, TProperty> Getter { get; init; }

        /// <summary>
        /// The setter function to set the property value.
        /// </summary>
        public Action<TObject, TProperty> Setter { get; init; }

        /// <summary>
        /// The condition to check if the property should be read.
        /// </summary>
        public Func<TObject, bool> Condition { get; init; }

        [Obsolete("Parameterless constructor for serialization purposes only. Do not use.", true)]
        public PropertyMapping() { }

        /// <summary>
        /// Creates a simplified property mapping based on the property name.
        /// </summary>
        /// <param name="propertyExpression">The expression to get the property value.</param>
        public PropertyMapping(Expression<Func<TObject, TProperty>> propertyExpression) : this(propertyExpression, null)
        {
        }

        /// <summary>
        /// Creates a simplified property mapping based on the property name and a condition.
        /// </summary>
        /// <param name="propertyExpression">The expression to get the property value.</param>
        /// <param name="condition">The condition to check if the property should be read.</param>
        public PropertyMapping(Expression<Func<TObject, TProperty>> propertyExpression, Func<TObject, bool> condition)
        {
            Name = GetExpressionName(propertyExpression);
            Getter = propertyExpression.Compile();
            Setter = CreateSetter(propertyExpression);
            Condition = condition;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyMapping{TObject, TProperty}"/> struct.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="getter">The accessor function to get the property value.</param>
        /// <param name="setter">The setter function to set the property value.</param>
        /// <param name="condition">The condition to check if the property should be read.</param>
        public PropertyMapping(
            string name, 
            Func<TObject, TProperty> getter,
            Action<TObject, TProperty> setter,
            Func<TObject, bool> condition = null)
        {
            Name = name;
            Getter = getter;
            Setter = setter;
            Condition = condition;
        }

        void IPropertyMapping.TryReadProperty(object obj, Dictionary<string, object> dict)
        {
            if (obj is TObject target)
            {
                TryReadProperty(target, dict);
            }
        }

        void IPropertyMapping.TryWriteProperty(object obj, Dictionary<string, object> dict)
        {
            if (obj is TObject target)
            {
                TryWriteProperty(target, dict);
            }
        }

        /// <summary>
        /// Reads the property value from the object and adds it to the dictionary.
        /// </summary>
        /// <param name="object">The object to read from.</param>
        /// <param name="dict">The dictionary to add the property value to.</param>
        public void TryReadProperty(TObject obj, Dictionary<string, object> dict)
        {
            if (obj is TObject target)
            {
                if (Condition == null || Condition(target))
                {
                    var value = Getter(target);
                    if (value is not null && !value.Equals(default(TProperty)))
                    {
                        dict[Name] = value;
                    }
                }
            }
        }

        /// <summary>
        /// Writes the property value to the object from the dictionary.
        /// </summary>
        /// <param name="object">The object to write to.</param>
        /// <param name="dict">The dictionary to read the property value from.</param>
        public void TryWriteProperty(TObject obj, Dictionary<string, object> dict)
        {
            if (obj is TObject target)
            {
                if (dict.TryGetValue(Name, out var value) && value is TProperty property)
                {
                    Setter(target, property);
                }
            }
        }
        private static string GetExpressionName(Expression<Func<TObject, TProperty>> propertyExpression)
        {
            // Similar to above but also include type information
            using var memberNames = Pool.Get<PooledList<string>>();
            using var memberTypes = Pool.Get<PooledList<string>>();
            Expression expression = propertyExpression.Body;

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
        private static Action<TObject, TProperty> CreateSetter(Expression<Func<TObject, TProperty>> propertyExpression)
        {
            // The property expression needs to be a MemberExpression
            if (propertyExpression.Body is not MemberExpression memberExpression)
            {
                return null;
            }

            // Create parameter expressions for the lambda
            var instanceParam = Expression.Parameter(typeof(TObject), "instance");
            var valueParam = Expression.Parameter(typeof(TProperty), "value");

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
            var lambda = Expression.Lambda<Action<TObject, TProperty>>(
                assignExpression, instanceParam, valueParam);

            return lambda.Compile();
        }
    }
}
