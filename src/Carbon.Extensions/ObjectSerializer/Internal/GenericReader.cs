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
    public static Func<BinaryReader, object, object, object, T> Read { get; }

    /// <summary>
    /// Initializes the <see cref="GenericReader{T}"/> class.
    /// </summary>
    /// <exception cref="NotSupportedException">Thrown when the type is not supported.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the type is object.</exception>
    static GenericReader()
    {
        var type = typeof(T);

        if (type == typeof(object)) Read = (_, _, _, _) => throw new InvalidOperationException("Cannot read object type directly. Use a specific type instead.");

        else if (type == typeof(bool)) Read = (r, _, _, _) => { var v = r.ReadBoolean(); return Unsafe.As<bool, T>(ref v); };
        else if (type == typeof(sbyte)) Read = (r, _, _, _) => { var v = r.ReadSByte(); return Unsafe.As<sbyte, T>(ref v); };
        else if (type == typeof(byte)) Read = (r, _, _, _) => { var v = r.ReadByte(); return Unsafe.As<byte, T>(ref v); };
        else if (type == typeof(short)) Read = (r, _, _, _) => { var v = r.ReadInt16(); return Unsafe.As<short, T>(ref v); };
        else if (type == typeof(ushort)) Read = (r, _, _, _) => { var v = r.ReadUInt16(); return Unsafe.As<ushort, T>(ref v); };
        else if (type == typeof(int)) Read = (r, _, _, _) => { var v = r.ReadInt32(); return Unsafe.As<int, T>(ref v); };
        else if (type == typeof(uint)) Read = (r, _, _, _) => { var v = r.ReadUInt32(); return Unsafe.As<uint, T>(ref v); };
        else if (type == typeof(long)) Read = (r, _, _, _) => { var v = r.ReadInt64(); return Unsafe.As<long, T>(ref v); };
        else if (type == typeof(ulong)) Read = (r, _, _, _) => { var v = r.ReadUInt64(); return Unsafe.As<ulong, T>(ref v); };
        else if (type == typeof(float)) Read = (r, _, _, _) => { var v = r.ReadSingle(); return Unsafe.As<float, T>(ref v); };
        else if (type == typeof(double)) Read = (r, _, _, _) => { var v = r.ReadDouble(); return Unsafe.As<double, T>(ref v); };
        else if (type == typeof(decimal)) Read = (r, _, _, _) => { var v = r.ReadDecimal(); return Unsafe.As<decimal, T>(ref v); };
        else if (type == typeof(char)) Read = (r, _, _, _) => { var v = r.ReadChar(); return Unsafe.As<char, T>(ref v); };

        else if (type == typeof(string)) Read = (r, _, _, _) => (T)(object)r.ReadString()!;

        else if (type == typeof(byte[])) Read = (r, arg1, arg2, arg3) =>
        {
            if (arg1 is not byte[] buffer) throw new ArgumentException("Buffer must be a byte array.", nameof(arg1));
            if (arg2 is not int offset) throw new ArgumentException("Offset must be an integer.", nameof(arg2));
            if (arg3 is not int count) throw new ArgumentException("Size must be an integer.", nameof(arg3));

            r.Read(buffer, offset, count);
            return (T)(object)buffer;
        };

        else if (type == typeof(Type)) Read = (r, _, _, _) => (T)(object)Type.GetType(r.ReadString())!;

        else if (type.IsEnum) Read = (r, _, _, _) => EnumReader<T>.Read(r);

        else if (type == typeof(Guid)) Read = (r, _, _, _) => { var v = r.ReadGuid(); return Unsafe.As<Guid, T>(ref v); };
        else if (type == typeof(DateTime)) Read = (r, _, _, _) => { var v = r.ReadDateTime(); return Unsafe.As<DateTime, T>(ref v); };
        else if (type == typeof(TimeSpan)) Read = (r, _, _, _) => { var v = r.ReadTimeSpan(); return Unsafe.As<TimeSpan, T>(ref v); };

        else if (type == typeof(Vector2)) Read = (r, _, _, _) => { var v = r.ReadVector2(); return Unsafe.As<Vector2, T>(ref v); };
        else if (type == typeof(Vector3)) Read = (r, _, _, _) => { var v = r.ReadVector3(); return Unsafe.As<Vector3, T>(ref v); };
        else if (type == typeof(Vector4)) Read = (r, _, _, _) => { var v = r.ReadVector4(); return Unsafe.As<Vector4, T>(ref v); };
        else if (type == typeof(Quaternion)) Read = (r, _, _, _) => { var v = r.ReadQuaternion(); return Unsafe.As<Quaternion, T>(ref v); };
        else if (type == typeof(Color)) Read = (r, _, _, _) => { var v = r.ReadColor(); return Unsafe.As<Color, T>(ref v); };

        else Read = (_, _, _, _) => throw new NotSupportedException($"Type '{type}' is not supported.");
    }
}
