using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Local.Repositories;

namespace Ukinee.Infrastructure.Ddd.Local.Extensions;

public static class RepositoryExtensions
{
    extension<TIdentifier, TEntity>(ITrackedRepository<TIdentifier, TEntity> repository)
    where TEntity : IEntity<TIdentifier>
    where TIdentifier : notnull
    {
        public async Task<(IReadOnlyDictionary<TIdentifier, TEntity> FoundEntities, IReadOnlyCollection<TIdentifier> MissingIdentifiers)> Separate(IEnumerable<TIdentifier> identifiers, CancellationToken cancellationToken)
        {
            var identifiersCollection = identifiers as ICollection<TIdentifier> ?? identifiers.ToList();

            var result = await repository
                .FindManyByIdAsync(identifiersCollection, cancellationToken)
                .ToDictionaryAsync(x => x.Identifier, cancellationToken: cancellationToken);

            var missing = identifiersCollection.Except(result.Keys);

            return (result, missing.ToList());
        }
    }
}
