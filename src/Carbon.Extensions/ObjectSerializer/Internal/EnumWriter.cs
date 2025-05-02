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

        bool unsafeCast = true;
        if (enumType == typeof(object))
        {
            unsafeCast = false;
            enumType = enumType.GetType();
        }

        if (!enumType.IsEnum) throw new EnumSerializationException(enumType);

        var underlyingType = Enum.GetUnderlyingType(enumType);
        if (underlyingType == typeof(byte)) Write = (w, v) => w.Write(unsafeCast ? Unsafe.As<TEnum, byte>(ref v) : (byte)(object)v);
        else if (underlyingType == typeof(sbyte)) Write = (w, v) => w.Write(unsafeCast ? Unsafe.As<TEnum, sbyte>(ref v) : (sbyte)(object)v);
        else if (underlyingType == typeof(short)) Write = (w, v) => w.Write(unsafeCast ? Unsafe.As<TEnum, short>(ref v) : (short)(object)v);
        else if (underlyingType == typeof(ushort)) Write = (w, v) => w.Write(unsafeCast ? Unsafe.As<TEnum, ushort>(ref v) : (ushort)(object)v);
        else if (underlyingType == typeof(int)) Write = (w, v) => w.Write(unsafeCast ? Unsafe.As<TEnum, int>(ref v) : (int)(object)v);
        else if (underlyingType == typeof(uint)) Write = (w, v) => w.Write(unsafeCast ? Unsafe.As<TEnum, uint>(ref v) : (uint)(object)v);
        else if (underlyingType == typeof(long)) Write = (w, v) => w.Write(unsafeCast ? Unsafe.As<TEnum, long>(ref v) : (long)(object)v);
        else if (underlyingType == typeof(ulong)) Write = (w, v) => w.Write(unsafeCast ? Unsafe.As<TEnum, ulong>(ref v) : (ulong)(object)v);
        else throw new EnumSerializationException(enumType);
    }
}