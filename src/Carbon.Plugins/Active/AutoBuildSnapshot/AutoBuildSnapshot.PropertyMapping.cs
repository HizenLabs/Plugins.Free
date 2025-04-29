using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Carbon.Plugins;

public partial class AutoBuildSnapshot
{
    private interface IPropertyMapping
    {
        void TryReadProperty(object obj, Dictionary<string, object> dict);
        void TryWriteProperty(object obj, Dictionary<string, object> dict);
    }

    /// <summary>
    /// Interface for property mapping.
    /// </summary>
    private interface IPropertyMapping<in TObject> : IPropertyMapping
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
    private readonly struct PropertyMapping<TObject, TProperty> : IPropertyMapping<TObject>
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
                    dict[Name] = Getter(target);
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
            var memberNames = new List<string>();
            var memberTypes = new List<string>();
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
        /// Creates a setter from a property or field expression.
        /// </summary>
        private static Action<TObject, TProperty> CreateSetter(Expression<Func<TObject, TProperty>> propertyExpression)
        {
            // Get the member expression (e.g., obj.Property or obj.Field)
            if (!(propertyExpression.Body is MemberExpression memberExpression))
            {
                // Handle more complex expressions (like nested properties)
                return CreateComplexSetter(propertyExpression);
            }
            
            // Check if it's a property or field
            var member = memberExpression.Member;

            if (member is PropertyInfo property)
            {
                if (property.SetMethod == null || !property.SetMethod.IsPublic)
                    return null; // No public setter available

                // Create parameter expressions for the lambda
                var instanceParam = Expression.Parameter(typeof(TObject), "instance");
                var valueParam = Expression.Parameter(typeof(TProperty), "value");

                // Create the property access and assignment
                var propertyAccess = Expression.Property(instanceParam, property);
                var assign = Expression.Assign(propertyAccess, valueParam);

                // Create and compile the lambda expression
                var lambda = Expression.Lambda<Action<TObject, TProperty>>(
                    assign, instanceParam, valueParam);

                return lambda.Compile();
            }
            else if (member is FieldInfo field)
            {
                if (!field.IsPublic)
                    return null; // Not a public field

                // Create parameter expressions for the lambda
                var instanceParam = Expression.Parameter(typeof(TObject), "instance");
                var valueParam = Expression.Parameter(typeof(TProperty), "value");

                // Create the field access and assignment
                var fieldAccess = Expression.Field(instanceParam, field);
                var assign = Expression.Assign(fieldAccess, valueParam);

                // Create and compile the lambda expression
                var lambda = Expression.Lambda<Action<TObject, TProperty>>(
                    assign, instanceParam, valueParam);

                return lambda.Compile();
            }

            return null;
        }

        /// <summary>
        /// Creates a setter for more complex property chains like obj => obj.SubObj.Property
        /// </summary>
        private static Action<TObject, TProperty> CreateComplexSetter(Expression<Func<TObject, TProperty>> propertyExpression)
        {
            try
            {
                // For complex paths, we'll create a more intricate setter logic
                // This is much more complex and would need extensive testing
                // Here's a basic implementation that tries to create a setter for common scenarios

                // Create parameter expressions for the lambda
                var instanceParam = Expression.Parameter(typeof(TObject), "instance");
                var valueParam = Expression.Parameter(typeof(TProperty), "value");

                // Recreate the chain of access but with our parameters
                var visitor = new MemberAccessVisitor(propertyExpression.Parameters[0], instanceParam);
                var accessChain = visitor.Visit(propertyExpression.Body);

                // Create the assignment
                var assign = Expression.Assign(accessChain, valueParam);

                // Create and compile the lambda expression
                var lambda = Expression.Lambda<Action<TObject, TProperty>>(
                    assign, instanceParam, valueParam);

                return lambda.Compile();
            }
            catch
            {
                // If we can't create a setter, return null
                return null;
            }
        }

        // Helper class to visit and rebuild member access chains
        private class MemberAccessVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _original;
            private readonly ParameterExpression _replacement;

            public MemberAccessVisitor(ParameterExpression original, ParameterExpression replacement)
            {
                _original = original;
                _replacement = replacement;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == _original ? _replacement : base.VisitParameter(node);
            }
        }
    }
}
