using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.Exceptions;
using Ukinee.Infrastructure.Ddd.Common.Specifications.Contracts;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;
using Ukinee.Users;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Common.Utils.Extensions;

public static class IIdentifierReaderExtensions
{
    extension<TIdentifier, TEntity>(IIdentifierReader<TIdentifier, TEntity> reader)
    where TIdentifier : notnull
    where TEntity : class, IEntity<TIdentifier>
    {
        public async Task<TEntity> GetByIdAsync(
            UserContext userContext,
            TIdentifier identifier,
            CancellationToken cancellationToken
        )
        {
            var result = await reader.FindByIdAsync(userContext, identifier, cancellationToken);

            if (result is null or IEntityWithSoftDelete<TEntity> { IsDeleted: true })
                throw new EntityNotFoundException<TIdentifier, TEntity>(identifier);

            return result;
        }

        public async Task<Dictionary<TIdentifier, TEntity>> FindManyByIdStrictAsync(
            UserContext userContext,
            IReadOnlyCollection<TIdentifier> identifiers,
            CancellationToken cancellationToken
        )
        {
            var response = await reader
                .FindManyByIdAsync(userContext, identifiers, cancellationToken)
                .ToDictionaryAsync(e => e.Identifier, cancellationToken: cancellationToken);

            if (response.Count != identifiers.Count)
            {
                var missingIds = identifiers.Except(response.Keys).AsCollection();

                throw new EntityNotFoundException<TIdentifier, TEntity>(missingIds);
            }

            return response;
        }
    }
}
