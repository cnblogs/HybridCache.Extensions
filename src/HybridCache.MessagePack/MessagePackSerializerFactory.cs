using System.Diagnostics.CodeAnalysis;

using MessagePack;
using MessagePack.Resolvers;

using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;

namespace Cnblogs.Cache.Hybrid.Extensions;

public class MessagePackSerializerFactory(
    IOptions<HybridCacheMessagePackSerializerOptions> options) : IHybridCacheSerializerFactory
{
    private readonly HybridCacheMessagePackSerializerOptions _options = options.Value;

    public bool TryCreateSerializer<T>([NotNullWhen(true)] out IHybridCacheSerializer<T>? serializer)
    {
        _options.SerializerOptions ??= CreateDefaultOptions();
        serializer = new HybridCacheMessagePackSerializer<T>(_options.SerializerOptions);
        return true;
    }

    public static MessagePackSerializerOptions CreateDefaultOptions()
    {
        var resolver = CompositeResolver.Create(
            NativeDateTimeResolver.Instance,
            NativeGuidResolver.Instance,
            NativeDecimalResolver.Instance,
            ContractlessStandardResolver.Instance);
        return MessagePackSerializerOptions.Standard.WithResolver(resolver);
    }
}