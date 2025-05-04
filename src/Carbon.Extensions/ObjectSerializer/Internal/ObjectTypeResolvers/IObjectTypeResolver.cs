using HizenLabs.Extensions.ObjectSerializer.Enums;
using System;

namespace HizenLabs.Extensions.ObjectSerializer.Internal.ObjectTypeResolvers;

internal interface IObjectTypeResolver
{
    bool TryResolve(Type type, out ObjectType objType);
}
