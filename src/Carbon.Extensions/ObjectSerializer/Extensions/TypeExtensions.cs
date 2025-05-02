using HizenLabs.Extensions.ObjectSerializer.Enums;
using System;
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

        var handle = type.TypeHandle;
        if (handle.Equals(typeof(object).TypeHandle)) return TypeMarker.Object;

        if (handle.Equals(typeof(bool).TypeHandle)) return TypeMarker.Boolean;
        if (handle.Equals(typeof(sbyte).TypeHandle)) return TypeMarker.SByte;
        if (handle.Equals(typeof(byte).TypeHandle)) return TypeMarker.Byte;
        if (handle.Equals(typeof(short).TypeHandle)) return TypeMarker.Int16;
        if (handle.Equals(typeof(ushort).TypeHandle)) return TypeMarker.UInt16;
        if (handle.Equals(typeof(int).TypeHandle)) return TypeMarker.Int32;
        if (handle.Equals(typeof(uint).TypeHandle)) return TypeMarker.UInt32;
        if (handle.Equals(typeof(long).TypeHandle)) return TypeMarker.Int64;
        if (handle.Equals(typeof(ulong).TypeHandle)) return TypeMarker.UInt64;
        if (handle.Equals(typeof(float).TypeHandle)) return TypeMarker.Single;
        if (handle.Equals(typeof(double).TypeHandle)) return TypeMarker.Double;
        if (handle.Equals(typeof(decimal).TypeHandle)) return TypeMarker.Decimal;
        if (handle.Equals(typeof(char).TypeHandle)) return TypeMarker.Char;
        if (handle.Equals(typeof(string).TypeHandle)) return TypeMarker.String;
        if (handle.Equals(typeof(byte[]).TypeHandle)) return TypeMarker.ByteArray;

        if (type.IsEnum) return TypeMarker.Enum;
        if (handle.Equals(typeof(Guid).TypeHandle)) return TypeMarker.Guid;
        if (handle.Equals(typeof(DateTime).TypeHandle)) return TypeMarker.DateTime;
        if (handle.Equals(typeof(TimeSpan).TypeHandle)) return TypeMarker.TimeSpan;
        if (handle.Equals(typeof(Type).TypeHandle)) return TypeMarker.Type;

        if (handle.Equals(typeof(Vector2).TypeHandle)) return TypeMarker.Vector2;
        if (handle.Equals(typeof(Vector3).TypeHandle)) return TypeMarker.Vector3;
        if (handle.Equals(typeof(Vector4).TypeHandle)) return TypeMarker.Vector4;
        if (handle.Equals(typeof(Quaternion).TypeHandle)) return TypeMarker.Quaternion;
        if (handle.Equals(typeof(Color).TypeHandle)) return TypeMarker.Color;

        if (type.IsArray) return TypeMarker.Array;
        if (typeof(System.Collections.IList).IsAssignableFrom(type)) return TypeMarker.List;
        if (typeof(System.Collections.IDictionary).IsAssignableFrom(type)) return TypeMarker.Dictionary;

        throw new ArgumentException($"Unsupported type: {type.FullName}", nameof(type));
    }
}
