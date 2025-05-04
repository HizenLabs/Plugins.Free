using Facepunch.Extend;
using HizenLabs.Extensions.ObjectSerializer.Serialization;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace HizenLabs.Extensions.ObjectSerializer.Mappers.Abstractions;

/// <summary>
/// Base class for all mappers, which are just serializable representations of Unity objects.
/// Implementations should be stateless and thread-safe.
/// </summary>
/// <typeparam name="TOriginal">The type of the original object.</typeparam>
public abstract class BaseObjectMapper<TOriginal> : IObjectMapper
    where TOriginal : class
{
    private readonly List<IObjectDataMapping> _mappings = new();

    /// <summary>
    /// Registers a ObjectDataMapping for serialization/deserialization of specific properties.
    /// </summary>
    /// <typeparam name="TMemberType">The type of the member.</typeparam>
    /// <param name="memberExpression">Expression to get the member value.</param>
    protected void SetupMapping<TMemberType>(Expression<Func<TOriginal, TMemberType>> memberExpression)
    {
        var mapping = new ObjectDataMapping<TOriginal, TMemberType>(memberExpression);
        _mappings.Add(mapping);
    }

    /// <summary>
    /// Registers a ObjectDataMapping for serialization/deserialization of specific properties with a condition.
    /// </summary>
    /// <typeparam name="TMemberType">The type of the member.</typeparam>
    /// <param name="memberExpression">Expression to get the member value.</param>
    /// <param name="condition">Condition to determine if the member should be serialized.</param>
    protected void SetupMapping<TMemberType>(
        Expression<Func<TOriginal, TMemberType>> memberExpression, 
        Func<TOriginal, bool> condition)
    {
        var mapping = new ObjectDataMapping<TOriginal, TMemberType>(memberExpression, condition);
        _mappings.Add(mapping);
    }

    /// <summary>
    /// Registers a ObjectDataMapping for serialization/deserialization of specific properties with custom getter/setter.
    /// </summary>
    /// <typeparam name="TMemberType">The type of the member.</typeparam>
    /// <param name="name">The name of the member.</param>
    /// <param name="getter">Function to get the member value.</param>
    /// <param name="setter">Function to set the member value.</param>
    /// <param name="condition">Optional condition to determine if the member should be serialized.</param>
    protected void SetupMapping<TMemberType>(
        string name, 
        Func<TOriginal, TMemberType> getter, 
        Action<TOriginal, TMemberType> setter, 
        Func<TOriginal, bool> condition = null)
    {
        var mapping = new ObjectDataMapping<TOriginal, TMemberType>(name, getter, setter, condition);
        _mappings.Add(mapping);
    }

    /// <summary>
    /// First stage of serialization. This is called during the initial object serialization.
    /// Not all objects are guaranteed to be ready at this stage.
    /// </summary>
    /// <param name="source">The object to serialize.</param>
    /// <param name="target">The target object to serialize to.</param>
    protected virtual void OnSerializeSelf(TOriginal source, SerializableObject target)
    {
        foreach (var mapping in _mappings)
        {
            mapping.TryWrite(source, target.Properties);
        }
    }

    /// <summary>
    /// Second stage of serialization. This executes after all objects have been initially serialized.
    /// </summary>
    /// <param name="source">The object to serialize.</param>
    /// <param name="target">The target object to serialize to.</param>
    /// <param name="context">The serialization context.</param>
    protected virtual void OnSerializeComplete(TOriginal source, SerializableObject target, SerializationContext context)
    {
    }

    /// <summary>
    /// First stage of deserialization. This is called during the initial object deserialization.
    /// </summary>
    /// <param name="source">The object to deserialize from.</param>
    /// <param name="target">The target object to deserialize to.</param>
    protected virtual void OnDeserializeSelf(SerializableObject source, TOriginal target)
    {
        foreach (var mapping in _mappings)
        {
            mapping.TryRead(target, source.Properties);
        }
    }

    /// <summary>
    /// Final stage of deserialization. This executes after all objects have been deserialized.
    /// </summary>
    /// <param name="source">The object to deserialize from.</param>
    /// <param name="target">The target object to deserialize to.</param>
    /// <param name="context">The serialization context.</param>
    protected virtual void OnDeserializeComplete(SerializableObject source, TOriginal target, SerializationContext context)
    {
    }

    void IObjectMapper.SerializeSelf(object source, SerializableObject target)
    {
        if (source is TOriginal original)
        {
            OnSerializeSelf(original, target);
        }
    }

    void IObjectMapper.SerializeComplete(object source, SerializableObject target, SerializationContext context)
    {
        if (source is TOriginal original)
        {
            OnSerializeComplete(original, target, context);
        }
    }

    void IObjectMapper.DeserializeSelf(SerializableObject source, object target)
    {
        if (target is TOriginal original)
        {
            OnDeserializeSelf(source, original);
        }
    }

    void IObjectMapper.DeserializeComplete(SerializableObject source, object target, SerializationContext context)
    {
        if (target is TOriginal original)
        {
            OnDeserializeComplete(source, original, context);
        }
    }
}
