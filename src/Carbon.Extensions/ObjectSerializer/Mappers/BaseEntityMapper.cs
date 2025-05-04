using HizenLabs.Extensions.ObjectSerializer.Mappers.Abstractions;

namespace HizenLabs.Extensions.ObjectSerializer.Mappers;

public sealed class BaseEntityMapper : BaseEntityMapper<BaseEntity> { }

public class BaseEntityMapper<TOriginal> : BaseObjectMapper<TOriginal>
    where TOriginal : BaseEntity
{
    public BaseEntityMapper()
    {

    }
}
