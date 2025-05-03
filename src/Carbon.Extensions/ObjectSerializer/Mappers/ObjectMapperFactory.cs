using HizenLabs.Extensions.ObjectSerializer.Mappers.Abstractions;
using System;
using System.Collections.Generic;

namespace HizenLabs.Extensions.ObjectSerializer.Mappers;

internal class ObjectMapperFactory
{
    /// <summary>
    /// A dictionary to cache the mappers for different types.
    /// </summary>
    private static readonly Dictionary<Type, IObjectMapper> _mappers = new();

    /// <summary>
    /// Gets the appropriate mapper for the specified type.
    /// </summary>
    /// <param name="type">The type for which to get the mapper.</param>
    /// <returns>The mapper for the specified type.</returns>
    public static IObjectMapper GetMapper(Type type)
    {
        if (!_mappers.TryGetValue(type, out var mapper))
        {
            mapper = CreateMapper(type);
            _mappers[type] = mapper;
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
        if (type.IsAssignableFrom(typeof(BaseEntity)))
        {
            return new BaseEntityMapper();
        }
        else if (type == typeof(Item))
        {
            return new ItemMapper();
        }
        
        throw new NotSupportedException($"The type {type} is not supported for serialization.");
    }
}
