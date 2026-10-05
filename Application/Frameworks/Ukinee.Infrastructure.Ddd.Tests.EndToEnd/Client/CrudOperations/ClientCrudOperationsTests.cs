using NUnit.Framework;
using Ukinee.Infrastructure.Ddd.Common.UseCases.Contracts;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Announcements;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Users;
using Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Client.Extensions;

namespace Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Client.CrudOperations;

public class ClientCrudOperationsTests : ClientTestBase
{
    [Test]
    public async Task CreateUseCase_LoggedIn_ShouldCreateAvailableToGetEntityOnBacked()
    {
        var createUseCase = GetService<ICreateEntityUseCase<Announcement, CreateAnnouncementRequest>>();
        var getUseCase = GetService<IGetEntityUseCase<AnnouncementIdentifier, Announcement>>();

        var request = ExampleAnnouncementFactory.CreateRequest1;
        var actor1 = ExampleUserFactory.User1;
        var actor2 = ExampleUserFactory.User2;

        await EnsureToken(actor1);

        var createResult = await createUseCase.Execute(actor1, request, CancellationToken.None);
        var getResult1 = await getUseCase.Execute(actor1, createResult.Identifier, CancellationToken.None);
        var getResult2 = await getUseCase.Execute(actor2, createResult.Identifier, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(getResult1, Is.Not.Null);
            Assert.That(createResult, Is.EqualTo(getResult1));
            Assert.That(createResult, Is.Not.SameAs(getResult1));
            Assert.That(getResult1, Is.SameAs(getResult2), "Should be similar reference because of local caching");
        }
    }

    [Test]
    public async Task UpdateUseCase_LoggedIn_ShouldEntityOnBacked()
    {
        var createUseCase = GetService<ICreateEntityUseCase<Announcement, CreateAnnouncementRequest>>();
        var updateUseCase = GetService<IUpdateEntityUseCase<AnnouncementIdentifier, Announcement, UpdateAnnouncementRequest>>();
        var getUseCase = GetService<IGetEntityUseCase<AnnouncementIdentifier, Announcement>>();

        var createRequest = ExampleAnnouncementFactory.CreateRequest1;
        var updateRequest = ExampleAnnouncementFactory.UpdateRequest1;
        var actor1 = ExampleUserFactory.User1;

        await EnsureToken(actor1);

        var createResult = await createUseCase.Execute(actor1, createRequest, CancellationToken.None);

        var getResult1 = await getUseCase.Execute(actor1, createResult.Identifier, CancellationToken.None);

        var updateResult = await updateUseCase.Execute(actor1, createResult.Identifier, updateRequest, CancellationToken.None);

        var getResult2 = await getUseCase.Execute(actor1, createResult.Identifier, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(getResult1, Is.Not.Null);
            Assert.That(createResult, Is.EqualTo(getResult1));
            Assert.That(createResult, Is.Not.SameAs(getResult1));
            Assert.That(getResult1, Is.Not.SameAs(getResult2), "Updates must invalidate or change cached entities");
        }
    }
}
