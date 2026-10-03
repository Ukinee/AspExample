using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Ukinee.Infrastructure.Ddd.Common.Authorization.Domain;
using Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.Local;
using Ukinee.Infrastructure.Ddd.Tests.Domain;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Announcements;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Borders;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Cookies;

namespace Ukinee.Infrastructure.Ddd.Tests.Features.Authorization;

public class LoggedInAuthorizationPolicyDefinitionTests : AuthorizationPolicyDefinitionTestBase
{
    protected override void ConfigureTestServices(IServiceCollection services)
    {
        var announcementPolicy = AuthorizationPolicyDefinition.LoggedInReadAndOwnerEdit<AnnouncementIdentifier, Announcement>(identifier => identifier.UserGuid);
        var borderPolicy = AuthorizationPolicyDefinition.LoggedInReadAndAdministratorEdit<BorderIdentifier, Border>();
        var cookiePolicy = TestingAuthorizationPolicyDefinition.LoggedInReadAndVipOwnerEdit<CookieIdentifier, Cookie>(identifier => identifier.UserGuid);

        var announcementPolicyDescriptors = AuthorizationHelperDi.GetDescriptors(announcementPolicy);
        var borderPolicyDescriptors = AuthorizationHelperDi.GetDescriptors(borderPolicy);
        var cookiePolicyDescriptors = AuthorizationHelperDi.GetDescriptors(cookiePolicy);

        var descriptors = announcementPolicyDescriptors
            .Concat(borderPolicyDescriptors)
            .Concat(cookiePolicyDescriptors);

        foreach (var descriptor in descriptors)
        {
            services.Add(descriptor);
        }
    }

    [Test]
    public async Task LoggedInRead_PublicRead_OwnerAndLoggedInMustHaveAccess()
    {
        var review = ExampleAnnouncementFactory.Example1;
        var results = await EvaluateReadAsync<AnnouncementIdentifier, Announcement>(review, AllUsers);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[User1], Is.True);
            Assert.That(results[User2], Is.True);
            Assert.That(results[User3], Is.True);
            Assert.That(results[Admin1], Is.True);
            Assert.That(results[Admin2], Is.True);
            Assert.That(results[Guest], Is.False);
        }
    }

    [Test]
    public async Task LoggedInRead_PrivateRead_OnlyOwnerMustHaveAccess()
    {
        var review = ExampleAnnouncementFactory.Example2;
        var results = await EvaluateReadAsync<AnnouncementIdentifier, Announcement>(review, AllUsers);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[User1], Is.False);
            Assert.That(results[User2], Is.True);
            Assert.That(results[User3], Is.False);
            Assert.That(results[Admin1], Is.False);
            Assert.That(results[Admin2], Is.False);
            Assert.That(results[Guest], Is.False);
        }
    }

    [Test]
    public async Task LoggedInRead_PublicRead_GroupAndLoggedInMustHaveAccess()
    {
        var review = ExampleBorderFactory.Example1;
        var results = await EvaluateReadAsync<BorderIdentifier, Border>(review, AllUsers);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[User1], Is.True);
            Assert.That(results[User2], Is.True);
            Assert.That(results[User3], Is.True);
            Assert.That(results[Admin1], Is.True);
            Assert.That(results[Admin2], Is.True);
            Assert.That(results[Guest], Is.False);
        }
    }

    [Test]
    public async Task LoggedInRead_PrivateRead_GroupMustHaveAccess()
    {
        var review = ExampleBorderFactory.Example2;
        var results = await EvaluateReadAsync<BorderIdentifier, Border>(review, AllUsers);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[User1], Is.False);
            Assert.That(results[User2], Is.False);
            Assert.That(results[User3], Is.False);
            Assert.That(results[Admin1], Is.True);
            Assert.That(results[Admin2], Is.True);
            Assert.That(results[Guest], Is.False);
        }
    }

    [Test]
    public async Task LoggedInRead_PublicRead_GroupOwnerAndLoggedInMustHaveAccess()
    {
        var review = ExampleCookieFactory.Example1;
        var results = await EvaluateReadAsync<CookieIdentifier, Cookie>(review, AllUsers);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[User1], Is.True);
            Assert.That(results[User2], Is.True);
            Assert.That(results[User3], Is.True);
            Assert.That(results[Admin1], Is.True);
            Assert.That(results[Admin2], Is.True);
            Assert.That(results[Guest], Is.False);
        }
    }

    [Test]
    public async Task LoggedInRead_PrivateRead_GroupOwnerMustHaveAccess()
    {
        var review = ExampleCookieFactory.Example2;
        var results = await EvaluateReadAsync<CookieIdentifier, Cookie>(review, AllUsers);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[User1], Is.False);
            Assert.That(results[User2], Is.True);
            Assert.That(results[User3], Is.False);
            Assert.That(results[Admin1], Is.False);
            Assert.That(results[Admin2], Is.False);
            Assert.That(results[Guest], Is.False);
        }
    }
}
