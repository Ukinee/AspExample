using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Ukinee.Infrastructure.Ddd.Common.Exceptions;
using Ukinee.Infrastructure.Ddd.Tests.Common.Services.Repositories;
using Ukinee.Infrastructure.Ddd.Tests.Common.Utils.TestBases;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Announcements;

namespace Ukinee.Infrastructure.Ddd.Tests.Features.Repositories;

public class InMemoryStoreTests : AnnouncementTestBase
{
    protected override void ConfigureTestServices(IServiceCollection services)
    {
        services.AddSingleton<InMemoryStore<AnnouncementIdentifier, Announcement>>();
    }

#region Positive
    [Test]
    public void AddExists_ShouldReturnFalseBeforeAndTrueAfter()
    {
        var store = GetService<InMemoryStore<AnnouncementIdentifier, Announcement>>();
        var entity = ExampleAnnouncementFactory.Example1;

        var existsBefore = store.Exists(entity.Identifier);
        store.AddRange([entity]);
        var existsAfter = store.Exists(entity.Identifier);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(existsBefore, Is.False);
            Assert.That(existsAfter, Is.True);
        }
    }

    [Test]
    public void RemoveExists_ShouldRemoveEntity_ExistsReturnsTrueBeforeAndFalseAfter()
    {
        var store = GetService<InMemoryStore<AnnouncementIdentifier, Announcement>>();
        var entity = ExampleAnnouncementFactory.Example1;
        store.AddRange([entity]);

        var existsBefore = store.Exists(entity.Identifier);
        store.RemoveRange([entity.Identifier], _ => true);
        var existsAfter = store.Exists(entity.Identifier);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(existsBefore, Is.True);
            Assert.That(existsAfter, Is.False);
        }
    }

    [Test]
    public void That_Update_UpdatesEntityInStore()
    {
        var store = GetService<InMemoryStore<AnnouncementIdentifier, Announcement>>();
        var originalEntity = ExampleAnnouncementFactory.Example1;
        store.AddRange([originalEntity]);

        var targetValue = !originalEntity.IsAvailableForPublicRead;

        var result = store.UpdateById(
            originalEntity.Identifier,
            _ => true,
            old => old with {
                IsAvailableForPublicRead = targetValue,
            }
        );

        var currentValue = store.FindById(originalEntity.Identifier, _ => true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Old, Is.Not.Null);

            Assert.That(result.Updated, Is.EqualTo(currentValue));
            Assert.That(result.Old, Is.EqualTo(originalEntity));

            Assert.That(result.Updated.IsAvailableForPublicRead, Is.EqualTo(targetValue));
            Assert.That(result.Old.IsAvailableForPublicRead, Is.EqualTo(originalEntity.IsAvailableForPublicRead));
        }
    }
#endregion

    [Test]
    public void UpdateById_WhenEntityDoesNotExist_ThrowsEntityNotFound()
    {
        var store = GetService<InMemoryStore<AnnouncementIdentifier, Announcement>>();
        var id = ExampleAnnouncementFactory.Example1.Identifier;

        Assert.Throws<EntityNotFoundException<AnnouncementIdentifier, Announcement>>(() => store.UpdateById(id, _ => true, e => e));
    }

    [Test]
    public void UpdateById_WhenFilterReturnsFalse_ThrowsEntityNotFound()
    {
        var store = GetService<InMemoryStore<AnnouncementIdentifier, Announcement>>();
        var entity = ExampleAnnouncementFactory.Example1;
        store.AddRange([entity]);

        Assert.Throws<EntityNotFoundException<AnnouncementIdentifier, Announcement>>(() => store.UpdateById(entity.Identifier, _ => false, e => e));
    }

#region UpdateManyById
    [Test]
    public void UpdateManyById_WhenAnyEntityDoesNotExist_ThrowsEntityNotFound()
    {
        var store = GetService<InMemoryStore<AnnouncementIdentifier, Announcement>>();
        var id = ExampleAnnouncementFactory.Example1.Identifier;

        Assert.Throws<EntityNotFoundException<AnnouncementIdentifier, Announcement>>(() => store.UpdateManyById([id], _ => true, (_, e) => e));
    }

    [Test]
    public void UpdateManyById_WhenFilterReturnsFalse_ThrowsEntityNotFound()
    {
        var store = GetService<InMemoryStore<AnnouncementIdentifier, Announcement>>();
        var entity = ExampleAnnouncementFactory.Example1;
        store.AddRange([entity]);

        Assert.Throws<EntityNotFoundException<AnnouncementIdentifier, Announcement>>(() => store.UpdateManyById([entity.Identifier], _ => false, (_, e) => e));
    }

    [Test]
    public void UpdateManyById_WhenAllValid_UpdatesAndReturnsResults()
    {
        var store = GetService<InMemoryStore<AnnouncementIdentifier, Announcement>>();
        var entity = ExampleAnnouncementFactory.Example1;
        store.AddRange([entity]);

        var targetValue = !entity.IsAvailableForPublicRead;

        var results = store.UpdateManyById(
            [entity.Identifier],
            _ => true,
            (_, old) => old with { IsAvailableForPublicRead = targetValue }
        );

        var fromStore = store.FindById(entity.Identifier, _ => true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(results[0].Old, Is.EqualTo(entity));
            Assert.That(results[0].Updated, Is.EqualTo(fromStore));
            Assert.That(results[0].Updated.IsAvailableForPublicRead, Is.EqualTo(targetValue));
        }
    }
#endregion

#region AddRange
    [Test]
    public void AddRange_WhenEntityAlreadyExists_ThrowsEntityAlreadyExists()
    {
        var store = GetService<InMemoryStore<AnnouncementIdentifier, Announcement>>();
        var entity = ExampleAnnouncementFactory.Example1;
        store.AddRange([entity]);

        Assert.Throws<EntityAlreadyExistsException<AnnouncementIdentifier, Announcement>>(() => store.AddRange([entity]));
    }
#endregion

#region RemoveRange
    [Test]
    public void RemoveRange_WhenEntityDoesNotExist_ThrowsEntityNotFound()
    {
        var store = GetService<InMemoryStore<AnnouncementIdentifier, Announcement>>();
        var id = ExampleAnnouncementFactory.Example1.Identifier;

        Assert.Throws<EntityNotFoundException<AnnouncementIdentifier, Announcement>>(() => store.RemoveRange([id], _ => true));
    }

    [Test]
    public void RemoveRange_WhenFilterReturnsFalse_ThrowsEntityNotFound()
    {
        var store = GetService<InMemoryStore<AnnouncementIdentifier, Announcement>>();
        var entity = ExampleAnnouncementFactory.Example1;
        store.AddRange([entity]);

        Assert.Throws<EntityNotFoundException<AnnouncementIdentifier, Announcement>>(() => store.RemoveRange([entity.Identifier], _ => false));
    }
#endregion

#region Upsert
    [Test]
    public void Upsert_WhenEntityDoesNotExist_AddsEntity()
    {
        var store = GetService<InMemoryStore<AnnouncementIdentifier, Announcement>>();
        var entity = ExampleAnnouncementFactory.Example1;

        store.Upsert([entity]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(store.Exists(entity.Identifier), Is.True);
            Assert.That(store.FindById(entity.Identifier, _ => true), Is.EqualTo(entity));
        }
    }

    [Test]
    public void Upsert_WhenEntityExists_UpdatesEntity()
    {
        var store = GetService<InMemoryStore<AnnouncementIdentifier, Announcement>>();
        var entity = ExampleAnnouncementFactory.Example1;
        store.AddRange([entity]);

        var targetValue = !entity.IsAvailableForPublicRead;
        var updated = entity with { IsAvailableForPublicRead = targetValue };

        store.Upsert([updated]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(store.FindById(entity.Identifier, _ => true), Is.EqualTo(updated));

            Assert.That(
                store.FindById(entity.Identifier, _ => true)!.IsAvailableForPublicRead,
                Is.EqualTo(targetValue)
            );
        }
    }
#endregion

#region FindById
    [Test]
    public void FindById_WhenEntityDoesNotExist_ReturnsNull()
    {
        var store = GetService<InMemoryStore<AnnouncementIdentifier, Announcement>>();
        var id = ExampleAnnouncementFactory.Example1.Identifier;

        Assert.That(store.FindById(id, _ => true), Is.Null);
    }

    [Test]
    public void FindById_WhenFilterReturnsFalse_ReturnsNull()
    {
        var store = GetService<InMemoryStore<AnnouncementIdentifier, Announcement>>();
        var entity = ExampleAnnouncementFactory.Example1;
        store.AddRange([entity]);

        Assert.That(store.FindById(entity.Identifier, _ => false), Is.Null);
    }

    [Test]
    public void FindById_WhenFilterReturnsTrue_ReturnsEntity()
    {
        var store = GetService<InMemoryStore<AnnouncementIdentifier, Announcement>>();
        var entity = ExampleAnnouncementFactory.Example1;
        store.AddRange([entity]);

        Assert.That(store.FindById(entity.Identifier, _ => true), Is.EqualTo(entity));
    }
#endregion

#region FindManyById
    [Test]
    public async Task FindManyById_WhenEntityDoesNotExist_YieldsNothing()
    {
        var store = GetService<InMemoryStore<AnnouncementIdentifier, Announcement>>();
        var id = ExampleAnnouncementFactory.Example1.Identifier;

        var result = new List<Announcement>();

        await foreach (var item in store.FindManyById([id], _ => true))
        {
            result.Add(item);
        }

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task FindManyById_WhenFilterReturnsFalse_YieldsNothing()
    {
        var store = GetService<InMemoryStore<AnnouncementIdentifier, Announcement>>();
        var entity = ExampleAnnouncementFactory.Example1;
        store.AddRange([entity]);

        var result = new List<Announcement>();

        await foreach (var item in store.FindManyById([entity.Identifier], _ => false))
        {
            result.Add(item);
        }

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task FindManyById_WhenFilterReturnsTrue_YieldsEntity()
    {
        var store = GetService<InMemoryStore<AnnouncementIdentifier, Announcement>>();
        var entity = ExampleAnnouncementFactory.Example1;
        store.AddRange([entity]);

        var result = new List<Announcement>();

        await foreach (var item in store.FindManyById([entity.Identifier], _ => true))
        {
            result.Add(item);
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0], Is.EqualTo(entity));
        }
    }
#endregion
}
