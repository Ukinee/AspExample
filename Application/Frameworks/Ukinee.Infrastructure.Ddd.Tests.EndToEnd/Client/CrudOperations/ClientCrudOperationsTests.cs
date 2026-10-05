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

        var creationResult = await createUseCase.Execute(actor1, request, CancellationToken.None);
        var gotResult1 = await getUseCase.Execute(actor1, creationResult.Identifier, CancellationToken.None);
        var gotResult2 = await getUseCase.Execute(actor2, creationResult.Identifier, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(gotResult1, Is.Not.Null);
            Assert.That(creationResult, Is.EqualTo(gotResult1));
            Assert.That(creationResult, Is.Not.SameAs(gotResult1));
            Assert.That(gotResult1, Is.SameAs(gotResult2));
        }
    }
}
