using HizenLabs.Extensions.ObjectSerializer.Serialization;

namespace HizenLabs.Extensions.ObjectSerializer.Mappers.Abstractions;

/// <summary>
/// Interface for object mappers.
/// </summary>
internal interface IObjectMapper
{
    void SerializeSelf(object source, SerializableObject target);
    void SerializeComplete(object source, SerializableObject target, SerializationContext context);
    object DeserializeSelf(SerializableObject source);
    void DeserializeComplete(SerializableObject source, object target, SerializationContext context);
}