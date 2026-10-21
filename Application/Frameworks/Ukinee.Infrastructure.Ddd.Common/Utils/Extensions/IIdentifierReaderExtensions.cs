using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.EntityFeatures.Contracts;
using Ukinee.Infrastructure.Ddd.Common.Exceptions;
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

            if (result is null)
                throw new EntityNotFoundException<TIdentifier, TEntity>(identifier);

            return result;
        }
    }
}
