using Ardalis.Specification;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using NUnit.Framework;
using Ukinee.Infrastructure.Ddd.Common.Specifications.Extensions;
using Ukinee.Infrastructure.Ddd.Common.UnitOfWork.Contacts;
using Ukinee.Infrastructure.Ddd.Common.UnitOfWork.Implementations;
using Ukinee.Infrastructure.Ddd.DependencyInjection.DependenciesStartup;
using Ukinee.Infrastructure.Ddd.Local.Repositories;
using Ukinee.Infrastructure.Ddd.Tests.Common.Services.Repositories;
using Ukinee.Infrastructure.Ddd.Tests.Common.Utils.TestBases;
using Ukinee.Infrastructure.Ddd.Tests.Domain;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Announcements;

namespace Ukinee.Infrastructure.Ddd.Tests.Features.Repositories;

public class InMemoryTransactionsTests : AnnouncementTestBase
{
    protected override void ConfigureTestServices(IServiceCollection services)
    {
        var config = new MediatRConfig {
            AssembliesToScanHandlers = [typeof(Program).Assembly],
        };

        services.AddSingleton<InMemoryStore<AnnouncementIdentifier, Announcement>>();
        services.AddScoped<IEditableTrackedRepository<AnnouncementIdentifier, Announcement>, TransactedInMemoryRepository<AnnouncementIdentifier, Announcement, TestingTag>>();

        services
            .SetupUnitOfWork()
            .SetupMediatR(config);


        services.AddScoped<UnitOfWorkFactory>();
        services.AddScoped<IUnitOfWorkProvider>(sp => sp.GetRequiredService<UnitOfWorkFactory>());
        services.AddScoped<IUnitOfWorkFactory>(sp => sp.GetRequiredService<UnitOfWorkFactory>());

        services.AddLogging();
    }

    [Test]
    public async Task AddRange_Commit_AddsElements()
    {
        var repository = Repository;
        var store = Store;

        List<Announcement> elements = [Example1, Example2, Example3];
        var factory = GetService<IUnitOfWorkFactory>();

        var uow = factory.Create<TestingTag>();

        await using (uow)
        {
            await repository.AddRange(elements, CancellationToken);

            await uow.CommitAsync(CancellationToken);
        }

        Assert.That(store.Collection, Has.Count.EqualTo(3));
    }

    [Test]
    public async Task AddRange_Rollback_VoidsElements()
    {
        var repository = Repository;
        var store = Store;

        List<Announcement> elements = [Example1, Example2, Example3];
        var factory = GetService<IUnitOfWorkFactory>();

        var uow = factory.Create<TestingTag>();

        await using (uow)
        {
            await repository.AddRange(elements, CancellationToken);

            await uow.RollbackAsync(CancellationToken);
        }

        Assert.That(store.Collection, Has.Count.EqualTo(0));
    }

    [Test]
    public async Task AddRange_Dispose_VoidsElements()
    {
        var repository = Repository;
        var store = Store;

        List<Announcement> elements = [Example1, Example2, Example3];
        var factory = GetService<IUnitOfWorkFactory>();

        var uow = factory.Create<TestingTag>();

        await using (uow)
        {
            await repository.AddRange(elements, CancellationToken);

            await uow.RollbackAsync(CancellationToken);
        }

        Assert.That(store.Collection, Has.Count.EqualTo(0));
    }

    [Test]
    public async Task RemoveRange_Rollback_ReturnsElements()
    {
        var repository = Repository;
        var store = Store;

        List<Announcement> elements = [Example1, Example2, Example3];
        List<AnnouncementIdentifier> identifiers = [Identifier1, Identifier2, Identifier3];

        var factory = GetService<IUnitOfWorkFactory>();

        await repository.AddRange(elements, CancellationToken);

        var uow = factory.Create<TestingTag>();

        await using (uow)
        {
            await repository.RemoveRange(identifiers, True, CancellationToken);

            await uow.RollbackAsync(CancellationToken);
        }

        Assert.That(store.Collection, Has.Count.EqualTo(3));
    }

    [Test]
    public async Task FindByIdAsync_InTransaction_ReturnsElementsBothFromChangeTrackerAndStore()
    {
        var repository = Repository;
        var store = Store;

        List<Announcement> initialElements = [Example1, Example3];

        var factory = GetService<IUnitOfWorkFactory>();

        await repository.AddRange(initialElements, CancellationToken);

        var uow = factory.Create<TestingTag>();

        await using (uow)
        {
            await repository.AddRange([Example2], CancellationToken);

            var found1 = await repository.FindByIdAsync(Identifier1, True, CancellationToken);
            var found2 = await repository.FindByIdAsync(Identifier2, True, CancellationToken);
            var found3 = await repository.FindByIdAsync(Identifier3, True, CancellationToken);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(store.Collection, Has.Count.EqualTo(2));

                Assert.That(found1, Is.Not.Null);
                Assert.That(found2, Is.Not.Null);
                Assert.That(found3, Is.Not.Null);
            }
        }
    }

    [Test]
    public async Task UpdateByIdAsync_Commit_UpdatesElementInStore()
    {
        var repository = Repository;
        var store = Store;

        await repository.AddRange([Example1], CancellationToken);

        var targetValue = !Example1.IsAvailableForPublicRead;
        var factory = GetService<IUnitOfWorkFactory>();

        await using (var uow = factory.Create<TestingTag>())
        {
            await repository.UpdateByIdAsync(
                Identifier1,
                True,
                old => old with { IsAvailableForPublicRead = targetValue },
                CancellationToken
            );

            await uow.CommitAsync(CancellationToken);
        }

        Assert.That(store.Collection[Identifier1].IsAvailableForPublicRead, Is.EqualTo(targetValue));
    }

    [Test]
    public async Task UpdateByIdAsync_Rollback_DoesNotUpdateStore()
    {
        var repository = Repository;
        var store = Store;

        await repository.AddRange([Example1], CancellationToken);

        var targetValue = !Example1.IsAvailableForPublicRead;
        var factory = GetService<IUnitOfWorkFactory>();

        await using (var uow = factory.Create<TestingTag>())
        {
            await repository.UpdateByIdAsync(
                Identifier1,
                True,
                old => old with { IsAvailableForPublicRead = targetValue },
                CancellationToken
            );

            await uow.RollbackAsync(CancellationToken);
        }

        Assert.That(
            store.Collection[Identifier1].IsAvailableForPublicRead,
            Is.EqualTo(Example1.IsAvailableForPublicRead)
        );
    }

    [Test]
    public async Task RemoveRange_Commit_RemovesElements()
    {
        var repository = Repository;
        var store = Store;

        await repository.AddRange([Example1, Example2, Example3], CancellationToken);

        var factory = GetService<IUnitOfWorkFactory>();

        await using (var uow = factory.Create<TestingTag>())
        {
            await repository.RemoveRange([Identifier1, Identifier3], True, CancellationToken);
            await uow.CommitAsync(CancellationToken);
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(store.Collection, Has.Count.EqualTo(1));
            Assert.That(store.Collection.ContainsKey(Identifier2), Is.True);
        }
    }

    [Test]
    public async Task AddUpdateRemove_Commit_AllPersisted()
    {
        var repository = Repository;
        var store = Store;

        await repository.AddRange([Example1], CancellationToken);

        var targetValue = !Example1.IsAvailableForPublicRead;
        var factory = GetService<IUnitOfWorkFactory>();

        await using (var uow = factory.Create<TestingTag>())
        {
            await repository.AddRange([Example2], CancellationToken);

            await repository.UpdateByIdAsync(
                Identifier1,
                True,
                old => old with { IsAvailableForPublicRead = targetValue },
                CancellationToken
            );

            await uow.CommitAsync(CancellationToken);
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(store.Collection, Has.Count.EqualTo(2));
            Assert.That(store.Collection.ContainsKey(Identifier2), Is.True);
            Assert.That(store.Collection[Identifier1].IsAvailableForPublicRead, Is.EqualTo(targetValue));
        }
    }

    [Test]
    public async Task AddUpdateRemove_Rollback_NothingPersisted()
    {
        var repository = Repository;
        var store = Store;

        await repository.AddRange([Example1, Example3], CancellationToken);

        var factory = GetService<IUnitOfWorkFactory>();

        await using (var uow = factory.Create<TestingTag>())
        {
            await repository.AddRange([Example2], CancellationToken);

            await repository.UpdateByIdAsync(
                Identifier1,
                True,
                old => old with { IsAvailableForPublicRead = !old.IsAvailableForPublicRead },
                CancellationToken
            );

            await repository.RemoveRange([Identifier3], True, CancellationToken);

            await uow.RollbackAsync(CancellationToken);
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(store.Collection, Has.Count.EqualTo(2));
            Assert.That(store.Collection.ContainsKey(Identifier2), Is.False);
            Assert.That(store.Collection[Identifier1], Is.EqualTo(Example1));
            Assert.That(store.Collection.ContainsKey(Identifier3), Is.True);
        }
    }

    [Test]
    public async Task FindByIdAsync_InTransaction_RemovedInTransaction_ReturnsNull()
    {
        var repository = Repository;

        await repository.AddRange([Example1], CancellationToken);

        var factory = GetService<IUnitOfWorkFactory>();

        await using (factory.Create<TestingTag>())
        {
            await repository.RemoveRange([Identifier1], True, CancellationToken);

            var found = await repository.FindByIdAsync(Identifier1, True, CancellationToken);

            Assert.That(found, Is.Null);
        }
    }

    [Test]
    public async Task FindManyAsync_InTransaction_SeesBothSources()
    {
        var repository = Repository;

        await repository.AddRange([Example1, Example3], CancellationToken);

        var specification = Specification
            .For<Announcement>()
            .Where(x => x.IsAvailableForPublicRead)
            .Specification;

        var factory = GetService<IUnitOfWorkFactory>();

        await using (factory.Create<TestingTag>())
        {
            await repository.AddRange([Example2], CancellationToken);

            var found = await repository
                .FindManyAsync(specification, True, CancellationToken)
                .ToListAsync();

            Assert.That(found, Has.Count.EqualTo(2));
        }
    }
}
