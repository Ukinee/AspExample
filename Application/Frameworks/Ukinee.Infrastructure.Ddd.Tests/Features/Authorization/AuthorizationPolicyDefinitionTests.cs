using Examples.Common.Domain.Models.Entities;
using Examples.Common.Domain.Models.Identifiers;
using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Ddd.Common.Authorization.Domain;
using Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.Local;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation.Contracts;
using Ukinee.Infrastructure.Ddd.Tests.Utils.Factories;
using Ukinee.Infrastructure.Ddd.Tests.Utils.TestBases;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Tests.Features.Authorization;

public class AuthorizationPolicyDefinitionTests : ReviewTestBase
{
    protected override void ConfigureTestServices(IServiceCollection services)
    {
        var reviewPolicy = AuthorizationPolicyDefinition.GuestReadAndOwnerEdit<ReviewIdentifier, Review>(identifier => identifier.ReviewUserGuid);

        var descriptors = AuthorizationHelper.GetDescriptors(reviewPolicy);

        foreach (var descriptor in descriptors)
        {
            services.Add(descriptor);
        }
    }

    [Test]
    public async Task GuestRead_PublicRead_AllUsersMustHaveAccess()
    {
        var readProvider = GetService<IEntityReadAccessExpressionProvider<ReviewIdentifier, Review>>();

        var publicReadReview = Example1;
        IEnumerable<UserContext> users = [UserFactory.User1, UserFactory.User2, UserFactory.UserAdmin, UserFactory.UserGuest];

        var results = new List<bool>();
        
        foreach (var userContext in users)
        {
            var expression = await readProvider.GetReadExpression(userContext);
            var compiledExpression = expression.Compile();
            
            results.Add(compiledExpression.Invoke(publicReadReview));
        }
        
        Assert.That(results, Has.All.True);
    }
    
    [Test]
    public async Task GuestRead_PrivateRead_OnlyOwnerMustHaveAccess()
    {
        var readProvider = GetService<IEntityReadAccessExpressionProvider<ReviewIdentifier, Review>>();

        var privateReadReview = Example2;
        IEnumerable<UserContext> users = [UserFactory.User1, UserFactory.User2, UserFactory.UserAdmin, UserFactory.UserGuest];

        var results = new List<bool>();
        
        foreach (var userContext in users)
        {
            var expression = await readProvider.GetReadExpression(userContext);
            var compiledExpression = expression.Compile();
            
            results.Add(compiledExpression.Invoke(privateReadReview));
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[0], Is.False);
            Assert.That(results[1], Is.True); // owner
            Assert.That(results[2], Is.False);
            Assert.That(results[3], Is.False);
        }
    }
    
    [Test]
    public async Task OwnerEdit_Update_OnlyOwnerMustHaveAccess()
    {
        var provider = GetService<IEntityUpdateAccessExpressionProvider<ReviewIdentifier, Review>>();

        var publicReadReview = Example2;
        IEnumerable<UserContext> users = [UserFactory.User1, UserFactory.User2, UserFactory.UserAdmin, UserFactory.UserGuest];

        var results = new List<bool>();
        
        foreach (var userContext in users)
        {
            var expression = await provider.GetUpdateExpression(userContext);
            var compiledExpression = expression.Compile();
            
            results.Add(compiledExpression.Invoke(publicReadReview));
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[0], Is.False);
            Assert.That(results[1], Is.True); // owner
            Assert.That(results[2], Is.False);
            Assert.That(results[3], Is.False);
        }
    }
    
    [Test]
    public async Task OwnerEdit_Delete_OnlyOwnerMustHaveAccess()
    {
        var provider = GetService<IEntityDeleteAccessExpressionProvider<ReviewIdentifier, Review>>();

        var publicReadReview = Example2;
        IEnumerable<UserContext> users = [UserFactory.User1, UserFactory.User2, UserFactory.UserAdmin, UserFactory.UserGuest];

        var results = new List<bool>();
        
        foreach (var userContext in users)
        {
            var expression = await provider.GetDeleteExpression(userContext);
            var compiledExpression = expression.Compile();
            
            results.Add(compiledExpression.Invoke(publicReadReview));
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[0], Is.False);
            Assert.That(results[1], Is.True); // owner
            Assert.That(results[2], Is.False);
            Assert.That(results[3], Is.False);
        }
    }
}
