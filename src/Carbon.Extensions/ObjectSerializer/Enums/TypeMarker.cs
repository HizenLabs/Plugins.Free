namespace HizenLabs.Extensions.ObjectSerializer.Enums;

/// <summary>
/// Represents the type markers used in the serialization process.
/// This is used for generic type serialization.
/// </summary>
public enum TypeMarker : byte
{
    /// <summary>
    /// <see cref="TypeMarker"/> for <see langword="null"/>
    /// </summary>
    Null = 0,

    /// <summary>
    /// <see cref="TypeMarker"/> for <see cref="object"/>
    /// </summary>
    Object = 1,

    /// <summary>
    /// <see cref="TypeMarker"/> for <see cref="bool"/>
    /// </summary>
    Boolean = 2,

    /// <summary>
    /// <see cref="TypeMarker"/> for <see cref="byte"/>
    /// </summary>
    SByte = 3,

    /// <summary>
    /// <see cref="TypeMarker"/> for <see cref="byte"/>
    /// </summary>
    Byte = 4,

    /// <summary>
    /// <see cref="TypeMarker"/> for <see cref="short"/>
    /// </summary>
    Int16 = 5,

    /// <summary>
    /// <see cref="TypeMarker"/> for <see cref="ushort"/>
    /// </summary>
    UInt16 = 6,

    /// <summary>
    /// <see cref="TypeMarker"/> for <see cref="int"/>
    /// </summary>
    Int32 = 7,

    /// <summary>
    /// <see cref="TypeMarker"/> for <see cref="uint"/>
    /// </summary>
    UInt32 = 8,

    /// <summary>
    /// <see cref="TypeMarker"/> for <see cref="long"/>
    /// </summary>
    Int64 = 9,

    /// <summary>
    /// <see cref="TypeMarker"/> for <see cref="ulong"/>
    /// </summary>
    UInt64 = 10,

    /// <summary>
    /// <see cref="TypeMarker"/> for <see cref="float"/>
    /// </summary>
    Single = 11,

    /// <summary>
    /// <see cref="TypeMarker"/> for <see cref="double"/>
    /// </summary>
    Double = 12,

    /// <summary>
    /// <see cref="TypeMarker"/> for <see cref="decimal"/>
    /// </summary>
    Decimal = 13,

    /// <summary>
    /// <see cref="TypeMarker"/> for <see cref="char"/>
    /// </summary>
    Char = 14,

    /// <summary>
    /// <see cref="TypeMarker"/> for <see cref="string"/>
    /// </summary>
    String = 15,

    /// <summary>
    /// <see cref="TypeMarker"/> for <see cref="byte[]"/>
    /// </summary>
    ByteArray = 16,

    /// <summary>
    /// <see cref="TypeMarker"/> for <see cref="System.Enum"/> types
    /// </summary>
    /// <remarks>
    /// This encapsulates all enum types as they will be serialized as their underlying type.
    /// </remarks>
    Enum = 20,

    /// <summary>
    /// <see cref="TypeMarker"/> for <see cref="System.Guid"/>
    /// </summary>
    Guid = 21,

    /// <summary>
    /// <see cref="TypeMarker"/> for <see cref="System.DateTime"/>
    /// </summary>
    DateTime = 22,

    /// <summary>
    /// <see cref="TypeMarker"/> for <see cref="System.TimeSpan"/>
    /// </summary>
    TimeSpan = 23,

    /// <summary>
    /// <see cref="TypeMarker"/> for <see cref="System.Type"/>
    /// </summary>
    Type = 24,

    /// <summary>
    /// <see cref="TypeMarker"/> for <see cref="UnityEngine.Vector2"/>
    /// </summary>
    Vector2 = 50,

    /// <summary>
    /// <see cref="TypeMarker"/> for <see cref="UnityEngine.Vector3"/>
    /// </summary>
    Vector3 = 51,

    /// <summary>
    /// <see cref="TypeMarker"/> for <see cref="UnityEngine.Vector4"/>
    /// </summary>
    Vector4 = 52,

    /// <summary>
    /// <see cref="TypeMarker"/> for <see cref="UnityEngine.Quaternion"/>
    /// </summary>
    Quaternion = 53,

    /// <summary>
    /// <see cref="TypeMarker"/> for <see cref="UnityEngine.Color"/>
    /// </summary>
    Color = 54,

    // Custom types (100+)
    // PersistantEntity = 100,
    // PersistantItem = 101,
    // PlayerMetaData = 102,
    // PooledList = 103,
}
