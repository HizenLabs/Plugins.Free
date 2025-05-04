namespace HizenLabs.Extensions.ObjectSerializer.Enums;

/// <summary>
/// Represents the object types used in the serialization process.
/// </summary>
public enum ObjectType : ushort
{
    /// <summary>
    /// Represents an unknown object type.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Represents a BaseEntity
    /// </summary>
    BaseEntity = 1,

    /// <summary>
    /// Represents an Item
    /// </summary>
    Item = 2,

    /// <summary>
    /// Represents a BasePlayer
    /// </summary>
    BasePlayer = 3
}
