using Facepunch;

namespace HizenLabs.Extensions.ObjectSerializer.Internal;

internal static class SerializationBuffers
{
    public static ArrayPool<byte> GuidPool { get; } = new(16);

    public static ArrayPool<byte> StreamPool { get; } = new(StreamBufferSize);
    public const int StreamBufferSize = 8192;
}
