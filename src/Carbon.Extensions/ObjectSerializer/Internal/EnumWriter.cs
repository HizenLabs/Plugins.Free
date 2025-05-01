using HizenLabs.Extensions.ObjectSerializer.Exceptions;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace HizenLabs.Extensions.ObjectSerializer.Internal;

/// <summary>
/// Provides methods for writing <see cref="Enum"/> values to a <see cref="BinaryWriter"/>.
/// </summary>
/// typeparam name="TEnum">The type of the enum.</typeparam>
internal static class EnumWriter<TEnum> where TEnum : unmanaged, Enum
{
    /// <summary>
    /// Writes an <see cref="Enum"/> value to the current stream and advances the stream position by the size of the underlying type.
    /// </summary>
    public static Action<BinaryWriter, TEnum> Write { get; }

    /// <summary>
    /// Initializes the <see cref="EnumWriter{TEnum}"/> class.
    /// </summary>
    /// <exception cref="EnumSerializationException"></exception>
    static EnumWriter()
    {
        var enumType = typeof(TEnum);
        var handle = Enum.GetUnderlyingType(enumType).TypeHandle;

        if (handle.Equals(typeof(byte).TypeHandle)) Write = (w, v) => w.Write(UnsafeCast<byte>(v));
        else if (handle.Equals(typeof(sbyte).TypeHandle)) Write = (w, v) => w.Write(UnsafeCast<sbyte>(v));
        else if (handle.Equals(typeof(short).TypeHandle)) Write = (w, v) => w.Write(UnsafeCast<short>(v));
        else if (handle.Equals(typeof(ushort).TypeHandle)) Write = (w, v) => w.Write(UnsafeCast<ushort>(v));
        else if (handle.Equals(typeof(int).TypeHandle)) Write = (w, v) => w.Write(UnsafeCast<int>(v));
        else if (handle.Equals(typeof(uint).TypeHandle)) Write = (w, v) => w.Write(UnsafeCast<uint>(v));
        else if (handle.Equals(typeof(long).TypeHandle)) Write = (w, v) => w.Write(UnsafeCast<long>(v));
        else if (handle.Equals(typeof(ulong).TypeHandle)) Write = (w, v) => w.Write(UnsafeCast<ulong>(v));
        else Write = (w, v) => throw new EnumSerializationException(typeof(TEnum));
    }

    /// <summary>
    /// Converts the given value to the specified enum type using unsafe casting.
    /// </summary>
    /// <typeparam name="T">The type of the value to convert.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted enum value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe T UnsafeCast<T>(TEnum value)
        where T : unmanaged => *(T*)&value;
}