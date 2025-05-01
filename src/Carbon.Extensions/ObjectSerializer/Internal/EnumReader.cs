using HizenLabs.Extensions.ObjectSerializer.Exceptions;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace HizenLabs.Extensions.ObjectSerializer.Internal;

/// <summary>
/// Provides methods for reading <see cref="Enum"/> values from a <see cref="BinaryReader"/>.
/// </summary>
/// typeparam name="TEnum">The type of the enum.</typeparam>
internal static class EnumReader<TEnum> where TEnum : unmanaged, Enum
{
    /// <summary>
    /// Reads an <see cref="Enum"/> value from the current stream and advances the stream position by the size of the underlying type.
    /// </summary>
    public static Func<BinaryReader, TEnum> Read { get; }

    /// <summary>
    /// Initializes the <see cref="EnumReader{TEnum}"/> class.
    /// </summary>
    /// <exception cref="EnumSerializationException"></exception>
    static EnumReader()
    {
        var enumType = typeof(TEnum);
        var handle = Enum.GetUnderlyingType(enumType).TypeHandle;

        if (handle.Equals(typeof(byte).TypeHandle)) Read = (r) => UnsafeCast(r.ReadByte());
        else if (handle.Equals(typeof(sbyte).TypeHandle)) Read = (r) => UnsafeCast(r.ReadSByte());
        else if (handle.Equals(typeof(short).TypeHandle)) Read = (r) => UnsafeCast(r.ReadInt16());
        else if (handle.Equals(typeof(ushort).TypeHandle)) Read = (r) => UnsafeCast(r.ReadUInt16());
        else if (handle.Equals(typeof(int).TypeHandle)) Read = (r) => UnsafeCast(r.ReadInt32());
        else if (handle.Equals(typeof(uint).TypeHandle)) Read = (r) => UnsafeCast(r.ReadUInt32());
        else if (handle.Equals(typeof(long).TypeHandle)) Read = (r) => UnsafeCast(r.ReadInt64());
        else if (handle.Equals(typeof(ulong).TypeHandle)) Read = (r) => UnsafeCast(r.ReadUInt64());
        else Read = (r) => throw new EnumSerializationException(enumType);
    }

    /// <summary>
    /// Converts the given value to the specified enum type using unsafe casting.
    /// </summary>
    /// <typeparam name="T">The type of the value to convert.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted enum value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe TEnum UnsafeCast<T>(T value) 
        where T : unmanaged => *(TEnum*)&value;
}