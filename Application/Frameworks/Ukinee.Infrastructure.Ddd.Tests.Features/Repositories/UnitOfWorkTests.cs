using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using NUnit.Framework;
using Ukinee.Infrastructure.Ddd.Common.UnitOfWork.Contacts;
using Ukinee.Infrastructure.Ddd.Common.UnitOfWork.Implementations;
using Ukinee.Infrastructure.Ddd.DependencyInjection.DependenciesStartup;
using Ukinee.Infrastructure.Ddd.Local.InMemory.Repositories.Transacted;
using Ukinee.Infrastructure.Ddd.Local.Repositories;
using Ukinee.Infrastructure.Ddd.Tests.Common.Utils.Mocks;
using Ukinee.Infrastructure.Ddd.Tests.Common.Utils.TestBases;
using Ukinee.Infrastructure.Ddd.Tests.Domain;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Announcements;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Borders;

namespace Ukinee.Infrastructure.Ddd.Tests.Features.Repositories;

public class UnitOfWorkTests : AnnouncementTestBase
{
    private static Border Border1 => ExampleBorderFactory.Example1;
    private static Border Border2 => ExampleBorderFactory.Example2;

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        var config = new MediatRConfig {
            AssembliesToScanHandlers = [typeof(Program).Assembly],
        };

        services.AddSingleton<InMemoryStore<AnnouncementIdentifier, Announcement>>();

        services
            .AddScoped<IEditableTrackedRepository<AnnouncementIdentifier, Announcement>,
                TransactedInMemoryRepository<TestingTag, AnnouncementIdentifier, Announcement>>();

        services.AddSingleton<InMemoryStore<BorderIdentifier, Border>>();
        services.AddScoped<IEditableTrackedRepository<BorderIdentifier, Border>, TransactedInMemoryRepository<TestingTag, BorderIdentifier, Border>>();

        services
            .SetupUnitOfWork()
            .SetupMediatR(config);

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

        await using var uow = factory.Create<TestingTag>();

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

        await using var uow = factory.Create<TestingTag>();

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

        await using (var uow = factory.Create<TestingTag>())
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
        var a = new FakeUnitOfWorkPart(typeof(RepoA), canRollbackPartial: true, log);
        var b = new FakeUnitOfWorkPart(typeof(RepoB), canRollbackPartial: false, log);

        var factory = GetService<IUnitOfWorkFactory>();
        var uow = (IEditableUnitOfWork)factory.Create<TestingTag>();
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
                        RepoA.Commit, RepoB.Commit,
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
        var uow = (IEditableUnitOfWork)factory.Create<TestingTag>();
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

        var b = new FakeUnitOfWorkPart(typeof(RepoB), canRollbackPartial: true, log);
        var a = new FakeUnitOfWorkPart(typeof(RepoA), canRollbackPartial: false, log, failOnCommit: true);

        var factory = GetService<IUnitOfWorkFactory>();
        var uow = (IEditableUnitOfWork)factory.Create<TestingTag>();
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
        var locationRepo = GetService<IEditableTrackedRepository<AnnouncementIdentifier, Announcement>>();
        var reviewRepo = GetService<IEditableTrackedRepository<BorderIdentifier, Border>>();

        var locationStore = GetService<InMemoryStore<AnnouncementIdentifier, Announcement>>();
        var reviewStore = GetService<InMemoryStore<BorderIdentifier, Border>>();

        await locationStore.WriteAsync(
            (session) =>
            {
                session.UpsertNew(Example1);
            },
            CancellationToken
        );

        await reviewStore.WriteAsync(
            (session) =>
            {
                session.UpsertNew(Border1);
            },
            CancellationToken
        );

        var review1 = reviewStore.Collection[Border1.Identifier].Entity;

        var factory = GetService<IUnitOfWorkFactory>();
        var uow = factory.Create<TestingTag>();

        await using (uow)
        {
            var editable = (IEditableUnitOfWork)uow;

            var failingPart = new FailingUnitOfWorkPart(typeof(object));
            editable.RegisterPart(failingPart);

            var targetLocationValue = !Example1.IsAvailableForPublicRead;

            await locationRepo.UpdateByIdAsync(
                Example1.Identifier,
                _ => true,
                old => old with { IsAvailableForPublicRead = targetLocationValue },
                CancellationToken
            );

            await reviewRepo.AddRange([Border2], CancellationToken);

            Assert.ThrowsAsync<InvalidOperationException>(() => uow.CommitAsync(CancellationToken));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(locationStore.Collection, Has.Count.EqualTo(1));

                Assert.That(
                    locationStore.Collection[Example1.Identifier].Entity.IsAvailableForPublicRead,
                    Is.EqualTo(Example1.IsAvailableForPublicRead),
                    "Location should be restored from snapshot"
                );

                Assert.That(
                    reviewStore.Collection.ContainsKey(Border2.Identifier),
                    Is.False,
                    "Review2 should be missing due to partial rollback"
                );

                Assert.That(reviewStore.Collection, Has.Count.EqualTo(1));

                Assert.That(
                    reviewStore.Collection[Border1.Identifier].Entity,
                    Is.EqualTo(review1),
                    "Review1 must remain as is"
                );

                Assert.That(failingPart.DisposeCalled, Is.True);
            }
        }
    }

    [Test]
    public async Task Commit_SecondImportantPartFails_DeletedEntityIsRestored()
    {
        var reviewRepo = GetService<IEditableTrackedRepository<BorderIdentifier, Border>>();
        var reviewStore = GetService<InMemoryStore<BorderIdentifier, Border>>();

        await reviewStore.WriteAsync(
            session =>
            {
                session.UpsertNew(Border1);
                session.UpsertNew(Border2);
            },
            CancellationToken
        );

        var factory = GetService<IUnitOfWorkFactory>();
        var uow = factory.Create<TestingTag>();

        await using (uow)
        {
            var editable = (IEditableUnitOfWork)uow;
            editable.RegisterPart(new FailingUnitOfWorkPart(typeof(object)));

            await reviewRepo.RemoveRange([Border1.Identifier], _ => true, CancellationToken);

            Assert.ThrowsAsync<InvalidOperationException>(() => uow.CommitAsync(CancellationToken));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(reviewStore.Collection, Has.Count.EqualTo(2));
                Assert.That(reviewStore.Collection.ContainsKey(Border1.Identifier), Is.True);
                Assert.That(reviewStore.Collection.ContainsKey(Border2.Identifier), Is.True);
            }
        }
    }

    [Test]
    public async Task Commit_ModifiedAfterLoad_ThrowsConcurrencyException()
    {
        var scopeFactory = GetService<IServiceScopeFactory>();

        var store = GetService<InMemoryStore<BorderIdentifier, Border>>();

        var scope1 = scopeFactory.CreateScope();
        var repoScope1 = scope1.ServiceProvider.GetRequiredService<IEditableTrackedRepository<BorderIdentifier, Border>>();
        var factoryScope1 = scope1.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>();

        var scope2 = scopeFactory.CreateScope();
        var repoScope2 = scope2.ServiceProvider.GetRequiredService<IEditableTrackedRepository<BorderIdentifier, Border>>();
        var factoryScope2 = scope2.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>();

        await store.WriteAsync(s => s.UpsertNew(Border1), CancellationToken);

        // 1. Загружаем сущность в change tracker (OriginalVersion = 1)
        await using var uow1 = factoryScope1.Create<TestingTag>();
        var loaded = store.Collection[Border1.Identifier].Entity;
        await repoScope1.UpdateByIdAsync(loaded.Identifier, (_) => true, old => old with { Size = 2 }, CancellationToken);

        // 2. Другой writer коммитит изменение (store.version = 2)
        await using (var uow2 = factoryScope2.Create<TestingTag>())
        {
            await repoScope2.UpdateByIdAsync(loaded.Identifier, (_) => true, old => old with { Size = 3 }, CancellationToken);
            await uow2.CommitAsync(CancellationToken);
        }

        // 3. Первый UoW пытается коммитить с устаревшей версией
        var ex = Assert.ThrowsAsync<ConcurrencyException>(() => uow1.CommitAsync(CancellationToken));
        Assert.That(ex!.ExpectedVersion, Is.EqualTo(1));
        Assert.That(ex!.CurrentVersion, Is.EqualTo(2));
    }
}
