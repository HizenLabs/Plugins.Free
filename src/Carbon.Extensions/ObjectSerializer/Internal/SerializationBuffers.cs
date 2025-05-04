using Facepunch;

namespace HizenLabs.Extensions.ObjectSerializer.Internal;

internal static class SerializationBuffers
{
    public static ArrayPool<byte> Guid { get; } = new(16);
}
