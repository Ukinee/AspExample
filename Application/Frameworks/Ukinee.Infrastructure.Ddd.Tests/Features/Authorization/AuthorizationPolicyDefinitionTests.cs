using Examples.Common.Domain.Models.Entities;
using Examples.Common.Domain.Models.Identifiers;
using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Ddd.Common.Authorization.Domain;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.Local;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation.Contracts;
using Ukinee.Infrastructure.Ddd.Tests.Utils.Factories;
using Ukinee.Infrastructure.Ddd.Tests.Utils.Mocks;
using Ukinee.Infrastructure.Ddd.Tests.Utils.TestBases;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Tests.Features.Authorization;

public class AuthorizationPolicyDefinitionTests : ReviewTestBase
{
    private const int User1 = 0;
    private const int User2 = 1;
    private const int Admin = 2;
    private const int Guest = 3;
    
    private static readonly UserContext[] AllUsers = [UserFactory.User1, UserFactory.User2, UserFactory.UserAdmin, UserFactory.UserGuest];

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        var reviewPolicy = AuthorizationPolicyDefinition.GuestReadAndOwnerEdit<ReviewIdentifier, Review>(identifier => identifier.ReviewUserGuid);
        var locationPolicy = AuthorizationPolicyDefinition.GuestReadAndAdministratorEdit<LocationIdentifier, Location>();
        var locationCategoryPolicy = AuthorizationPolicyDefinition.AdministratorOnly<LocationCategoryIdentifier, LocationCategory>();
        var eventPolicy = AuthorizationPolicyDefinition.OwnerOnly<EventIdentifier, Event>(e => e.UserGuid);
        var attendancePolicy = AuthorizationPolicyDefinition.LoggedInReadAndAdministratorEdit<AnnouncementIdentifier, Announcement>();

        var reviewDescriptors = AuthorizationHelper.GetDescriptors(reviewPolicy);
        var locationDescriptors = AuthorizationHelper.GetDescriptors(locationPolicy);
        var locationCategoryDescriptors = AuthorizationHelper.GetDescriptors(locationCategoryPolicy);
        var eventPolicyDescriptors = AuthorizationHelper.GetDescriptors(eventPolicy);
        var attendanceDescriptors = AuthorizationHelper.GetDescriptors(attendancePolicy);

        var descriptors = reviewDescriptors
            .Concat(locationDescriptors)
            .Concat(locationCategoryDescriptors)
            .Concat(eventPolicyDescriptors)
            .Concat(attendanceDescriptors);

        foreach (var descriptor in descriptors)
        {
            services.Add(descriptor);
        }
    }

    [Test]
    public async Task GuestRead_PublicRead_AllUsersMustHaveAccess()
    {
        var provider = GetService<IEntityReadAccessExpressionProvider<ReviewIdentifier, Review>>();

        var review = Example1;
        var results = await EvaluateReadAsync(provider, review, AllUsers);

        Assert.That(results, Has.All.True);
    }

    [Test]
    public async Task GuestRead_PrivateRead_OnlyOwnerMustHaveAccess()
    {
        var provider = GetService<IEntityReadAccessExpressionProvider<ReviewIdentifier, Review>>();

        var review = Example2;
        var results = await EvaluateReadAsync(provider, review, AllUsers);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[User1], Is.False);
            Assert.That(results[User2], Is.True); // Owner
            Assert.That(results[Admin], Is.False);
            Assert.That(results[Guest], Is.False);
        }
    }

    [Test]
    public async Task OwnerEdit_Update_OnlyOwnerMustHaveAccess()
    {
        var provider = GetService<IEntityUpdateAccessExpressionProvider<ReviewIdentifier, Review>>();

        var review = Example2;
        var results = await EvaluateUpdateAsync(provider, review, AllUsers);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[User1], Is.False);
            Assert.That(results[User2], Is.True); // Owner
            Assert.That(results[Admin], Is.False);
            Assert.That(results[Guest], Is.False);
        }
    }

    [Test]
    public async Task OwnerEdit_Delete_OnlyOwnerMustHaveAccess()
    {
        var provider = GetService<IEntityDeleteAccessExpressionProvider<ReviewIdentifier, Review>>();

        var review = Example2;
        var results = await EvaluateDeleteAsync(provider, review, AllUsers);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[User1], Is.False);
            Assert.That(results[User2], Is.True); // Owner
            Assert.That(results[Admin], Is.False);
            Assert.That(results[Guest], Is.False);
        }
    }

    [Test]
    public async Task Location_GuestRead_PublicRead_AllUsersMustHaveAccess()
    {
        var readProvider = GetService<IEntityReadAccessExpressionProvider<LocationIdentifier, Location>>();

        var publicReadLocation = LocationFactory.Example1;
        var results = await EvaluateReadAsync(readProvider, publicReadLocation, AllUsers);

        Assert.That(results, Has.All.True);
    }

    [Test]
    public async Task Location_GuestRead_PrivateRead_OnlyAdministratorMustHaveAccess()
    {
        var readProvider = GetService<IEntityReadAccessExpressionProvider<LocationIdentifier, Location>>();

        var privateReadLocation = LocationFactory.Example2;
        var results = await EvaluateReadAsync(readProvider, privateReadLocation, AllUsers);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[User1], Is.False); 
            Assert.That(results[User2], Is.False); 
            Assert.That(results[Admin], Is.True); 
            Assert.That(results[Guest], Is.False); 
        }
    }

    [Test]
    public async Task Location_AdministratorEdit_Update_OnlyAdministratorMustHaveAccess()
    {
        var provider = GetService<IEntityUpdateAccessExpressionProvider<LocationIdentifier, Location>>();

        var location = LocationFactory.Example2;
        var results = await EvaluateUpdateAsync(provider, location, AllUsers);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[User1], Is.False);
            Assert.That(results[User2], Is.False);
            Assert.That(results[Admin], Is.True);
            Assert.That(results[Guest], Is.False);
        }
    }

    [Test]
    public async Task Location_AdministratorEdit_Delete_OnlyAdministratorMustHaveAccess()
    {
        var provider = GetService<IEntityDeleteAccessExpressionProvider<LocationIdentifier, Location>>();

        var location = LocationFactory.Example2;
        var results = await EvaluateDeleteAsync(provider, location, AllUsers);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[User1], Is.False);
            Assert.That(results[User2], Is.False);
            Assert.That(results[Admin], Is.True);
            Assert.That(results[Guest], Is.False);
        }
    }

    [Test]
    public async Task LocationCategory_AdministratorOnly_Read_OnlyAdministratorMustHaveAccess()
    {
        var readProvider = GetService<IEntityReadAccessExpressionProvider<LocationCategoryIdentifier, LocationCategory>>();

        var category = LocationCategoryFactory.Example1;
        var results = await EvaluateReadAsync(readProvider, category, AllUsers);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[User1], Is.False);
            Assert.That(results[User2], Is.False);
            Assert.That(results[Admin], Is.True);
            Assert.That(results[Guest], Is.False);
        }
    }

    [Test]
    public async Task LocationCategory_AdministratorOnly_Update_OnlyAdministratorMustHaveAccess()
    {
        var provider = GetService<IEntityUpdateAccessExpressionProvider<LocationCategoryIdentifier, LocationCategory>>();

        var category = LocationCategoryFactory.Example1;
        var results = await EvaluateUpdateAsync(provider, category, AllUsers);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[User1], Is.False);
            Assert.That(results[User2], Is.False);
            Assert.That(results[Admin], Is.True);
            Assert.That(results[Guest], Is.False);
        }
    }

    [Test]
    public async Task LocationCategory_AdministratorOnly_Delete_OnlyAdministratorMustHaveAccess()
    {
        var provider = GetService<IEntityDeleteAccessExpressionProvider<LocationCategoryIdentifier, LocationCategory>>();

        var category = LocationCategoryFactory.Example1;
        var results = await EvaluateDeleteAsync(provider, category, AllUsers);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[User1], Is.False);
            Assert.That(results[User2], Is.False);
            Assert.That(results[Admin], Is.True); 
            Assert.That(results[Guest], Is.False);
        }
    }

    [Test]
    public async Task Event_OwnerOnly_Read_OnlyOwnerMustHaveAccess()
    {
        var readProvider = GetService<IEntityReadAccessExpressionProvider<EventIdentifier, Event>>();

        var eventEntity = EventFactory.Example1;
        var results = await EvaluateReadAsync(readProvider, eventEntity, AllUsers);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[User1], Is.True); 
            Assert.That(results[User2], Is.False);
            Assert.That(results[Admin], Is.False);
            Assert.That(results[Guest], Is.False);
        }
    }

    [Test]
    public async Task Event_OwnerOnly_Update_OnlyOwnerMustHaveAccess()
    {
        var provider = GetService<IEntityUpdateAccessExpressionProvider<EventIdentifier, Event>>();

        var eventEntity = EventFactory.Example1; 
        var results = await EvaluateUpdateAsync(provider, eventEntity, AllUsers);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[User1], Is.True);
            Assert.That(results[User2], Is.False);
            Assert.That(results[Admin], Is.False);
            Assert.That(results[Guest], Is.False);
        }
    }

    [Test]
    public async Task Event_OwnerOnly_Delete_OnlyOwnerMustHaveAccess()
    {
        var provider = GetService<IEntityDeleteAccessExpressionProvider<EventIdentifier, Event>>();

        var eventEntity = EventFactory.Example1;
        var results = await EvaluateDeleteAsync(provider, eventEntity, AllUsers);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[User1], Is.True);
            Assert.That(results[User2], Is.False);
            Assert.That(results[Admin], Is.False);
            Assert.That(results[Guest], Is.False);
        }
    }

    [Test]
    public async Task Review_Create_OnlyLoggedInUsersMustHaveAccess()
    {
        var provider = GetService<IEntityCreateAccessExpressionProvider<ReviewIdentifier, Review>>();

        var entity = ReviewFactory.Example1;
        var results = await EvaluateCreateAsync(provider, entity, AllUsers);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[User1], Is.True);
            Assert.That(results[User2], Is.True);
            Assert.That(results[Admin], Is.True);
            Assert.That(results[Guest], Is.False);
        }
    }

    [Test]
    public async Task Location_Create_OnlyAdministratorMustHaveAccess()
    {
        var provider = GetService<IEntityCreateAccessExpressionProvider<LocationIdentifier, Location>>();

        var entity = LocationFactory.Example1;
        var results = await EvaluateCreateAsync(provider, entity, AllUsers);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[User1], Is.False);
            Assert.That(results[User2], Is.False);
            Assert.That(results[Admin], Is.True);
            Assert.That(results[Guest], Is.False); 
        }
    }

    [Test]
    public async Task LocationCategory_Create_OnlyAdministratorMustHaveAccess()
    {
        var provider = GetService<IEntityCreateAccessExpressionProvider<LocationCategoryIdentifier, LocationCategory>>();

        var entity = LocationCategoryFactory.Example1;
        var results = await EvaluateCreateAsync(provider, entity, AllUsers);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[User1], Is.False);
            Assert.That(results[User2], Is.False);
            Assert.That(results[Admin], Is.True); 
            Assert.That(results[Guest], Is.False);
        }
    }

    [Test]
    public async Task Event_Create_OnlyLoggedInUsersMustHaveAccess()
    {
        var provider = GetService<IEntityCreateAccessExpressionProvider<EventIdentifier, Event>>();

        var entity = EventFactory.Example1;
        var results = await EvaluateCreateAsync(provider, entity, AllUsers);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[User1], Is.True);
            Assert.That(results[User2], Is.True);
            Assert.That(results[Admin], Is.True);
            Assert.That(results[Guest], Is.False); 
        }
    }

    [Test]
    public async Task Attendance_LoggedInRead_PublicRead_AllLoggedInMustHaveAccess_GuestMustNot()
    {
        var readProvider = GetService<IEntityReadAccessExpressionProvider<AnnouncementIdentifier, Announcement>>();

        var attendance = AttendanceFactory.Example1;
        var results = await EvaluateReadAsync(readProvider, attendance, AllUsers);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[User1], Is.True);
            Assert.That(results[User2], Is.True);
            Assert.That(results[Admin], Is.True);
            Assert.That(results[Guest], Is.False);
        }
    }
    
    [Test]
    public async Task Attendance_LoggedInRead_PrivateRead_AllLoggedInMustHaveAccess_GuestMustNot()
    {
        var readProvider = GetService<IEntityReadAccessExpressionProvider<AnnouncementIdentifier, Announcement>>();

        var attendance = AttendanceFactory.Example2;
        var results = await EvaluateReadAsync(readProvider, attendance, AllUsers);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[User1], Is.False);
            Assert.That(results[User2], Is.False);
            Assert.That(results[Admin], Is.True);
            Assert.That(results[Guest], Is.False);
        }
    }

    [Test]
    public async Task Attendance_Create_OnlyAdministratorMustHaveAccess()
    {
        var provider = GetService<IEntityCreateAccessExpressionProvider<AnnouncementIdentifier, Announcement>>();

        var attendance = AttendanceFactory.Example1;
        var results = await EvaluateCreateAsync(provider, attendance, AllUsers);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[User1], Is.False);
            Assert.That(results[User2], Is.False);
            Assert.That(results[Admin], Is.True);
            Assert.That(results[Guest], Is.False);
        }
    }

    [Test]
    public async Task Attendance_AdministratorEdit_Update_OnlyAdministratorMustHaveAccess()
    {
        var provider = GetService<IEntityUpdateAccessExpressionProvider<AnnouncementIdentifier, Announcement>>();

        var attendance = AttendanceFactory.Example1;
        var results = await EvaluateUpdateAsync(provider, attendance, AllUsers);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[User1], Is.False);
            Assert.That(results[User2], Is.False);
            Assert.That(results[Admin], Is.True);
            Assert.That(results[Guest], Is.False);
        }
    }

    [Test]
    public async Task Attendance_AdministratorEdit_Delete_OnlyAdministratorMustHaveAccess()
    {
        var provider = GetService<IEntityDeleteAccessExpressionProvider<AnnouncementIdentifier, Announcement>>();

        var attendance = AttendanceFactory.Example1;
        var results = await EvaluateDeleteAsync(provider, attendance, AllUsers);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[User1], Is.False);
            Assert.That(results[User2], Is.False);
            Assert.That(results[Admin], Is.True);
            Assert.That(results[Guest], Is.False);
        }
    }

    private static async Task<List<bool>> EvaluateReadAsync<TIdentifier, TEntity>(
        IEntityReadAccessExpressionProvider<TIdentifier, TEntity> provider,
        TEntity entity,
        IEnumerable<UserContext> users
    )
    where TEntity : IEntity<TIdentifier>
    {
        var results = new List<bool>();

        foreach (var userContext in users)
        {
            var expression = await provider.GetReadExpression(userContext);
            var compiledExpression = expression.Compile();
            results.Add(compiledExpression.Invoke(entity));
        }

        return results;
    }

    private static async Task<List<bool>> EvaluateUpdateAsync<TIdentifier, TEntity>(
        IEntityUpdateAccessExpressionProvider<TIdentifier, TEntity> provider,
        TEntity entity,
        IEnumerable<UserContext> users
    )
    where TEntity : IEntity<TIdentifier>
    {
        var results = new List<bool>();

        foreach (var userContext in users)
        {
            var expression = await provider.GetUpdateExpression(userContext);
            var compiledExpression = expression.Compile();
            results.Add(compiledExpression.Invoke(entity));
        }

        return results;
    }

    private static async Task<List<bool>> EvaluateDeleteAsync<TIdentifier, TEntity>(
        IEntityDeleteAccessExpressionProvider<TIdentifier, TEntity> provider,
        TEntity entity,
        IEnumerable<UserContext> users
    )
    where TEntity : IEntity<TIdentifier>
    {
        var results = new List<bool>();

        foreach (var userContext in users)
        {
            var expression = await provider.GetDeleteExpression(userContext);
            var compiledExpression = expression.Compile();
            results.Add(compiledExpression.Invoke(entity));
        }

        return results;
    }

    private static async Task<List<bool>> EvaluateCreateAsync<TIdentifier, TEntity>(
        IEntityCreateAccessExpressionProvider<TIdentifier, TEntity> provider,
        TEntity entity,
        IEnumerable<UserContext> users
    )
    where TEntity : IEntity<TIdentifier>
    {
        var results = new List<bool>();

        foreach (var userContext in users)
        {
            var expression = await provider.GetCreateExpression(userContext);
            var compiledExpression = expression.Compile();
            results.Add(compiledExpression.Invoke(entity));
        }

        return results;
    }
}
