using HizenLabs.Extensions.ObjectSerializer.Exceptions;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace HizenLabs.Extensions.ObjectSerializer.Internal;

/// <summary>
/// Provides methods for reading <see cref="Enum"/> values from a <see cref="BinaryReader"/>.
/// </summary>
/// typeparam name="TEnum">The type of the enum.</typeparam>
internal static class EnumReader<TEnum>
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

        if (!enumType.IsEnum) throw new EnumSerializationException(enumType);

        var underlyingType = Enum.GetUnderlyingType(enumType);

        if (underlyingType.Equals(typeof(byte))) Read = r => { var v = r.ReadByte(); return Unsafe.As<byte, TEnum>(ref v); };
        else if (underlyingType.Equals(typeof(sbyte))) Read = r => { var v = r.ReadSByte(); return Unsafe.As<sbyte, TEnum>(ref v); };
        else if (underlyingType.Equals(typeof(short))) Read = r => { var v = r.ReadInt16(); return Unsafe.As<short, TEnum>(ref v); };
        else if (underlyingType.Equals(typeof(ushort))) Read = r => { var v = r.ReadUInt16(); return Unsafe.As<ushort, TEnum>(ref v); };
        else if (underlyingType.Equals(typeof(int))) Read = r => { var v = r.ReadInt32(); return Unsafe.As<int, TEnum>(ref v); };
        else if (underlyingType.Equals(typeof(uint))) Read = r => { var v = r.ReadUInt32(); return Unsafe.As<uint, TEnum>(ref v); };
        else if (underlyingType.Equals(typeof(long))) Read = r => { var v = r.ReadInt64(); return Unsafe.As<long, TEnum>(ref v); };
        else if (underlyingType.Equals(typeof(ulong))) Read = r => { var v = r.ReadUInt64(); return Unsafe.As<ulong, TEnum>(ref v); };
        else Read = (r) => throw new EnumSerializationException(enumType);
    }
}