using Ardalis.Specification;
using Examples.Common.Domain.Models.Entities;
using Examples.Common.Domain.Models.Identifiers;
using Examples.Common.Startup;
using Examples.Server.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Ukinee.Infrastructure.Ddd.Common.EventBuses;
using Ukinee.Infrastructure.Ddd.Common.Exceptions;
using Ukinee.Infrastructure.Ddd.Common.Specifications.Extensions;
using Ukinee.Infrastructure.Ddd.Local.InMemory.Repositories;
using Ukinee.Infrastructure.Ddd.Local.Repositories;
using Ukinee.Infrastructure.Ddd.Local.UnitOfWork.Contacts;
using Ukinee.Infrastructure.Ddd.Local.UnitOfWork.Implementations;
using Ukinee.Infrastructure.Ddd.Tests.Features.Repositories.Common;
using Ukinee.Infrastructure.Ddd.Tests.Utils.Factories;
using Ukinee.Infrastructure.Ddd.Tests.Utils.TestBases;

namespace Ukinee.Infrastructure.Ddd.Tests.Features.Repositories;

public class InMemoryRepositoryTests : LocationsTestBase
{
    protected override void ConfigureTestServices(IServiceCollection services)
    {
        var config = new MediatRConfig {
            AssembliesToScanHandlers = [typeof(Program).Assembly],
        };

        services.AddSingleton<InMemoryStore<LocationIdentifier, Location>>();
        services.AddScoped<IEditableTrackedRepository<LocationIdentifier, Location>, InMemoryRepositoryLocations<LocationIdentifier, Location, ServerExampleTag>>();

        services.SetupCommonServices(config);

        services.AddScoped<UnitOfWorkFactory>();
        services.AddScoped<IUnitOfWorkProvider>(sp => sp.GetRequiredService<UnitOfWorkFactory>());
        services.AddScoped<IUnitOfWorkFactory>(sp => sp.GetRequiredService<UnitOfWorkFactory>());

        services.AddLogging();
    }

    [TestCase(TransactionMode.None)]
    [TestCase(TransactionMode.Transaction)]
    public async Task FindByIdAsync_TrueFilter_ShouldReturnNullBeforeAndInstanceAfter(TransactionMode mode)
    {
        await ExecuteInMode(
            mode,
            async _ =>
            {
                var repository = Repository;

                var existing1Before = await repository.FindByIdAsync(Identifier1, True, CancellationToken);
                var existing2Before = await repository.FindByIdAsync(Identifier2, True, CancellationToken);

                await repository.AddRange([Example1, Example2], CancellationToken);

                var existing1After = await repository.FindByIdAsync(Identifier1, True, CancellationToken);
                var existing2After = await repository.FindByIdAsync(Identifier2, True, CancellationToken);

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(existing1Before, Is.Null);
                    Assert.That(existing2Before, Is.Null);

                    Assert.That(existing1After, Is.EqualTo(Example1));
                    Assert.That(existing2After, Is.EqualTo(Example2));
                }
            }
        );
    }

    [TestCase(TransactionMode.None)]
    [TestCase(TransactionMode.Transaction)]
    public async Task FindByIdAsync_FalseFilter_ShouldReturnNullBeforeAndAfter(TransactionMode mode)
    {
        await ExecuteInMode(
            mode,
            async _ =>
            {
                var repository = Repository;

                var existing1Before = await repository.FindByIdAsync(Identifier1, False, CancellationToken);
                var existing2Before = await repository.FindByIdAsync(Identifier2, False, CancellationToken);

                await repository.AddRange([Example1, Example2], CancellationToken);

                var existing1After = await repository.FindByIdAsync(Identifier1, False, CancellationToken);
                var existing2After = await repository.FindByIdAsync(Identifier2, False, CancellationToken);

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(existing1Before, Is.Null);
                    Assert.That(existing2Before, Is.Null);

                    Assert.That(existing1After, Is.Null);
                    Assert.That(existing2After, Is.Null);
                }
            }
        );
    }

    [TestCase(TransactionMode.None)]
    [TestCase(TransactionMode.Transaction)]
    public async Task FindManyByIdAsync_TrueFilter_ShouldYieldElements(TransactionMode mode)
    {
        await ExecuteInMode(
            mode,
            async _ =>
            {
                var repository = Repository;

                var existingBefore = await repository.FindManyByIdAsync([Identifier1, Identifier2], True, CancellationToken).ToListAsync();

                await repository.AddRange([Example1, Example2], CancellationToken);

                var existingAfter = await repository.FindManyByIdAsync([Identifier1, Identifier2], True, CancellationToken).ToDictionaryAsync(x => x.Identifier);

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(existingBefore, Has.Count.EqualTo(0));
                    Assert.That(existingAfter, Has.Count.EqualTo(2));

                    Assert.That(existingAfter[Identifier1], Is.EqualTo(Example1));
                    Assert.That(existingAfter[Identifier2], Is.EqualTo(Example2));
                }
            }
        );
    }

    [TestCase(TransactionMode.None)]
    [TestCase(TransactionMode.Transaction)]
    public async Task FindManyByIdAsync_FalseFilter_ShouldYieldNoElements(TransactionMode mode)
    {
        await ExecuteInMode(
            mode,
            async _ =>
            {
                var repository = Repository;

                var existingBefore = await repository.FindManyByIdAsync([Identifier1, Identifier2], False, CancellationToken).ToListAsync();

                await repository.AddRange([Example1, Example2], CancellationToken);

                var existingAfter = await repository.FindManyByIdAsync([Identifier1, Identifier2], False, CancellationToken).ToDictionaryAsync(x => x.Identifier);

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(existingBefore, Has.Count.EqualTo(0));
                    Assert.That(existingAfter, Has.Count.EqualTo(0));
                }
            }
        );
    }

    [TestCase(TransactionMode.None)]
    [TestCase(TransactionMode.Transaction)]
    public async Task FindManyAsync_TrueFilter_ShouldYieldElements(TransactionMode mode)
    {
        await ExecuteInMode(
            mode,
            async _ =>
            {
                var repository = Repository;

                await repository.AddRange([Example1, Example2, Example3], CancellationToken);

                var specification = Specification.For<Location>().Where(x => x.IsAvailableForPublicRead).Specification;

                var existing = await repository.FindManyAsync(specification, True, CancellationToken).ToListAsync();

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(existing, Has.Count.EqualTo(2));
                    Assert.That(existing, Contains.Item(Example1));
                    Assert.That(existing, Contains.Item(Example3));
                }
            }
        );
    }

    [TestCase(TransactionMode.None)]
    [TestCase(TransactionMode.Transaction)]
    public async Task AddRange_MultipleAdd_ThrowsEntityAlreadyExistsException(TransactionMode mode)
    {
        await ExecuteInMode(
            mode,
            async _ =>
            {
                var repository = Repository;

                await repository.AddRange([Example1, Example2, Example3], CancellationToken);

                Assert.ThrowsAsync<EntityAlreadyExistsException<LocationIdentifier, Location>>(() => repository.AddRange([Example1, Example3], CancellationToken));
            }
        );
    }

    [TestCase(TransactionMode.None)]
    [TestCase(TransactionMode.Transaction)]
    public async Task UpdateEntityById_EmptyRepository_ThrowsEntityNotFoundException(TransactionMode mode)
    {
        await ExecuteInMode(
            mode,
            async _ =>
            {
                var repository = Repository;

                AsyncTestDelegate testDelegate = () => repository.UpdateByIdAsync(Identifier1, UpdateLock.Delta, True, old => old, CancellationToken);

                Assert.ThrowsAsync<EntityNotFoundException<LocationIdentifier, Location>>(testDelegate);
            }
        );
    }

    [TestCase(TransactionMode.None)]
    [TestCase(TransactionMode.Transaction)]
    public async Task UpdateEntityById_TrueFilter_NoId_ThrowsEntityNotFoundException(TransactionMode mode)
    {
        await ExecuteInMode(
            mode,
            async _ =>
            {
                var repository = Repository;

                await repository.AddRange([Example1, Example3], CancellationToken);

                AsyncTestDelegate testDelegate = () => repository.UpdateByIdAsync(Identifier2, UpdateLock.Delta, True, old => old, CancellationToken);

                Assert.ThrowsAsync<EntityNotFoundException<LocationIdentifier, Location>>(testDelegate);
            }
        );
    }

    [TestCase(TransactionMode.None)]
    [TestCase(TransactionMode.Transaction)]
    public async Task UpdateEntityById_FalseFilter_ThrowsEntityNotFoundException(TransactionMode mode)
    {
        await ExecuteInMode(
            mode,
            async _ =>
            {
                var repository = Repository;

                await repository.AddRange([Example1, Example3], CancellationToken);

                AsyncTestDelegate testDelegate = () => repository.UpdateByIdAsync(Identifier1, UpdateLock.Delta, False, old => old, CancellationToken);

                Assert.ThrowsAsync<EntityNotFoundException<LocationIdentifier, Location>>(testDelegate);
            }
        );
    }

    [TestCase(TransactionMode.None)]
    [TestCase(TransactionMode.Transaction)]
    public async Task UpdateEntityById_TrueFilter_EntityResultUpdates(TransactionMode mode)
    {
        await ExecuteInMode(
            mode,
            async _ =>
            {
                var repository = Repository;

                await repository.AddRange([Example1, Example3], CancellationToken);

                var targetValue = !Example1.IsAvailableForPublicRead;

                var result = await repository.UpdateByIdAsync(
                    Identifier1,
                    UpdateLock.Delta,
                    True,
                    old => old with {
                        IsAvailableForPublicRead = targetValue,
                    },
                    CancellationToken
                );

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(result.Old, Is.Not.Null);
                    Assert.That(result.Old, Is.EqualTo(Example1));
                    Assert.That(result.Updated, Is.EqualTo(Example1 with { IsAvailableForPublicRead = targetValue }));
                }
            }
        );
    }

    [TestCase(TransactionMode.None)]
    [TestCase(TransactionMode.Transaction)]
    public async Task UpdateEntityById_TrueFilter_EntityInsideUpdates(TransactionMode mode)
    {
        await ExecuteInMode(
            mode,
            async _ =>
            {
                var repository = Repository;

                var entity = LocationFactory.Example1;
                await repository.AddRange([entity], CancellationToken);

                var targetValue = !entity.IsAvailableForPublicRead;

                await repository.UpdateByIdAsync(
                    entity.Identifier,
                    UpdateLock.Delta,
                    True,
                    (old) => old with { IsAvailableForPublicRead = targetValue },
                    CancellationToken
                );

                var fromStore = await repository.FindByIdAsync(entity.Identifier, True, CancellationToken);

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(fromStore, Is.Not.Null);
                    Assert.That(fromStore.IsAvailableForPublicRead, Is.EqualTo(targetValue));
                }
            }
        );
    }

    [TestCase(TransactionMode.None)]
    [TestCase(TransactionMode.Transaction)]
    public async Task FindAsync_TrueFilterAndMatchingSpecification_ShouldReturnInstance(TransactionMode mode)
    {
        await ExecuteInMode(
            mode,
            async _ =>
            {
                var repository = Repository;

                await repository.AddRange([Example1, Example2, Example3], CancellationToken);

                var specification = Specification.For<Location>().Where(x => x.IsAvailableForPublicRead).Specification;

                var result = await repository.FindAsync(specification, True, CancellationToken);

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(result, Is.Not.Null);
                    Assert.That(result!.IsAvailableForPublicRead, Is.True);
                }
            }
        );
    }

    [TestCase(TransactionMode.None)]
    [TestCase(TransactionMode.Transaction)]
    public async Task FindAsync_FalseFilter_ShouldReturnNull(TransactionMode mode)
    {
        await ExecuteInMode(
            mode,
            async _ =>
            {
                var repository = Repository;

                await repository.AddRange([Example1, Example2, Example3], CancellationToken);

                var specification = Specification.For<Location>().Where(x => x.IsAvailableForPublicRead).Specification;

                var result = await repository.FindAsync(specification, False, CancellationToken);

                Assert.That(result, Is.Null);
            }
        );
    }

    [TestCase(TransactionMode.None)]
    [TestCase(TransactionMode.Transaction)]
    public async Task FindAsync_EmptyRepository_ShouldReturnNull(TransactionMode mode)
    {
        await ExecuteInMode(
            mode,
            async _ =>
            {
                var repository = Repository;

                var specification = Specification.For<Location>().Where(x => x.IsAvailableForPublicRead).Specification;

                var result = await repository.FindAsync(specification, True, CancellationToken);

                Assert.That(result, Is.Null);
            }
        );
    }

    [TestCase(TransactionMode.None)]
    [TestCase(TransactionMode.Transaction)]
    public async Task FindAsync_SpecificationMatchesNothing_ShouldReturnNull(TransactionMode mode)
    {
        await ExecuteInMode(
            mode,
            async _ =>
            {
                var repository = Repository;

                await repository.AddRange([Example2], CancellationToken);

                var specification = Specification.For<Location>().Where(x => x.IsAvailableForPublicRead).Specification;

                var result = await repository.FindAsync(specification, True, CancellationToken);

                Assert.That(result, Is.Null);
            }
        );
    }

    [TestCase(TransactionMode.None)]
    [TestCase(TransactionMode.Transaction)]
    public async Task FindManyAsync_FalseFilter_ShouldYieldNoElements(TransactionMode mode)
    {
        await ExecuteInMode(
            mode,
            async _ =>
            {
                var repository = Repository;

                await repository.AddRange([Example1, Example2, Example3], CancellationToken);

                var specification = Specification.For<Location>().Where(x => x.IsAvailableForPublicRead).Specification;

                var existing = await repository.FindManyAsync(specification, False, CancellationToken).ToListAsync();

                Assert.That(existing, Is.Empty);
            }
        );
    }

    [TestCase(TransactionMode.None)]
    [TestCase(TransactionMode.Transaction)]
    public async Task FindManyAsync_EmptyRepository_ShouldYieldNoElements(TransactionMode mode)
    {
        await ExecuteInMode(
            mode,
            async _ =>
            {
                var repository = Repository;

                var specification = Specification.For<Location>().Where(x => x.IsAvailableForPublicRead).Specification;

                var existing = await repository.FindManyAsync(specification, True, CancellationToken).ToListAsync();

                Assert.That(existing, Is.Empty);
            }
        );
    }

    [TestCase(TransactionMode.None)]
    [TestCase(TransactionMode.Transaction)]
    public async Task FindManyAsync_SpecificationMatchesNothing_ShouldYieldNoElements(TransactionMode mode)
    {
        await ExecuteInMode(
            mode,
            async _ =>
            {
                var repository = Repository;

                await repository.AddRange([Example2], CancellationToken);

                var specification = Specification.For<Location>().Where(x => x.IsAvailableForPublicRead).Specification;

                var existing = await repository.FindManyAsync(specification, True, CancellationToken).ToListAsync();

                Assert.That(existing, Is.Empty);
            }
        );
    }

    [TestCase(TransactionMode.None)]
    [TestCase(TransactionMode.Transaction)]
    public async Task FindManyByIdAsync_EmptyRepository_ShouldYieldNoElements(TransactionMode mode)
    {
        await ExecuteInMode(
            mode,
            async _ =>
            {
                var repository = Repository;

                var existing = await repository.FindManyByIdAsync([Identifier1, Identifier2], True, CancellationToken).ToListAsync();

                Assert.That(existing, Is.Empty);
            }
        );
    }

    [TestCase(TransactionMode.None)]
    [TestCase(TransactionMode.Transaction)]
    public async Task FindManyByIdAsync_PartiallyMissingIds_ShouldYieldOnlyExisting(TransactionMode mode)
    {
        await ExecuteInMode(
            mode,
            async _ =>
            {
                var repository = Repository;

                await repository.AddRange([Example1, Example3], CancellationToken);

                var existing = await repository
                    .FindManyByIdAsync([Identifier1, Identifier2, Identifier3], True, CancellationToken)
                    .ToDictionaryAsync(x => x.Identifier);

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(existing, Has.Count.EqualTo(2));
                    Assert.That(existing.ContainsKey(Identifier1), Is.True);
                    Assert.That(existing.ContainsKey(Identifier2), Is.False);
                    Assert.That(existing.ContainsKey(Identifier3), Is.True);
                }
            }
        );
    }

    [TestCase(TransactionMode.None)]
    [TestCase(TransactionMode.Transaction)]
    public async Task AddRange_SingleEntity_ShouldBeFindableByIdentifier(TransactionMode mode)
    {
        await ExecuteInMode(
            mode,
            async _ =>
            {
                var repository = Repository;

                await repository.AddRange([Example1], CancellationToken);

                var found = await repository.FindByIdAsync(Identifier1, True, CancellationToken);

                Assert.That(found, Is.EqualTo(Example1));
            }
        );
    }

    [TestCase(TransactionMode.None)]
    [TestCase(TransactionMode.Transaction)]
    public async Task RemoveRange_TrueFilter_ShouldRemoveEntity(TransactionMode mode)
    {
        await ExecuteInMode(
            mode,
            async _ =>
            {
                var repository = Repository;

                await repository.AddRange([Example1, Example2], CancellationToken);

                var removed = await repository.RemoveRange([Identifier1], True, CancellationToken);

                var existing1 = await repository.FindByIdAsync(Identifier1, True, CancellationToken);
                var existing2 = await repository.FindByIdAsync(Identifier2, True, CancellationToken);

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(removed, Has.Count.EqualTo(1));
                    Assert.That(removed.Single(), Is.EqualTo(Example1));

                    Assert.That(existing1, Is.Null);
                    Assert.That(existing2, Is.EqualTo(Example2));
                }
            }
        );
    }

    [TestCase(TransactionMode.None)]
    [TestCase(TransactionMode.Transaction)]
    public async Task RemoveRange_EmptyRepository_ThrowsEntityNotFoundException(TransactionMode mode)
    {
        await ExecuteInMode(
            mode,
            async _ =>
            {
                var repository = Repository;

                AsyncTestDelegate testDelegate = () => repository.RemoveRange([Identifier1], True, CancellationToken);

                Assert.ThrowsAsync<EntityNotFoundException<LocationIdentifier, Location>>(testDelegate);
            }
        );
    }

    [TestCase(TransactionMode.None)]
    [TestCase(TransactionMode.Transaction)]
    public async Task RemoveRange_MissingId_ThrowsEntityNotFoundException(TransactionMode mode)
    {
        await ExecuteInMode(
            mode,
            async _ =>
            {
                var repository = Repository;

                await repository.AddRange([Example1, Example3], CancellationToken);

                AsyncTestDelegate testDelegate = () => repository.RemoveRange([Identifier2], True, CancellationToken);

                Assert.ThrowsAsync<EntityNotFoundException<LocationIdentifier, Location>>(testDelegate);
            }
        );
    }

    [TestCase(TransactionMode.None)]
    [TestCase(TransactionMode.Transaction)]
    public async Task RemoveRange_FalseFilter_ThrowsEntityNotFoundException(TransactionMode mode)
    {
        await ExecuteInMode(
            mode,
            async _ =>
            {
                var repository = Repository;

                await repository.AddRange([Example1, Example3], CancellationToken);

                AsyncTestDelegate testDelegate = () => repository.RemoveRange([Identifier1], False, CancellationToken);

                Assert.ThrowsAsync<EntityNotFoundException<LocationIdentifier, Location>>(testDelegate);
            }
        );
    }

    [TestCase(TransactionMode.None)]
    [TestCase(TransactionMode.Transaction)]
    public async Task RemoveRange_MultipleIds_TrueFilter_ShouldRemoveAll(TransactionMode mode)
    {
        await ExecuteInMode(
            mode,
            async _ =>
            {
                var repository = Repository;

                await repository.AddRange([Example1, Example2, Example3], CancellationToken);

                var removed = await repository.RemoveRange([Identifier1, Identifier3], True, CancellationToken);

                var existing1 = await repository.FindByIdAsync(Identifier1, True, CancellationToken);
                var existing2 = await repository.FindByIdAsync(Identifier2, True, CancellationToken);
                var existing3 = await repository.FindByIdAsync(Identifier3, True, CancellationToken);

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(removed, Has.Count.EqualTo(2));

                    Assert.That(existing1, Is.Null);
                    Assert.That(existing2, Is.EqualTo(Example2));
                    Assert.That(existing3, Is.Null);
                }
            }
        );
    }

    [TestCase(TransactionMode.None)]
    [TestCase(TransactionMode.Transaction)]
    public async Task UpdateManyByIdAsync_TrueFilter_UpdatesAllAndReturnsResults(TransactionMode mode)
    {
        await ExecuteInMode(
            mode,
            async _ =>
            {
                var repository = Repository;

                await repository.AddRange([Example1, Example3], CancellationToken);

                var targetValue = !Example1.IsAvailableForPublicRead;

                var results = await repository.UpdateManyByIdAsync(
                    [Identifier1, Identifier3],
                    UpdateLock.Delta,
                    True,
                    (_, old) => old with { IsAvailableForPublicRead = targetValue },
                    CancellationToken
                );

                var fromStore1 = await repository.FindByIdAsync(Identifier1, True, CancellationToken);
                var fromStore3 = await repository.FindByIdAsync(Identifier3, True, CancellationToken);

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(results, Has.Count.EqualTo(2));

                    Assert.That(results.Single(r => r.Old.Identifier.Equals(Identifier1)).Old, Is.EqualTo(Example1));
                    Assert.That(results.Single(r => r.Old.Identifier.Equals(Identifier3)).Old, Is.EqualTo(Example3));

                    Assert.That(fromStore1!.IsAvailableForPublicRead, Is.EqualTo(targetValue));
                    Assert.That(fromStore3!.IsAvailableForPublicRead, Is.EqualTo(targetValue));
                }
            }
        );
    }

    [TestCase(TransactionMode.None)]
    [TestCase(TransactionMode.Transaction)]
    public async Task UpdateManyByIdAsync_EmptyRepository_ThrowsEntityNotFoundException(TransactionMode mode)
    {
        await ExecuteInMode(
            mode,
            async _ =>
            {
                var repository = Repository;

                AsyncTestDelegate testDelegate = () => repository.UpdateManyByIdAsync(
                    [Identifier1],
                    UpdateLock.Delta,
                    True,
                    (_, old) => old,
                    CancellationToken
                );

                Assert.ThrowsAsync<EntityNotFoundException<LocationIdentifier, Location>>(testDelegate);
            }
        );
    }

    [TestCase(TransactionMode.None)]
    [TestCase(TransactionMode.Transaction)]
    public async Task UpdateManyByIdAsync_MissingId_ThrowsEntityNotFoundException(TransactionMode mode)
    {
        await ExecuteInMode(
            mode,
            async ct =>
            {
                var repository = Repository;

                await repository.AddRange([Example1, Example3], ct);

                AsyncTestDelegate testDelegate = () => repository.UpdateManyByIdAsync(
                    [Identifier1, Identifier2],
                    UpdateLock.Delta,
                    True,
                    (_, old) => old,
                    ct
                );

                Assert.ThrowsAsync<EntityNotFoundException<LocationIdentifier, Location>>(testDelegate);
            }
        );
    }

    [TestCase(TransactionMode.None)]
    [TestCase(TransactionMode.Transaction)]
    public async Task UpdateManyByIdAsync_FalseFilter_ThrowsEntityNotFoundException(TransactionMode mode)
    {
        await ExecuteInMode(
            mode,
            async ct =>
            {
                var repository = Repository;

                await repository.AddRange([Example1, Example3], ct);

                AsyncTestDelegate testDelegate = () => repository.UpdateManyByIdAsync(
                    [Identifier1, Identifier3],
                    UpdateLock.Delta,
                    False,
                    (_, old) => old,
                    ct
                );

                Assert.ThrowsAsync<EntityNotFoundException<LocationIdentifier, Location>>(testDelegate);
            }
        );
    }

    private async Task ExecuteInMode(TransactionMode mode, Func<CancellationToken, Task> action)
    {
        if (mode == TransactionMode.None)
        {
            await action(CancellationToken);

            return;
        }

        var factory = GetService<IUnitOfWorkFactory>();

        await using var unitOfWork = factory.Create<ServerExampleTag>();

        await action(CancellationToken);
    }
}
