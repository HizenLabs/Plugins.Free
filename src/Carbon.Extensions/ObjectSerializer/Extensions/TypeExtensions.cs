using HizenLabs.Extensions.ObjectSerializer.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HizenLabs.Extensions.ObjectSerializer.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Type"/> class.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Gets the <see cref="TypeMarker"/> for the specified <see cref="Type"/>.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to get the marker for.</param>
    /// <returns>The <see cref="TypeMarker"/> for the specified <see cref="Type"/>.</returns>
    public static TypeMarker GetTypeMarker(this Type type)
    {
        if (type is null) return TypeMarker.Null;

        if (type == typeof(object)) return TypeMarker.Object;

        if (type == typeof(bool)) return TypeMarker.Boolean;
        if (type == typeof(sbyte)) return TypeMarker.SByte;
        if (type == typeof(byte)) return TypeMarker.Byte;
        if (type == typeof(short)) return TypeMarker.Int16;
        if (type == typeof(ushort)) return TypeMarker.UInt16;
        if (type == typeof(int)) return TypeMarker.Int32;
        if (type == typeof(uint)) return TypeMarker.UInt32;
        if (type == typeof(long)) return TypeMarker.Int64;
        if (type == typeof(ulong)) return TypeMarker.UInt64;
        if (type == typeof(float)) return TypeMarker.Single;
        if (type == typeof(double)) return TypeMarker.Double;
        if (type == typeof(decimal)) return TypeMarker.Decimal;
        if (type == typeof(char)) return TypeMarker.Char;
        if (type == typeof(string)) return TypeMarker.String;
        if (type == typeof(byte[])) return TypeMarker.ByteArray;

        if (type.IsEnum) return TypeMarker.Enum;
        if (type == typeof(Guid)) return TypeMarker.Guid;
        if (type == typeof(DateTime)) return TypeMarker.DateTime;
        if (type == typeof(TimeSpan)) return TypeMarker.TimeSpan;
        if (type == typeof(Type)) return TypeMarker.Type;

        if (type == typeof(Vector2)) return TypeMarker.Vector2;
        if (type == typeof(Vector3)) return TypeMarker.Vector3;
        if (type == typeof(Vector4)) return TypeMarker.Vector4;
        if (type == typeof(Quaternion)) return TypeMarker.Quaternion;
        if (type == typeof(Color)) return TypeMarker.Color;

        if (type.IsArray) return TypeMarker.Array;

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) return TypeMarker.List;
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>)) return TypeMarker.Dictionary;

        // I don't understand, but there is a System.RuntimeType as well as System.Type, and this
        // is basically the only clean way to test for it since it's a hidden type (so I can't use typeof)
        if (type.FullName == "System.RuntimeType") return TypeMarker.Type;

        throw new ArgumentException($"Unsupported type: {type.FullName}", nameof(type));
    }
}