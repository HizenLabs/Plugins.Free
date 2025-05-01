using HizenLabs.Extensions.ObjectSerializer.Enums;
using System;
using UnityEngine;

namespace HizenLabs.Extensions.ObjectSerializer.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="TypeMarker"/> enum.
/// </summary>
public static class TypeMarkerExtensions
{
    /// <summary>
    /// Gets the <see cref="Type"/> represented by the specified <see cref="TypeMarker"/>.
    /// </summary>
    /// <param name="marker">The <see cref="TypeMarker"/> to get the type for.</param>
    /// <returns>The corresponding <see cref="Type"/>.</returns>
    public static Type GetSystemType(this TypeMarker marker)
    {
        return marker switch
        {
            TypeMarker.Null => null,
            TypeMarker.Object => typeof(object),

            TypeMarker.Boolean => typeof(bool),
            TypeMarker.SByte => typeof(sbyte),
            TypeMarker.Byte => typeof(byte),
            TypeMarker.Int16 => typeof(short),
            TypeMarker.UInt16 => typeof(ushort),
            TypeMarker.Int32 => typeof(int),
            TypeMarker.UInt32 => typeof(uint),
            TypeMarker.Int64 => typeof(long),
            TypeMarker.UInt64 => typeof(ulong),

            TypeMarker.Single => typeof(float),
            TypeMarker.Double => typeof(double),
            TypeMarker.Decimal => typeof(decimal),
            TypeMarker.Char => typeof(char),
            TypeMarker.String => typeof(string),
            TypeMarker.ByteArray => typeof(byte[]),

            TypeMarker.Enum => typeof(Enum), // fallback, as enum types require more context
            TypeMarker.Guid => typeof(Guid),
            TypeMarker.DateTime => typeof(DateTime),
            TypeMarker.TimeSpan => typeof(TimeSpan),
            TypeMarker.Type => typeof(Type),

            TypeMarker.Vector2 => typeof(Vector2),
            TypeMarker.Vector3 => typeof(Vector3),
            TypeMarker.Vector4 => typeof(Vector4),
            TypeMarker.Quaternion => typeof(Quaternion),
            TypeMarker.Color => typeof(Color),

            _ => throw new ArgumentException($"Unsupported TypeMarker: {marker}", nameof(marker))
        };
    }
}
