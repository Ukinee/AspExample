using System.Net;
using NUnit.Framework;
using Ukinee.Infrastructure.Ddd.Common.Exceptions;
using Ukinee.Infrastructure.Ddd.Common.UseCases.Contracts;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Announcements;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Borders;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Users;
using Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Client.Extensions;

namespace Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Client.CrudOperations;

public class ClientUpdateOperationsTests : ClientTestBase
{
    [Test]
    public async Task UpdateUseCase_LoggedIn_ShouldHaveAccess()
    {
        var createUseCase = GetService<ICreateEntityUseCase<Announcement, CreateAnnouncementRequest>>();
        var updateUseCase = GetService<IUpdateEntityUseCase<AnnouncementIdentifier, Announcement, UpdateAnnouncementContentsRequest>>();
        var getUseCase = GetService<IGetEntityUseCase<AnnouncementIdentifier, Announcement>>();

        var createRequest = ExampleAnnouncementFactory.CreateRequest1;
        var updateRequest = ExampleAnnouncementFactory.UpdateRequest1;
        var actor1 = ExampleUserFactory.User1;

        await EnsureToken(actor1);

        var createResult = await createUseCase.Execute(actor1, createRequest, CancellationToken.None);

        var getResult1 = await getUseCase.Execute(actor1, createResult.Identifier, CancellationToken.None);
        var updateResult = await updateUseCase.Execute(actor1, createResult.Identifier, updateRequest, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(getResult1, Is.Not.Null);
            Assert.That(createResult, Is.EqualTo(getResult1));
            Assert.That(createResult, Is.Not.SameAs(getResult1));
            Assert.That(updateResult.Contents, Is.EqualTo(updateRequest.Contents));
        }
    }

    [Test]
    public async Task UpdateUseCase_LoggedIn_NotOwner_ShouldFailWithNotFound()
    {
        var createUseCase = GetService<ICreateEntityUseCase<Announcement, CreateAnnouncementRequest>>();
        var updateUseCase = GetService<IUpdateEntityUseCase<AnnouncementIdentifier, Announcement, UpdateAnnouncementContentsRequest>>();
        var getUseCase = GetService<IGetEntityUseCase<AnnouncementIdentifier, Announcement>>();

        var createRequest = ExampleAnnouncementFactory.CreateRequest1;
        var updateRequest = ExampleAnnouncementFactory.UpdateRequest1;
        var actor1 = ExampleUserFactory.User1;
        var actor2 = ExampleUserFactory.User2;

        await EnsureToken(actor1);
        await EnsureToken(actor2);

        var createResult = await createUseCase.Execute(actor1, createRequest, CancellationToken.None);
        var getResult1 = await getUseCase.Execute(actor1, createResult.Identifier, CancellationToken.None);
        var updateResult1 = await updateUseCase.Execute(actor1, createResult.Identifier, updateRequest, CancellationToken.None);

        var getResult2 = await getUseCase.Execute(actor2, createResult.Identifier, CancellationToken.None);
        var updateResult2 = updateUseCase.Execute(actor2, createResult.Identifier, updateRequest, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(createResult, Is.EqualTo(getResult1));
            Assert.That(createResult, Is.Not.EqualTo(updateResult1));
            Assert.That(updateResult1, Is.EqualTo(getResult2));
            Assert.ThrowsAsync<EntityNotFoundException<AnnouncementIdentifier, Announcement>>(() => updateResult2);
        }
    }

    [Test]
    public async Task UpdateUseCase_LoggedIn_Guest_ShouldFailWithUnauthorized()
    {
        var createUseCase = GetService<ICreateEntityUseCase<Announcement, CreateAnnouncementRequest>>();
        var updateUseCase = GetService<IUpdateEntityUseCase<AnnouncementIdentifier, Announcement, UpdateAnnouncementContentsRequest>>();
        var getUseCase = GetService<IGetEntityUseCase<AnnouncementIdentifier, Announcement>>();

        var createRequest = ExampleAnnouncementFactory.CreateRequest1;
        var updateRequest = ExampleAnnouncementFactory.UpdateRequest1;
        var actor1 = ExampleUserFactory.User1;
        var actorGuest = ExampleUserFactory.UserGuest;

        await EnsureToken(actor1);

        var createResult = await createUseCase.Execute(actor1, createRequest, CancellationToken.None);
        var getResult1 = await getUseCase.Execute(actor1, createResult.Identifier, CancellationToken.None);

        var getResultGuest = await getUseCase.Execute(actorGuest, createResult.Identifier, CancellationToken.None);
        var updateResult2 = updateUseCase.Execute(actorGuest, createResult.Identifier, updateRequest, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(createResult, Is.EqualTo(getResult1));
            Assert.That(getResult1, Is.EqualTo(getResultGuest));
            var exception = Assert.ThrowsAsync<HttpRequestException>(() => updateResult2);

            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }
    }

    [Test]
    public async Task UpdateUseCase_Group_AllGroupShouldHaveAccess()
    {
        var createUseCase = GetService<ICreateEntityUseCase<Announcement, CreateAnnouncementRequest>>();
        var updateUseCase = GetService<IUpdateEntityUseCase<AnnouncementIdentifier, Announcement, UpdateAnnouncementContentsRequest>>();

        var createRequest = ExampleAnnouncementFactory.CreateRequest1;
        var updateRequest1 = ExampleAnnouncementFactory.UpdateRequest1;
        var updateRequest2 = ExampleAnnouncementFactory.UpdateRequest2;
        var actorAdmin1 = ExampleUserFactory.UserAdmin1;
        var actorAdmin2 = ExampleUserFactory.UserAdmin2;

        await EnsureToken(actorAdmin1);
        await EnsureToken(actorAdmin2);

        var createResult = await createUseCase.Execute(actorAdmin1, createRequest, CancellationToken.None);

        var updateResult1 = await updateUseCase.Execute(actorAdmin1, createResult.Identifier, updateRequest1, CancellationToken.None);
        var updateResult2 = await updateUseCase.Execute(actorAdmin2, createResult.Identifier, updateRequest2, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(updateResult1.Contents, Is.EqualTo(updateRequest1.Contents));
            Assert.That(updateResult2.Contents, Is.EqualTo(updateRequest2.Contents));
        }
    }

    [Test]
    public async Task UpdateUseCase_Group_Guest_ShouldFailWithUnauthorized()
    {
        var createUseCase = GetService<ICreateEntityUseCase<Announcement, CreateAnnouncementRequest>>();
        var updateUseCase = GetService<IUpdateEntityUseCase<AnnouncementIdentifier, Announcement, UpdateAnnouncementContentsRequest>>();

        var createRequest = ExampleAnnouncementFactory.CreateRequest1;
        var updateRequest1 = ExampleAnnouncementFactory.UpdateRequest1;
        var actorAdmin1 = ExampleUserFactory.UserAdmin1;
        var actorGuest = ExampleUserFactory.UserGuest;

        await EnsureToken(actorAdmin1);

        var createResult = await createUseCase.Execute(actorAdmin1, createRequest, CancellationToken.None);

        var updateResult1 = updateUseCase.Execute(actorGuest, createResult.Identifier, updateRequest1, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            var exception = Assert.ThrowsAsync<HttpRequestException>(() => updateResult1);

            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }
    }

    [Test]
    public async Task UpdateUseCase_Group_LoggedIn_ShouldFailWithForbidden()
    {
        var createUseCase = GetService<ICreateEntityUseCase<Border, CreateBorderRequest>>();
        var updateUseCase = GetService<IUpdateEntityUseCase<BorderIdentifier, Border, UpdateBorderRequest>>();

        var createRequest = ExampleBorderFactory.CreateRequest1;
        var updateRequest1 = ExampleBorderFactory.UpdateRequest1;
        var actorAdmin1 = ExampleUserFactory.UserAdmin1;
        var actor1 = ExampleUserFactory.User1;

        await EnsureToken(actorAdmin1);
        await EnsureToken(actor1);

        var createResult = await createUseCase.Execute(actorAdmin1, createRequest, CancellationToken.None);

        var updateResult1 = updateUseCase.Execute(actor1, createResult.Identifier, updateRequest1, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            var exception = Assert.ThrowsAsync<HttpRequestException>(() => updateResult1);

            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }
    }

    [Test]
    public async Task UpdateUseCase_GroupOwner_ShouldHaveAccess_NotOwner_ShouldFailWithNotFound()
    {
        var createUseCase = GetService<ICreateEntityUseCase<Announcement, CreateAnnouncementRequest>>();
        var updateUseCase = GetService<IUpdateEntityUseCase<AnnouncementIdentifier, Announcement, UpdateAnnouncementContentsRequest>>();

        var createRequest = ExampleAnnouncementFactory.CreateRequest1;
        var updateRequest2 = ExampleAnnouncementFactory.UpdateRequest1;
        var updateRequest3 = ExampleAnnouncementFactory.UpdateRequest2;
        var actor2 = ExampleUserFactory.User2;
        var actor3 = ExampleUserFactory.User3;

        await EnsureToken(actor2);
        await EnsureToken(actor3);

        var createResult = await createUseCase.Execute(actor2, createRequest, CancellationToken.None);

        var updateResult2 = await updateUseCase.Execute(actor2, createResult.Identifier, updateRequest2, CancellationToken.None);
        var updateResult3 = updateUseCase.Execute(actor3, createResult.Identifier, updateRequest3, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(updateResult2.Contents, Is.EqualTo(updateRequest2.Contents));

            Assert.ThrowsAsync<EntityNotFoundException<AnnouncementIdentifier, Announcement>>(() => updateResult3);
        }
    }
}
