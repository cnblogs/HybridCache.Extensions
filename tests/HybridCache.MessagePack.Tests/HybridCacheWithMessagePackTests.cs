using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;

using StackExchange.Redis;

namespace Cnblogs.Cache.Hybrid.Extensions.Tests;

public class HybridCacheWithMessagePackTests
{
    [Test]
    public async Task HybridCache_GetOrCreateAsync_ShouldSucceed()
    {
        // arrange
        var builder = WebApplication.CreateBuilder();

        const string redisConnectString = "redis:6379";
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectString;
        });

        builder.Services.AddHybridCache()
            .AddSerializerFactory<MessagePackSerializerFactory>();

        // act
        using var app = builder.Build();
        var hybridCache = app.Services.GetRequiredService<HybridCache>();

        var cacheKey = $"blogposts-list-{DateTime.Now}";
        var entryOptions = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(1),
            Flags = HybridCacheEntryFlags.DisableLocalCache
        };

        var blogPosts = await hybridCache.GetOrCreateAsync(
            cacheKey,
            async token => await GetBlogPostsAsync(token),
            entryOptions);

        // Assert
        var expected = await GetBlogPostsAsync();
        await Assert.That(blogPosts).IsNotEmpty();
        await Assert.That(blogPosts)
            .IsEquivalentTo(expected)
            .Using((p1, p2) => p1!.Id == p2!.Id && p1.Title == p2.Title);

        ConnectionMultiplexer redis = await ConnectionMultiplexer.ConnectAsync(redisConnectString);
        await Assert.That(redis).IsNotNull();

        IDatabase db = redis.GetDatabase();
        await Assert.That(db.KeyExists(cacheKey)).IsTrue();

        blogPosts = await hybridCache.GetOrCreateAsync(
            cacheKey,
            async token => await Task.FromResult<List<BlogPost>>([]),
            entryOptions);

        await Assert.That(blogPosts)
            .IsEquivalentTo(expected)
            .Using((p1, p2) => p1!.Id == p2!.Id && p1.Title == p2.Title);

        await hybridCache.RemoveAsync(cacheKey);
    }

    private static Task<List<BlogPost>> GetBlogPostsAsync(CancellationToken? cancellationToken = null)
    {
        return Task.FromResult<List<BlogPost>>([new BlogPost(1, "title1"), new BlogPost(2, "title2")]);
    }

    public record BlogPost(int Id, string Title);
}
