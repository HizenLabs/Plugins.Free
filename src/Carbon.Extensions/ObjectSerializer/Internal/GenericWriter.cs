using HizenLabs.Extensions.ObjectSerializer.Enums;
using HizenLabs.Extensions.ObjectSerializer.Extensions;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace HizenLabs.Extensions.ObjectSerializer.Internal;

/// <summary>
/// Provides methods for writing generic instances of <typeparamref name="T"/> to a <see cref="BinaryWriter"/>.
/// </summary>
/// typeparam name="T">The type of the object to write.</typeparam>
internal class GenericWriter<T>
{
    /// <summary>
    /// The delegate that writes the <typeparamref name="T"/> to the <see cref="BinaryWriter"/>.
    /// </summary>
    public static Action<BinaryWriter, T> Write { get; }

    /// <summary>
    /// Initializes the <see cref="GenericWriter{T}"/> class.
    /// </summary>
    /// <exception cref="NotSupportedException">Thrown when the type is not supported.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the type is object.</exception>
    static GenericWriter()
    {
        var type = typeof(T);
        var typeMarker = type.GetTypeMarker();

        if (typeMarker == TypeMarker.Object) Write = (w, v) =>
        {
            if (v is null) w.Write(TypeMarker.Null);
            else
            {
                var objType = v.GetType().GetTypeMarker();
                w.Write(objType);

                if (objType == TypeMarker.Boolean && v is bool valueBool) w.Write(valueBool);
                else if (objType == TypeMarker.SByte && v is sbyte valueSByte) w.Write(valueSByte);
                else if (objType == TypeMarker.Byte && v is byte valueByte) w.Write(valueByte);
                else if (objType == TypeMarker.Int16 && v is short valueShort) w.Write(valueShort);
                else if (objType == TypeMarker.UInt16 && v is ushort valueUShort) w.Write(valueUShort);
                else if (objType == TypeMarker.Int32 && v is int valueInt) w.Write(valueInt);
                else if (objType == TypeMarker.UInt32 && v is uint valueUInt) w.Write(valueUInt);
                else if (objType == TypeMarker.Int64 && v is long valueLong) w.Write(valueLong);
                else if (objType == TypeMarker.UInt64 && v is ulong valueULong) w.Write(valueULong);
                else if (objType == TypeMarker.Single && v is float valueFloat) w.Write(valueFloat);
                else if (objType == TypeMarker.Double && v is double valueDouble) w.Write(valueDouble);
                else if (objType == TypeMarker.Decimal && v is decimal valueDecimal) w.Write(valueDecimal);
                else if (objType == TypeMarker.Char && v is char valueChar) w.Write(valueChar);

                else if (objType == TypeMarker.String && v is string valueString) w.Write(valueString);
                else if (objType == TypeMarker.Type && v is Type valueType) w.Write(valueType);

                else if (objType == TypeMarker.Enum) EnumWriter<T>.Write(w, v);

                else if (objType == TypeMarker.Guid && v is Guid valueGuid) w.Write(valueGuid);
                else if (objType == TypeMarker.DateTime && v is DateTime valueDateTime) w.Write(valueDateTime);
                else if (objType == TypeMarker.TimeSpan && v is TimeSpan valueTimeSpan) w.Write(valueTimeSpan);

                else if (objType == TypeMarker.Vector2 && v is Vector2 valueVector2) w.Write(valueVector2);
                else if (objType == TypeMarker.Vector3 && v is Vector3 valueVector3) w.Write(valueVector3);
                else if (objType == TypeMarker.Vector4 && v is Vector4 valueVector4) w.Write(valueVector4);
                else if (objType == TypeMarker.Quaternion && v is Quaternion valueQuaternion) w.Write(valueQuaternion);
                else if (objType == TypeMarker.Color && v is Color valueColor) w.Write(valueColor);

                else throw new NotSupportedException($"Type {objType} is not supported.");
            }
        };
        else if (typeMarker == TypeMarker.Boolean) Write = (w, v) => w.Write(Unsafe.As<T, bool>(ref v));
        else if (typeMarker == TypeMarker.SByte) Write = (w, v) => w.Write(Unsafe.As<T, sbyte>(ref v));
        else if (typeMarker == TypeMarker.Byte) Write = (w, v) => w.Write(Unsafe.As<T, byte>(ref v));
        else if (typeMarker == TypeMarker.Int16) Write = (w, v) => w.Write(Unsafe.As<T, short>(ref v));
        else if (typeMarker == TypeMarker.UInt16) Write = (w, v) => w.Write(Unsafe.As<T, ushort>(ref v));
        else if (typeMarker == TypeMarker.Int32) Write = (w, v) => w.Write(Unsafe.As<T, int>(ref v));
        else if (typeMarker == TypeMarker.UInt32) Write = (w, v) => w.Write(Unsafe.As<T, uint>(ref v));
        else if (typeMarker == TypeMarker.Int64) Write = (w, v) => w.Write(Unsafe.As<T, long>(ref v));
        else if (typeMarker == TypeMarker.UInt64) Write = (w, v) => w.Write(Unsafe.As<T, ulong>(ref v));
        else if (typeMarker == TypeMarker.Single) Write = (w, v) => w.Write(Unsafe.As<T, float>(ref v));
        else if (typeMarker == TypeMarker.Double) Write = (w, v) => w.Write(Unsafe.As<T, double>(ref v));
        else if (typeMarker == TypeMarker.Decimal) Write = (w, v) => w.Write(Unsafe.As<T, decimal>(ref v));
        else if (typeMarker == TypeMarker.Char) Write = (w, v) => w.Write(Unsafe.As<T, char>(ref v));

        else if (typeMarker == TypeMarker.String) Write = (w, v) => w.Write((string)(object)v!);
        else if (typeMarker == TypeMarker.Type) Write = (w, v) => w.Write((Type)(object)v!);

        else if (typeMarker == TypeMarker.Enum) Write = (w, v) => EnumWriter<T>.Write(w, v);

        else if (typeMarker == TypeMarker.Guid) Write = (w, v) => w.Write(Unsafe.As<T, Guid>(ref v));
        else if (typeMarker == TypeMarker.DateTime) Write = (w, v) => w.Write(Unsafe.As<T, DateTime>(ref v));
        else if (typeMarker == TypeMarker.TimeSpan) Write = (w, v) => w.Write(Unsafe.As<T, TimeSpan>(ref v));

        else if (typeMarker == TypeMarker.Vector2) Write = (w, v) => w.Write(Unsafe.As<T, Vector2>(ref v));
        else if (typeMarker == TypeMarker.Vector3) Write = (w, v) => w.Write(Unsafe.As<T, Vector3>(ref v));
        else if (typeMarker == TypeMarker.Vector4) Write = (w, v) => w.Write(Unsafe.As<T, Vector4>(ref v));
        else if (typeMarker == TypeMarker.Quaternion) Write = (w, v) => w.Write(Unsafe.As<T, Quaternion>(ref v));
        else if (typeMarker == TypeMarker.Color) Write = (w, v) => w.Write(Unsafe.As<T, Color>(ref v));

        else if (typeMarker == TypeMarker.List) Write = (w, v) =>
        {
            var elementType = type.GetGenericArguments()[0].GetTypeMarker();
            throw new NotImplementedException();

        };
        else if (typeMarker == TypeMarker.Dictionary) Write = (w, v) =>
        {
            throw new NotImplementedException();
        };

        // Disabling nesting of generic array types because we don't currently have a suitable way to prevent new allocations during read.
        // We _could_ just enable them during write, but then we don't have a matching generic reader for them and it would cause headaches.
        // With GenericArrayReader<T> we at least directly read into the array, but we don't have the type params for that here (nor do we want to add them yet).
        // In general, we should use List<> anyway.
        else if (typeMarker == TypeMarker.Array) Write = (_, _) => throw new Exception("Generic array mappings are not supported. Please use List<> instead.");

        else Write = (_, _) => throw new NotSupportedException($"Type {typeof(T)} is not supported.");
    }
}