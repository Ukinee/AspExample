using NUnit.Framework;
using Ukinee.Infrastructure.Ddd.Common.LocalCache.Contracts;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Announcements;
using Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Client.Extensions;

namespace Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Client.Cache;

public class CacheTests : ClientTestBase
{
    [Test]
    public async Task Lookup_WhenNeverCached_ReturnsUnknown()
    {
        var cache = GetService<IEntityCache<AnnouncementIdentifier, Announcement>>();
        var id = AnnouncementIdentifier.Create(Guid.NewGuid(), Guid.NewGuid());

        var result = cache.Lookup(id);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsUnknown, Is.True);
            Assert.That(result.Entity, Is.Null);
        }
    }

    [Test]
    public async Task Upsert_ThenLookup_ReturnsHit()
    {
        var cache = GetService<IEntityCache<AnnouncementIdentifier, Announcement>>();
        var entity = CreateEntity();

        cache.Upsert(entity);
        var result = cache.Lookup(entity.Identifier);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsHit, Is.True);
            Assert.That(result.Entity, Is.EqualTo(entity));
        }
    }

    [Test]
    public async Task MarkNotExists_ThenLookup_ReturnsKnownMissing()
    {
        var cache = GetService<IEntityCache<AnnouncementIdentifier, Announcement>>();
        var id = AnnouncementIdentifier.Create(Guid.NewGuid(), Guid.NewGuid());

        cache.MarkNotExists(id);
        var result = cache.Lookup(id);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsKnownMissing, Is.True);
            Assert.That(result.Entity, Is.Null);
        }
    }

    [Test]
    public async Task Invalidate_AfterUpsert_ReturnsUnknown()
    {
        var cache = GetService<IEntityCache<AnnouncementIdentifier, Announcement>>();
        var entity = CreateEntity();

        cache.Upsert(entity);
        cache.Invalidate(entity.Identifier);
        var result = cache.Lookup(entity.Identifier);

        Assert.That(result.IsUnknown, Is.True);
    }

    [Test]
    public async Task Upsert_OverridesKnownMissing()
    {
        var cache = GetService<IEntityCache<AnnouncementIdentifier, Announcement>>();
        var entity = CreateEntity();

        cache.MarkNotExists(entity.Identifier);
        cache.Upsert(entity);
        var result = cache.Lookup(entity.Identifier);

        Assert.That(result.IsHit, Is.True, "Entity now exists; Upsert must beat the negative cache.");
    }

    [Test]
    public async Task MarkNotExists_OverridesUpsert()
    {
        var cache = GetService<IEntityCache<AnnouncementIdentifier, Announcement>>();
        var entity = CreateEntity();

        cache.Upsert(entity);
        cache.MarkNotExists(entity.Identifier);
        var result = cache.Lookup(entity.Identifier);

        Assert.That(result.IsKnownMissing, Is.True, "Fresh knowledge that entity is missing wins over the stale cache entry.");
    }

    private Announcement CreateEntity()
    {
        return ExampleAnnouncementFactory.Example1;
    }
}
