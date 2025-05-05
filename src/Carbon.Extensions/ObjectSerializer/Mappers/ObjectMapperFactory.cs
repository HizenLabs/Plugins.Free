using HizenLabs.Extensions.ObjectSerializer.Enums;
using HizenLabs.Extensions.ObjectSerializer.Extensions;
using HizenLabs.Extensions.ObjectSerializer.Mappers.Abstractions;
using System;
using System.Collections.Generic;

namespace HizenLabs.Extensions.ObjectSerializer.Mappers;

internal static class ObjectMapperFactory
{
    /// <summary>
    /// A dictionary to cache the mappers for different types.
    /// </summary>
    /// <remarks>
    /// This is to prevent the order from changing and also modifiying the dictionary during creation.
    /// </remarks>
    private static readonly Dictionary<ObjectType, IObjectMapper> _mapperCache = new()
    {
        { ObjectType.Item, new ItemMapper() },
        { ObjectType.BaseEntity, new BaseEntityMapper() }
    };

    /// Gets the appropriate mapper for the specified object type.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <returns>The mapper for the specified type.</returns>
    public static IObjectMapper GetMapper<T>()
    {
        var objectType = typeof(T).GetObjectType();
        return GetMapperByObjectType(objectType);
    }

    /// <summary>
    /// Gets the appropriate mapper for the specified object type.
    /// </summary>
    /// <param name="objectType">The object type to get a mapper for.</param>
    /// <returns>The mapper for the specified object type.</returns>
    public static IObjectMapper GetMapperByObjectType(ObjectType objectType)
    {
        if (_mapperCache.TryGetValue(objectType, out var mapper))
        {
            return mapper;
        }

        throw new NotSupportedException($"No mapper found for object type {objectType}");
    }
}
