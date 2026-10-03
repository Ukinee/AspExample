using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Ukinee.Infrastructure.Ddd.Common.Authorization.Domain;
using Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.Local;
using Ukinee.Infrastructure.Ddd.Tests.Domain;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Announcements;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Borders;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Cookies;

namespace Ukinee.Infrastructure.Ddd.Tests.Features.Authorization;

public class OwnerAuthorizationPolicyDefinitionTests : AuthorizationPolicyDefinitionTestBase
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
    public async Task Create_LoggedIn_MustHaveAccess()
    {
        var review = ExampleAnnouncementFactory.Example1;
        var results = await EvaluateCreateAsync<AnnouncementIdentifier, Announcement>(review, AllUsers);

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
    public async Task Update_Owner_MustHaveAccess()
    {
        var review = ExampleAnnouncementFactory.Example1;
        var results = await EvaluateUpdateAsync<AnnouncementIdentifier, Announcement>(review, AllUsers);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[User1], Is.True);
            Assert.That(results[User2], Is.False);
            Assert.That(results[User3], Is.False);
            Assert.That(results[Admin1], Is.False);
            Assert.That(results[Admin2], Is.False);
            Assert.That(results[Guest], Is.False);
        }
    }

    [Test]
    public async Task Delete_Owner_MustHaveAccess()
    {
        var review = ExampleAnnouncementFactory.Example1;
        var results = await EvaluateDeleteAsync<AnnouncementIdentifier, Announcement>(review, AllUsers);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[User1], Is.True);
            Assert.That(results[User2], Is.False);
            Assert.That(results[User3], Is.False);
            Assert.That(results[Admin1], Is.False);
            Assert.That(results[Admin2], Is.False);
            Assert.That(results[Guest], Is.False);
        }
    }

    [Test]
    public async Task Create_Group_MustHaveAccess()
    {
        var review = ExampleBorderFactory.Example1;
        var results = await EvaluateCreateAsync<BorderIdentifier, Border>(review, AllUsers);

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
    public async Task Update_Group_MustHaveAccess()
    {
        var review = ExampleBorderFactory.Example1;
        var results = await EvaluateUpdateAsync<BorderIdentifier, Border>(review, AllUsers);

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
    public async Task Delete_Group_MustHaveAccess()
    {
        var review = ExampleBorderFactory.Example2;
        var results = await EvaluateDeleteAsync<BorderIdentifier, Border>(review, AllUsers);

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
    public async Task Create_GroupOwner_MustHaveAccess()
    {
        var review = ExampleCookieFactory.Example1;
        var results = await EvaluateCreateAsync<CookieIdentifier, Cookie>(review, AllUsers);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[User1], Is.False);
            Assert.That(results[User2], Is.True);
            Assert.That(results[User3], Is.True);
            Assert.That(results[Admin1], Is.False);
            Assert.That(results[Admin2], Is.False);
            Assert.That(results[Guest], Is.False);
        }
    }

    [Test]
    public async Task Update_GroupOwner_MustHaveAccess()
    {
        var review = ExampleCookieFactory.Example3;
        var results = await EvaluateUpdateAsync<CookieIdentifier, Cookie>(review, AllUsers);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[User1], Is.False);
            Assert.That(results[User2], Is.False);
            Assert.That(results[User3], Is.True);
            Assert.That(results[Admin1], Is.False);
            Assert.That(results[Admin2], Is.False);
            Assert.That(results[Guest], Is.False);
        }
    }

    [Test]
    public async Task Delete_GroupOwner_MustHaveAccess()
    {
        var review = ExampleCookieFactory.Example3;
        var results = await EvaluateDeleteAsync<CookieIdentifier, Cookie>(review, AllUsers);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[User1], Is.False);
            Assert.That(results[User2], Is.False);
            Assert.That(results[User3], Is.True);
            Assert.That(results[Admin1], Is.False);
            Assert.That(results[Admin2], Is.False);
            Assert.That(results[Guest], Is.False);
        }
    }
}
