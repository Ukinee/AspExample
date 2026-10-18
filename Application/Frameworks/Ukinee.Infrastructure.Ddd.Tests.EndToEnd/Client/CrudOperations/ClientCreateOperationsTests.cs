using System.Net;
using NUnit.Framework;
using Ukinee.Infrastructure.Ddd.Common.UseCases.Contracts;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Announcements;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Borders;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Cookies;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Users;
using Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Client.Extensions;
using Cookie = Ukinee.Infrastructure.Ddd.Tests.Domain.Cookies.Cookie;

namespace Ukinee.Infrastructure.Ddd.Tests.EndToEnd.Client.CrudOperations;

public class ClientCreateOperationsTests : ClientTestBase
{
    private ICreateEntityUseCase<Announcement, CreateAnnouncementRequest> _createAnnouncementUseCase;
    private IGetEntityUseCase<AnnouncementIdentifier, Announcement> _getAnnouncementUseCase;

    private ICreateEntityUseCase<Border, CreateBorderRequest> _createBorderUseCase;
    private IGetEntityUseCase<BorderIdentifier, Border> _getBorderUseCase;

    private ICreateEntityUseCase<Cookie, CreateCookieRequest> _createCookieUseCase;
    private IGetEntityUseCase<CookieIdentifier, Cookie> _getCookieUseCase;

    public override void ResolveServices()
    {
        _createAnnouncementUseCase = GetService<ICreateEntityUseCase<Announcement, CreateAnnouncementRequest>>();
        _getAnnouncementUseCase = GetService<IGetEntityUseCase<AnnouncementIdentifier, Announcement>>();

        _createBorderUseCase = GetService<ICreateEntityUseCase<Border, CreateBorderRequest>>();
        _getBorderUseCase = GetService<IGetEntityUseCase<BorderIdentifier, Border>>();

        _createCookieUseCase = GetService<ICreateEntityUseCase<Cookie, CreateCookieRequest>>();
        _getCookieUseCase = GetService<IGetEntityUseCase<CookieIdentifier, Cookie>>();
    }

    [Test]
    public async Task CreateUseCase_LoggedIn_ShouldCreate()
    {
        var request = ExampleAnnouncementFactory.CreateRequest1;
        var actor1 = ExampleUserFactory.User1;

        await EnsureToken(actor1);

        var createResult = await _createAnnouncementUseCase.Execute(actor1, request, CancellationToken.None);
        var getResult1 = await _getAnnouncementUseCase.Execute(actor1, createResult.Identifier, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(getResult1, Is.Not.Null);
            Assert.That(createResult, Is.EqualTo(getResult1));
        }
    }

    [Test]
    public async Task CreateUseCase_Guest_ShouldFailWithUnauthorized()
    {
        var request = ExampleAnnouncementFactory.CreateRequest1;
        var actorGuest = ExampleUserFactory.UserGuest;

        var createResult = _createAnnouncementUseCase.Execute(actorGuest, request, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            var exception = Assert.ThrowsAsync<HttpRequestException>(() => createResult);

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }
    }

    [Test]
    public async Task CreateUseCase_Group_ShouldFailWithUnauthorized()
    {
        var request = ExampleBorderFactory.CreateRequest1;
        var actor1 = ExampleUserFactory.User1;

        await EnsureToken(actor1);

        var createResult = _createBorderUseCase.Execute(actor1, request, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            var exception = Assert.ThrowsAsync<HttpRequestException>(() => createResult);

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }
    }

    [Test]
    public async Task CreateUseCase_Group_ShouldCreate()
    {
        var request = ExampleBorderFactory.CreateRequest1;
        var actorAdmin1 = ExampleUserFactory.UserAdmin1;

        await EnsureToken(actorAdmin1);

        var createResult = await _createBorderUseCase.Execute(actorAdmin1, request, CancellationToken.None);
        var getResult1 = await _getBorderUseCase.Execute(actorAdmin1, createResult.Identifier, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(getResult1, Is.Not.Null);
            Assert.That(createResult, Is.EqualTo(getResult1));
        }
    }

    [Test]
    public async Task CreateUseCase_GroupOwner_ShouldCreate()
    {
        var request = ExampleCookieFactory.CreateRequest1;
        var actor2 = ExampleUserFactory.User2;

        await EnsureToken(actor2);

        var createResult = await _createCookieUseCase.Execute(actor2, request, CancellationToken.None);
        var getResult2 = await _getCookieUseCase.Execute(actor2, createResult.Identifier, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(getResult2, Is.Not.Null);
            Assert.That(createResult, Is.EqualTo(getResult2));
        }
    }

    [Test]
    public async Task CreateUseCase_GroupOwner_ShouldFailWithForbidden()
    {
        var request = ExampleCookieFactory.CreateRequest1;
        var actor1 = ExampleUserFactory.User1;

        await EnsureToken(actor1);

        var createResult = _createCookieUseCase.Execute(actor1, request, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            var exception = Assert.ThrowsAsync<HttpRequestException>(() => createResult);

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }
    }
}
