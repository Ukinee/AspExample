using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation.Contracts;
using Ukinee.Infrastructure.Ddd.Tests.Common.Utils.TestBases;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Users;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Tests.Features.Authorization;

public abstract class AuthorizationPolicyDefinitionTestBase : TestBase
{
    protected const int User1 = 0;
    protected const int User2 = 1;
    protected const int User3 = 2;
    protected const int Admin1 = 3;
    protected const int Admin2 = 4;
    protected const int Guest = 5;

    protected static readonly UserContext[] AllUsers = [
        ExampleUserFactory.User1,
        ExampleUserFactory.User2,
        ExampleUserFactory.User3,
        ExampleUserFactory.UserAdmin1,
        ExampleUserFactory.UserAdmin2,
        ExampleUserFactory.UserGuest,
    ];

    protected async Task<List<bool>> EvaluateReadAsync<TIdentifier, TEntity>(
        TEntity entity,
        IEnumerable<UserContext> users
    )
    where TEntity : IEntity<TIdentifier>
    {
        var provider = GetService<IEntityReadAccessExpressionProvider<TIdentifier, TEntity>>();

        var results = new List<bool>();

        foreach (var userContext in users)
        {
            var expression = await provider.GetReadExpression(userContext);
            var compiledExpression = expression.Compile();
            results.Add(compiledExpression.Invoke(entity));
        }

        return results;
    }

    protected async Task<List<bool>> EvaluateUpdateAsync<TIdentifier, TEntity>(
        TEntity entity,
        IEnumerable<UserContext> users
    )
    where TEntity : IEntity<TIdentifier>
    {
        var provider = GetService<IEntityUpdateAccessExpressionProvider<TIdentifier, TEntity>>();

        var results = new List<bool>();

        foreach (var userContext in users)
        {
            var expression = await provider.GetUpdateExpression(userContext);
            var compiledExpression = expression.Compile();
            results.Add(compiledExpression.Invoke(entity));
        }

        return results;
    }

    protected async Task<List<bool>> EvaluateDeleteAsync<TIdentifier, TEntity>(
        TEntity entity,
        IEnumerable<UserContext> users
    )
    where TEntity : IEntity<TIdentifier>
    {
        var provider = GetService<IEntityDeleteAccessExpressionProvider<TIdentifier, TEntity>>();

        var results = new List<bool>();

        foreach (var userContext in users)
        {
            var expression = await provider.GetDeleteExpression(userContext);
            var compiledExpression = expression.Compile();
            results.Add(compiledExpression.Invoke(entity));
        }

        return results;
    }

    protected async Task<List<bool>> EvaluateCreateAsync<TIdentifier, TEntity>(
        TEntity entity,
        IEnumerable<UserContext> users
    )
    where TEntity : IEntity<TIdentifier>
    {
        var provider = GetService<IEntityCreateAccessExpressionProvider<TIdentifier, TEntity>>();

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
