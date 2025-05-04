using HizenLabs.Extensions.ObjectSerializer.Enums;
using HizenLabs.Extensions.ObjectSerializer.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HizenLabs.Extensions.ObjectSerializer.Internal.TypeMarkerResolvers;

internal class DirectTypeMarkerResolver : ITypeMarkerResolver
{
    private static readonly Dictionary<Type, TypeMarker> _cache = new()
    {
        { typeof(object), TypeMarker.Object },
        { typeof(bool), TypeMarker.Boolean },
        { typeof(sbyte), TypeMarker.SByte },
        { typeof(byte), TypeMarker.Byte },
        { typeof(short), TypeMarker.Int16 },
        { typeof(ushort), TypeMarker.UInt16 },
        { typeof(int), TypeMarker.Int32 },
        { typeof(uint), TypeMarker.UInt32 },
        { typeof(long), TypeMarker.Int64 },
        { typeof(ulong), TypeMarker.UInt64 },
        { typeof(float), TypeMarker.Single },
        { typeof(double), TypeMarker.Double },
        { typeof(decimal), TypeMarker.Decimal },
        { typeof(char), TypeMarker.Char },
        { typeof(string), TypeMarker.String },
        { typeof(byte[]), TypeMarker.ByteArray },
        { typeof(Guid), TypeMarker.Guid },
        { typeof(DateTime), TypeMarker.DateTime },
        { typeof(TimeSpan), TypeMarker.TimeSpan },
        { typeof(Type), TypeMarker.Type },
        { typeof(Vector2), TypeMarker.Vector2 },
        { typeof(Vector3), TypeMarker.Vector3 },
        { typeof(Vector4), TypeMarker.Vector4 },
        { typeof(Quaternion), TypeMarker.Quaternion },
        { typeof(Color), TypeMarker.Color },
        { typeof(SerializableObject), TypeMarker.SerializableObject }
    };

    public bool TryResolve(Type type, out TypeMarker marker) => _cache.TryGetValue(type, out marker);
}
