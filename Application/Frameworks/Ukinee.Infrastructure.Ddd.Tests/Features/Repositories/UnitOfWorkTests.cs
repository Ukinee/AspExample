using Examples.Common.Domain.Models.Entities;
using Examples.Common.Domain.Models.Identifiers;
using Examples.Common.Startup;
using Examples.Server.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Ukinee.Infrastructure.Ddd.Common.EventBuses;
using Ukinee.Infrastructure.Ddd.Local.InMemory.Repositories;
using Ukinee.Infrastructure.Ddd.Local.Repositories;
using Ukinee.Infrastructure.Ddd.Local.UnitOfWork.Contacts;
using Ukinee.Infrastructure.Ddd.Local.UnitOfWork.Implementations;
using Ukinee.Infrastructure.Ddd.Tests.Utils.Factories;
using Ukinee.Infrastructure.Ddd.Tests.Utils.Mocks;
using Ukinee.Infrastructure.Ddd.Tests.Utils.TestBases;

namespace Ukinee.Infrastructure.Ddd.Tests.Features.Repositories;

public class UnitOfWorkTests : LocationsTestBase
{
    private static Review Review1 => ReviewFactory.Example1;
    private static Review Review2 => ReviewFactory.Example2;

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        var config = new MediatRConfig {
            AssembliesToScanHandlers = [typeof(Program).Assembly],
        };

        services.AddSingleton<InMemoryStore<LocationIdentifier, Location>>();
        services.AddScoped<IEditableTrackedRepository<LocationIdentifier, Location>, InMemoryRepositoryLocations<LocationIdentifier, Location, ServerExampleTag>>();

        services.AddSingleton<InMemoryStore<ReviewIdentifier, Review>>();
        services.AddScoped<IEditableTrackedRepository<ReviewIdentifier, Review>, InMemoryRepositoryLocations<ReviewIdentifier, Review, ServerExampleTag>>();

        services.SetupCommonServices(config);

        services.AddScoped<UnitOfWorkFactory>();
        services.AddScoped<IUnitOfWorkProvider>(sp => sp.GetRequiredService<UnitOfWorkFactory>());
        services.AddScoped<IUnitOfWorkFactory>(sp => sp.GetRequiredService<UnitOfWorkFactory>());

        services.AddLogging();
    }

    [Test]
    public async Task Commit_Twice_DoesThrowAndPersistsOnce()
    {
        var repository = Repository;
        var store = Store;
        var factory = GetService<IUnitOfWorkFactory>();

        await using var uow = factory.Create<ServerExampleTag>();

        await repository.AddRange([Example1], CancellationToken);

        await uow.CommitAsync(CancellationToken);

        Assert.ThrowsAsync<InvalidOperationException>(() => uow.CommitAsync(CancellationToken));
        Assert.That(store.Collection, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Rollback_AfterCommit_DoesNotUndoPersistedData()
    {
        var repository = Repository;
        var factory = GetService<IUnitOfWorkFactory>();

        await using var uow = factory.Create<ServerExampleTag>();

        await repository.AddRange([Example1], CancellationToken);
        await uow.CommitAsync(CancellationToken);

        Assert.ThrowsAsync<InvalidOperationException>(() => uow.RollbackAsync(CancellationToken));
    }

    [Test]
    public async Task Dispose_AfterCommit_DoesNotRollback()
    {
        var repository = Repository;
        var store = Store;
        var factory = GetService<IUnitOfWorkFactory>();

        await using (var uow = factory.Create<ServerExampleTag>())
        {
            await repository.AddRange([Example1], CancellationToken);
            await uow.CommitAsync(CancellationToken);
        }

        Assert.That(store.Collection, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Commit_PreparesAllThenCommitsInOrder()
    {
        var log = new List<string>();
        var a = new FakeUnitOfWorkPart(typeof(RepoA), isImportant: true, log);
        var b = new FakeUnitOfWorkPart(typeof(RepoB), isImportant: false, log);

        var factory = GetService<IUnitOfWorkFactory>();
        var uow = (IEditableUnitOfWork)factory.Create<ServerExampleTag>();
        uow.RegisterPart(a);
        uow.RegisterPart(b);

        await uow.CommitAsync(CancellationToken);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(
                log,
                Is.EqualTo(
                    new[] {
                        RepoA.Prepare, RepoB.Prepare,
                        RepoB.Commit, RepoA.Commit,
                        RepoA.Dispose, RepoB.Dispose,
                    }
                )
            );

            Assert.That(a.Committed, Is.True);
            Assert.That(b.Committed, Is.True);
            Assert.That(a.DisposeCalled, Is.False);
            Assert.That(b.DisposeCalled, Is.False);
        }
    }

    [Test]
    public async Task Commit_PrepareFails_NobodyCommits()
    {
        var log = new List<string>();
        var a = new FakeUnitOfWorkPart(typeof(RepoA), true, log, failOnPrepare: true);
        var b = new FakeUnitOfWorkPart(typeof(RepoB), false, log);

        var factory = GetService<IUnitOfWorkFactory>();
        var uow = (IEditableUnitOfWork)factory.Create<ServerExampleTag>();
        uow.RegisterPart(a);
        uow.RegisterPart(b);

        Assert.ThrowsAsync<InvalidOperationException>(() => uow.CommitAsync(CancellationToken));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(log, Does.Not.Contain(RepoA.Commit));
            Assert.That(log, Does.Not.Contain(RepoB.Commit));

            Assert.That(a.RollbackCalled, Is.True);
            Assert.That(b.RollbackCalled, Is.True);

            Assert.That(a.DisposeCalled, Is.False);
            Assert.That(b.DisposeCalled, Is.False);
        }
    }

    [Test]
    public async Task Commit_SecondPartFails_FirstIsPartialRollback()
    {
        var log = new List<string>();

        var b = new FakeUnitOfWorkPart(typeof(RepoB), isImportant: false, log);
        var a = new FakeUnitOfWorkPart(typeof(RepoA), isImportant: true, log, failOnCommit: true);

        var factory = GetService<IUnitOfWorkFactory>();
        var uow = (IEditableUnitOfWork)factory.Create<ServerExampleTag>();
        uow.RegisterPart(b);
        uow.RegisterPart(a);

        Assert.ThrowsAsync<InvalidOperationException>(() => uow.CommitAsync(CancellationToken));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(b.Committed, Is.True);
            Assert.That(b.PartialRollbackCalled, Is.True);
            Assert.That(b.RollbackCalled, Is.False);

            Assert.That(a.Committed, Is.False);
            Assert.That(a.PartialRollbackCalled, Is.False);
            Assert.That(a.RollbackCalled, Is.True);
        }
    }

    [Test]
    public async Task Commit_SecondImportantPartFails_InMemoryPartsAreRestored()
    {
        var locationRepo = GetService<IEditableTrackedRepository<LocationIdentifier, Location>>();
        var reviewRepo = GetService<IEditableTrackedRepository<ReviewIdentifier, Review>>();

        var locationStore = GetService<InMemoryStore<LocationIdentifier, Location>>();
        var reviewStore = GetService<InMemoryStore<ReviewIdentifier, Review>>();

        await locationRepo.AddRange([Example1], CancellationToken); // initial state
        await reviewRepo.AddRange([Review1], CancellationToken);

        var factory = GetService<IUnitOfWorkFactory>();
        var uow = factory.Create<ServerExampleTag>();

        await using (uow)
        {
            var editable = (IEditableUnitOfWork)uow;

            var failingPart = new FailingUnitOfWorkPart(typeof(object));
            editable.RegisterPart(failingPart);

            var targetLocationValue = !Example1.IsAvailableForPublicRead;

            await locationRepo.UpdateByIdAsync(
                Example1.Identifier,
                UpdateLock.Delta,
                _ => true,
                old => old with { IsAvailableForPublicRead = targetLocationValue },
                CancellationToken
            );

            await reviewRepo.AddRange([Review2], CancellationToken);

            Assert.ThrowsAsync<InvalidOperationException>(() => uow.CommitAsync(CancellationToken));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(locationStore.Collection, Has.Count.EqualTo(1));

                Assert.That(
                    locationStore.Collection[Example1.Identifier].IsAvailableForPublicRead,
                    Is.EqualTo(Example1.IsAvailableForPublicRead),
                    "Location should be restored from snapshot"
                );

                Assert.That(
                    reviewStore.Collection.ContainsKey(Review2.Identifier),
                    Is.False,
                    "Review2 should be missing due to partial rollback"
                );

                Assert.That(reviewStore.Collection, Has.Count.EqualTo(1));

                Assert.That(
                    reviewStore.Collection[Review1.Identifier],
                    Is.EqualTo(Review1),
                    "Review1 must remain as is"
                );

                Assert.That(failingPart.DisposeCalled, Is.True);
            }
        }
    }

    [Test]
    public async Task Commit_SecondImportantPartFails_DeletedEntityIsRestored()
    {
        var reviewRepo = GetService<IEditableTrackedRepository<ReviewIdentifier, Review>>();
        var reviewStore = GetService<InMemoryStore<ReviewIdentifier, Review>>();

        await reviewRepo.AddRange([Review1, Review2], CancellationToken);

        var factory = GetService<IUnitOfWorkFactory>();
        var uow = factory.Create<ServerExampleTag>();

        await using (uow)
        {
            var editable = (IEditableUnitOfWork)uow;
            editable.RegisterPart(new FailingUnitOfWorkPart(typeof(object)));

            await reviewRepo.RemoveRange([Review1.Identifier], _ => true, CancellationToken);

            Assert.ThrowsAsync<InvalidOperationException>(() => uow.CommitAsync(CancellationToken));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(reviewStore.Collection, Has.Count.EqualTo(2));
                Assert.That(reviewStore.Collection.ContainsKey(Review1.Identifier), Is.True);
                Assert.That(reviewStore.Collection.ContainsKey(Review2.Identifier), Is.True);
            }
        }
    }
}
