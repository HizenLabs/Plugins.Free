using HizenLabs.Extensions.ObjectSerializer.Enums;
using HizenLabs.Extensions.ObjectSerializer.Extensions;
using HizenLabs.Extensions.ObjectSerializer.Internal.Delegates;
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
        var typeMarker = type.GetTypeMarker();

        if (typeMarker == TypeMarker.Object) Read = r =>
        {
            var marker = r.ReadEnum<TypeMarker>();

            if (marker == TypeMarker.Null) return default;
            else if (marker == TypeMarker.Boolean) return (T)(object)r.ReadBoolean();
            else if (marker == TypeMarker.SByte) return (T)(object)r.ReadSByte();
            else if (marker == TypeMarker.Byte) return (T)(object)r.ReadByte();
            else if (marker == TypeMarker.Int16) return (T)(object)r.ReadInt16();
            else if (marker == TypeMarker.UInt16) return (T)(object)r.ReadUInt16();
            else if (marker == TypeMarker.Int32) return (T)(object)r.ReadInt32();
            else if (marker == TypeMarker.UInt32) return (T)(object)r.ReadUInt32();
            else if (marker == TypeMarker.Int64) return (T)(object)r.ReadInt64();
            else if (marker == TypeMarker.UInt64) return (T)(object)r.ReadUInt64();
            else if (marker == TypeMarker.Single) return (T)(object)r.ReadSingle();
            else if (marker == TypeMarker.Double) return (T)(object)r.ReadDouble();
            else if (marker == TypeMarker.Decimal) return (T)(object)r.ReadDecimal();
            else if (marker == TypeMarker.Char) return (T)(object)r.ReadChar();

            else if (marker == TypeMarker.String) return (T)(object)r.ReadString()!;
            else if (marker == TypeMarker.Type) return (T)(object)Type.GetType(r.ReadString())!;

            else if (marker == TypeMarker.Enum) return EnumReader<T>.Read(r);

            else if (marker == TypeMarker.Guid) return (T)(object)r.ReadGuid();
            else if (marker == TypeMarker.DateTime) return (T)(object)r.ReadDateTime();
            else if (marker == TypeMarker.TimeSpan) return (T)(object)r.ReadTimeSpan();

            else if (marker == TypeMarker.Vector2) return (T)(object)r.ReadVector2();
            else if (marker == TypeMarker.Vector3) return (T)(object)r.ReadVector3();
            else if (marker == TypeMarker.Vector4) return (T)(object)r.ReadVector4();
            else if (marker == TypeMarker.Quaternion) return (T)(object)r.ReadQuaternion();
            else if (marker == TypeMarker.Color) return (T)(object)r.ReadColor();

            else throw new NotSupportedException($"Type '{marker}' is not supported.");
        };
        else if (typeMarker == TypeMarker.Boolean) Read = r => { var v = r.ReadBoolean(); return Unsafe.As<bool, T>(ref v); };
        else if (typeMarker == TypeMarker.SByte) Read = r => { var v = r.ReadSByte(); return Unsafe.As<sbyte, T>(ref v); };
        else if (typeMarker == TypeMarker.Byte) Read = r => { var v = r.ReadByte(); return Unsafe.As<byte, T>(ref v); };
        else if (typeMarker == TypeMarker.Int16) Read = r => { var v = r.ReadInt16(); return Unsafe.As<short, T>(ref v); };
        else if (typeMarker == TypeMarker.UInt16) Read = r => { var v = r.ReadUInt16(); return Unsafe.As<ushort, T>(ref v); };
        else if (typeMarker == TypeMarker.Int32) Read = r => { var v = r.ReadInt32(); return Unsafe.As<int, T>(ref v); };
        else if (typeMarker == TypeMarker.UInt32) Read = r => { var v = r.ReadUInt32(); return Unsafe.As<uint, T>(ref v); };
        else if (typeMarker == TypeMarker.Int64) Read = r => { var v = r.ReadInt64(); return Unsafe.As<long, T>(ref v); };
        else if (typeMarker == TypeMarker.UInt64) Read = r => { var v = r.ReadUInt64(); return Unsafe.As<ulong, T>(ref v); };
        else if (typeMarker == TypeMarker.Single) Read = r => { var v = r.ReadSingle(); return Unsafe.As<float, T>(ref v); };
        else if (typeMarker == TypeMarker.Double) Read = r => { var v = r.ReadDouble(); return Unsafe.As<double, T>(ref v); };
        else if (typeMarker == TypeMarker.Decimal) Read = r => { var v = r.ReadDecimal(); return Unsafe.As<decimal, T>(ref v); };
        else if (typeMarker == TypeMarker.Char) Read = r => { var v = r.ReadChar(); return Unsafe.As<char, T>(ref v); };

        else if (typeMarker == TypeMarker.String) Read = r => (T)(object)r.ReadString();
        else if (typeMarker == TypeMarker.Type) Read = r => (T)(object)Type.GetType(r.ReadString());

        else if (typeMarker == TypeMarker.Enum) Read = r => EnumReader<T>.Read(r);

        else if (typeMarker == TypeMarker.Guid) Read = r => { var v = r.ReadGuid(); return Unsafe.As<Guid, T>(ref v); };
        else if (typeMarker == TypeMarker.DateTime) Read = r => { var v = r.ReadDateTime(); return Unsafe.As<DateTime, T>(ref v); };
        else if (typeMarker == TypeMarker.TimeSpan) Read = r => { var v = r.ReadTimeSpan(); return Unsafe.As<TimeSpan, T>(ref v); };

        else if (typeMarker == TypeMarker.Vector2) Read = r => { var v = r.ReadVector2(); return Unsafe.As<Vector2, T>(ref v); };
        else if (typeMarker == TypeMarker.Vector3) Read = r => { var v = r.ReadVector3(); return Unsafe.As<Vector3, T>(ref v); };
        else if (typeMarker == TypeMarker.Vector4) Read = r => { var v = r.ReadVector4(); return Unsafe.As<Vector4, T>(ref v); };
        else if (typeMarker == TypeMarker.Quaternion) Read = r => { var v = r.ReadQuaternion(); return Unsafe.As<Quaternion, T>(ref v); };
        else if (typeMarker == TypeMarker.Color) Read = r => { var v = r.ReadColor(); return Unsafe.As<Color, T>(ref v); };

        else if (typeMarker == TypeMarker.List)
        {
            var elementType = type.GetGenericArguments()[0];
            Read = GenericDelegateFactory.BuildGenericMethod<Func<BinaryReader, T>>(
                staticType: typeof(BinaryReaderExtensions),
                methodName: nameof(BinaryReaderExtensions.ReadList),
                elementType);
        }
        else if (typeMarker == TypeMarker.Dictionary)
        {
            var genericArgs = type.GetGenericArguments();
            var keyType = genericArgs[0];
            var valueType = genericArgs[1];
            Read = GenericDelegateFactory.BuildGenericMethod<Func<BinaryReader, T>>(
                staticType: typeof(BinaryReaderExtensions),
                methodName: nameof(BinaryReaderExtensions.ReadDictionary),
                keyType, valueType);
        }

        // Disabling nesting of generic array types because we don't currently have a suitable way to prevent new allocations.
        // With GenericArrayReader<T> we at least directly read into the array, but we don't have the type params for that here (nor do we want to add them yet).
        // In general, we should use List<> anyway.
        else if (typeMarker == TypeMarker.Array) Read = _ => throw new Exception("Generic array mappings are not supported. Please use List<> instead.");

        else Read = _ => throw new NotSupportedException($"Type '{typeMarker}' is not supported.");
    }
}
