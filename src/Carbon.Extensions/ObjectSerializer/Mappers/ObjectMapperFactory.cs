using HizenLabs.Extensions.ObjectSerializer.Mappers.Abstractions;
using System;
using System.Collections.Generic;

namespace HizenLabs.Extensions.ObjectSerializer.Mappers;

internal class ObjectMapperFactory
{
    /// <summary>
    /// A dictionary to cache the mappers for different types.
    /// </summary>
    /// <remarks>
    /// This is to prevent the order from changing and also modifiying the dictionary during creation.
    /// </remarks>
    private static readonly Dictionary<Type, IObjectMapper> _mapperCache = new();

    /// <summary>
    /// A list to register mappers for specific types, ordered from most specific to least specific.
    /// </summary>
    private static readonly List<(Type Type, IObjectMapper Mapper)> _registeredMappers = new()
    {
        (typeof(Item), new ItemMapper()),
        (typeof(BaseEntity), new BaseEntityMapper())
    };

    /// <summary>
    /// Gets the appropriate mapper for the specified type.
    /// </summary>
    /// <typeparam name="T">The type for which to get the mapper.</typeparam>
    /// <returns>The mapper for the specified type.</returns>
    public static IObjectMapper GetMapper<T>()
    {
        var type = typeof(T);
        if (!_mapperCache.TryGetValue(type, out var mapper))
        {
            mapper = CreateMapper(type);
            _mapperCache[type] = mapper;
        }

        return mapper;
    }

    /// <summary>
    /// Creates a new mapper for the specified type.
    /// </summary>
    /// <param name="type">The type for which to create the mapper.</param>
    /// <returns>The newly created mapper.</returns>
    private static IObjectMapper CreateMapper(Type type)
    {
        foreach (var (mappingType, mapper) in _registeredMappers)
        {
            if (mappingType.IsAssignableFrom(type))
            {
                return mapper;
            }
        }

        throw new NotSupportedException($"The type {type} is not supported for serialization.");
    }
}
