using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Users;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Common.UseCases.Contracts;

public interface IGetEntityUseCase<TIdentifier, TEntity>
where TEntity : IEntity<TIdentifier>
where TIdentifier : struct
{
    public Task<TEntity> Execute(UserContext userContext, TIdentifier identifier, CancellationToken cancellationToken);
    public Task<TEntity?> Execute(UserContext userContext, TIdentifier? identifier, CancellationToken cancellationToken);
    public IAsyncEnumerable<TEntity> ExecuteSoft(UserContext userContext, IReadOnlyCollection<TIdentifier> identifiers, CancellationToken cancellationToken);
    public Task<Dictionary<TIdentifier, TEntity>> ExecuteStrict(UserContext userContext, IReadOnlyCollection<TIdentifier> identifiers, CancellationToken cancellationToken);
}

public interface IGetAllEntityUseCase<TIdentifier, out TEntity>
where TEntity : IEntity<TIdentifier>
where TIdentifier : struct
{
    public IAsyncEnumerable<TEntity> ExecuteAll(UserContext userContext, CancellationToken cancellationToken);
}
