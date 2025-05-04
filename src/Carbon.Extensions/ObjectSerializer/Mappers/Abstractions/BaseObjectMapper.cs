using HizenLabs.Extensions.ObjectSerializer.Serialization;

namespace HizenLabs.Extensions.ObjectSerializer.Mappers.Abstractions;

/// <summary>
/// Base class for all mappers, which are just serializable representations of Unity objects.
/// Implementations should be stateless and thread-safe.
/// </summary>
/// <typeparam name="TOriginal">The type of the original object.</typeparam>
public abstract class BaseObjectMapper<TOriginal> : IObjectMapper
{
    /// <summary>
    /// Registers a <see cref="ObjectDataMapping{TObject, TMemberType}"/>This should be done during construction.
    /// </summary>
    protected void SetupMapping()
    {
    }

    /// <summary>
    /// First stage of serialization. This is called during the initial object serialization.
    /// Not all objects are guaranteed to be ready at this stage.
    /// </summary>
    /// <param name="source">The object to serialize.</param>
    /// <param name="target">The target object to serialize to.</param>
    protected virtual void OnSerializeSelf(TOriginal source, SerializableObject target)
    {
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
    protected virtual void OnDeserializeSelf(TOriginal source, SerializableObject target)
    {
    }

    /// <summary>
    /// Final stage of deserialization. This executes after all objects have been deserialized.
    /// </summary>
    /// <param name="source">The object to deserialize from.</param>
    /// <param name="target">The target object to deserialize to.</param>
    /// <param name="context">The serialization context.</param>
    protected virtual void OnDeserializeComplete(TOriginal source, SerializableObject target, SerializationContext context)
    {
    }
}
