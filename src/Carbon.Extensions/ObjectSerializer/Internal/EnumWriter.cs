using HizenLabs.Extensions.ObjectSerializer.Exceptions;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace HizenLabs.Extensions.ObjectSerializer.Internal;

/// <summary>
/// Provides methods for writing <see cref="Enum"/> values to a <see cref="BinaryWriter"/>.
/// </summary>
/// typeparam name="TEnum">The type of the enum.</typeparam>
internal static class EnumWriter<TEnum>
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

        if (!enumType.IsEnum) throw new EnumSerializationException(enumType);

        var underlyingType = Enum.GetUnderlyingType(enumType);

        if (underlyingType == typeof(byte)) Write = (w, v) => w.Write(Unsafe.As<TEnum, byte>(ref v));
        else if (underlyingType == typeof(sbyte)) Write = (w, v) => w.Write(Unsafe.As<TEnum, sbyte>(ref v));
        else if (underlyingType == typeof(short)) Write = (w, v) => w.Write(Unsafe.As<TEnum, short>(ref v));
        else if (underlyingType == typeof(ushort)) Write = (w, v) => w.Write(Unsafe.As<TEnum, ushort>(ref v));
        else if (underlyingType == typeof(int)) Write = (w, v) => w.Write(Unsafe.As<TEnum, int>(ref v));
        else if (underlyingType == typeof(uint)) Write = (w, v) => w.Write(Unsafe.As<TEnum, uint>(ref v));
        else if (underlyingType == typeof(long)) Write = (w, v) => w.Write(Unsafe.As<TEnum, long>(ref v));
        else if (underlyingType == typeof(ulong)) Write = (w, v) => w.Write(Unsafe.As<TEnum, ulong>(ref v));
        else Write = (w, v) => throw new EnumSerializationException(typeof(TEnum));
    }
}