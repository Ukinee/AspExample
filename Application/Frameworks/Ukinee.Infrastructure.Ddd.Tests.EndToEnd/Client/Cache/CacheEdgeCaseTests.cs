using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Ukinee.Infrastructure.Ddd.Common.LocalCache.Contracts;
using Ukinee.Infrastructure.Ddd.Common.UseCases.Contracts;
using Ukinee.Infrastructure.Ddd.Tests.Common.Services;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Borders;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Users;
using Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Client.Extensions;

namespace Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Client.Cache;

public class CacheEdgeCaseTests : ClientTestBase
{
    protected override void ConfigureTestServices(IServiceCollection services)
    {
        base.ConfigureTestServices(services);

        services.Decorate<IEntityCache<BorderIdentifier, Border>, PausingEntityCache<BorderIdentifier, Border>>();
    }

    [Test]
    public async Task GetDuringUpdate_ShouldNotPoisonCache()
    {
        var createUseCase = GetService<ICreateEntityUseCase<Border, CreateBorderRequest>>();
        var updateUseCase = GetService<IUpdateEntityUseCase<BorderIdentifier, Border, UpdateBorderRequest>>();
        var getUseCase = GetService<IGetEntityUseCase<BorderIdentifier, Border>>();
        var pause = (PausingEntityCache<BorderIdentifier, Border>)GetService<IEntityCache<BorderIdentifier, Border>>();

        var actor = ExampleUserFactory.UserAdmin1;
        await EnsureToken(actor);

        var createRequest = ExampleBorderFactory.CreateRequest1;
        var updateRequest = ExampleBorderFactory.UpdateRequest1;

        var createResult = await createUseCase.Execute(actor, createRequest, CancellationToken.None);
        var id = createResult.Identifier;

        pause.ArmBeforeUpsert(id);
        
        pause.Invalidate(id); // remove if createUseCase cached it's results so read reads from source and upserts

        var staleRead = Task.Run(() => getUseCase.Execute(actor, id, CancellationToken.None)); // T0: Read request with enabled caching

        await pause.WaitForUpsertAttemptAsync();

        var updateResult = await updateUseCase.Execute(actor, id, updateRequest, CancellationToken.None); // T1: Update invalidates before read writes to cache

        pause.ReleaseUpsert();
        await staleRead; // T2: Read tries to write stale value to cache

        var fresh = await getUseCase.Execute(actor, id, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(updateResult.Size, Is.EqualTo(updateRequest.Size));

            Assert.That(
                fresh.Size,
                Is.EqualTo(updateRequest.Size),
                "Stale read must not poison the cache after a concurrent update."
            );
        }
    }
}
