using System.Buffers;

using MessagePack;

using Microsoft.Extensions.Caching.Hybrid;

namespace Cnblogs.Cache.Hybrid.Extensions;

public class HybridCacheMessagePackSerializer<T>
    (MessagePackSerializerOptions? options) : IHybridCacheSerializer<T>
{
    public T Deserialize(ReadOnlySequence<byte> source)
    {
        return MessagePackSerializer.Deserialize<T>(source, options);
    }

    public void Serialize(T value, IBufferWriter<byte> target)
    {
        MessagePackSerializer.Serialize(target, value, options);
    }
}
