using HizenLabs.Extensions.ObjectSerializer.Extensions;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace HizenLabs.Extensions.ObjectSerializer.Internal;

/// <summary>
/// Provides methods for reading generic instances of <typeparamref name="T"/> from a <see cref="BinaryReader"/>.
/// </summary>
/// typeparam name="T">The type of the object to read.</typeparam>
internal class GenericReader<T>
{
    /// <summary>
    /// The delegate that reads the <typeparamref name="T"/> from the <see cref="BinaryReader"/>.
    /// </summary>
    public static Func<BinaryReader, T> Read { get; }

    /// <summary>
    /// Initializes the <see cref="GenericReader{T}"/> class.
    /// </summary>
    /// <exception cref="NotSupportedException">Thrown when the type is not supported.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the type is object.</exception>
    static GenericReader()
    {
        var type = typeof(T);

        if (type == typeof(object)) Read = _ => throw new InvalidOperationException("Cannot read object type directly. Use a specific type instead.");

        else if (type == typeof(bool)) Read = r => { var v = r.ReadBoolean(); return Unsafe.As<bool, T>(ref v); };
        else if (type == typeof(sbyte)) Read = r => { var v = r.ReadSByte(); return Unsafe.As<sbyte, T>(ref v); };
        else if (type == typeof(byte)) Read = r => { var v = r.ReadByte(); return Unsafe.As<byte, T>(ref v); };
        else if (type == typeof(short)) Read = r => { var v = r.ReadInt16(); return Unsafe.As<short, T>(ref v); };
        else if (type == typeof(ushort)) Read = r => { var v = r.ReadUInt16(); return Unsafe.As<ushort, T>(ref v); };
        else if (type == typeof(int)) Read = r => { var v = r.ReadInt32(); return Unsafe.As<int, T>(ref v); };
        else if (type == typeof(uint)) Read = r => { var v = r.ReadUInt32(); return Unsafe.As<uint, T>(ref v); };
        else if (type == typeof(long)) Read = r => { var v = r.ReadInt64(); return Unsafe.As<long, T>(ref v); };
        else if (type == typeof(ulong)) Read = r => { var v = r.ReadUInt64(); return Unsafe.As<ulong, T>(ref v); };
        else if (type == typeof(float)) Read = r => { var v = r.ReadSingle(); return Unsafe.As<float, T>(ref v); };
        else if (type == typeof(double)) Read = r => { var v = r.ReadDouble(); return Unsafe.As<double, T>(ref v); };
        else if (type == typeof(decimal)) Read = r => { var v = r.ReadDecimal(); return Unsafe.As<decimal, T>(ref v); };
        else if (type == typeof(char)) Read = r => { var v = r.ReadChar(); return Unsafe.As<char, T>(ref v); };

        else if (type == typeof(string)) Read = r => (T)(object)r.ReadString()!;

        else if (type == typeof(Type)) Read = r => (T)(object)Type.GetType(r.ReadString())!;

        else if (type.IsEnum) Read = r => EnumReader<T>.Read(r);

        else if (type == typeof(Guid)) Read = r => { var v = r.ReadGuid(); return Unsafe.As<Guid, T>(ref v); };
        else if (type == typeof(DateTime)) Read = r => { var v = r.ReadDateTime(); return Unsafe.As<DateTime, T>(ref v); };
        else if (type == typeof(TimeSpan)) Read = r => { var v = r.ReadTimeSpan(); return Unsafe.As<TimeSpan, T>(ref v); };

        else if (type == typeof(Vector2)) Read = r => { var v = r.ReadVector2(); return Unsafe.As<Vector2, T>(ref v); };
        else if (type == typeof(Vector3)) Read = r => { var v = r.ReadVector3(); return Unsafe.As<Vector3, T>(ref v); };
        else if (type == typeof(Vector4)) Read = r => { var v = r.ReadVector4(); return Unsafe.As<Vector4, T>(ref v); };
        else if (type == typeof(Quaternion)) Read = r => { var v = r.ReadQuaternion(); return Unsafe.As<Quaternion, T>(ref v); };
        else if (type == typeof(Color)) Read = r => { var v = r.ReadColor(); return Unsafe.As<Color, T>(ref v); };

        else Read = _ => throw new NotSupportedException($"Type '{type}' is not supported.");
    }
}
