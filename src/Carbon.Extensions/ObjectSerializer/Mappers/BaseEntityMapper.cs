using HizenLabs.Extensions.ObjectSerializer.Mappers.Abstractions;
using HizenLabs.Extensions.ObjectSerializer.Serialization;
using System;

namespace HizenLabs.Extensions.ObjectSerializer.Mappers;

public sealed class BaseEntityMapper : BaseEntityMapper<BaseEntity> { }

public class BaseEntityMapper<TOriginal> : BaseObjectMapper<TOriginal>
    where TOriginal : BaseEntity
{
    public BaseEntityMapper()
    {
    }

    protected override TOriginal CreateInstance(SerializableObject source)
    {
        throw new NotImplementedException();
    }
}
