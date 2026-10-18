using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Ukinee.Infrastructure.Ddd.Common.LocalCache.Contracts;
using Ukinee.Infrastructure.Ddd.Common.UseCases.Contracts;
using Ukinee.Infrastructure.Ddd.Tests.Common.Services;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Announcements;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Users;
using Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Client.Extensions;

namespace Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Client.Cache;

public class CacheEdgeCaseTests : ClientTestBase
{
    protected override void ConfigureTestServices(IServiceCollection services)
    {
        base.ConfigureTestServices(services);

        services.Decorate<IEntityCache<AnnouncementIdentifier, Announcement>, PausingEntityCache<AnnouncementIdentifier, Announcement>>();
    }

    [Test]
    public async Task GetDuringUpdate_ShouldNotPoisonCache()
    {
        var createUseCase = GetService<ICreateEntityUseCase<Announcement, CreateAnnouncementRequest>>();
        var updateUseCase = GetService<IUpdateEntityUseCase<AnnouncementIdentifier, Announcement, UpdateAnnouncementContentsRequest>>();
        var getUseCase = GetService<IGetEntityUseCase<AnnouncementIdentifier, Announcement>>();
        var pause = (PausingEntityCache<AnnouncementIdentifier, Announcement>)GetService<IEntityCache<AnnouncementIdentifier, Announcement>>();

        var actor = ExampleUserFactory.User1;
        await EnsureToken(actor);

        var createRequest = ExampleAnnouncementFactory.CreateRequest1;
        var updateRequest = ExampleAnnouncementFactory.UpdateRequest1;

        var createResult = await createUseCase.Execute(actor, createRequest, CancellationToken.None);
        var id = createResult.Identifier;

        pause.ArmBeforeUpsert(id);

        var staleRead = Task.Run(() => getUseCase.Execute(actor, id, CancellationToken.None)); // T0: Read request with enabled caching

        await pause.WaitForUpsertAttemptAsync();

        var updateResult = await updateUseCase.Execute(actor, id, updateRequest, CancellationToken.None); // T1: Update invalidates before read writes to cache

        pause.ReleaseUpsert();
        await staleRead; // T2: Read tries to write stale value to cache

        var fresh = await getUseCase.Execute(actor, id, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(updateResult.Contents, Is.EqualTo(updateRequest.Contents));

            Assert.That(
                fresh.Contents,
                Is.EqualTo(updateRequest.Contents),
                "Stale read must not poison the cache after a concurrent update."
            );
        }
    }
}
