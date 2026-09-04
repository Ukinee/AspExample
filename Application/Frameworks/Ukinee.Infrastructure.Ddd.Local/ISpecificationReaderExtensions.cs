using Ardalis.Specification;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.Exceptions;
using Ukinee.Infrastructure.Ddd.Common.Specifications.Contracts;
using Ukinee.Infrastructure.Ddd.Local.UseCaseServices.Contracts;
using Ukinee.Users.Domain;

namespace Ukinee.Infrastructure.Ddd.Local;

public static class ISpecificationReaderExtensions
{
    extension<TIdentifier, TEntity>(ISpecificationReader<TIdentifier, TEntity> reader)
    where TIdentifier : notnull
    where TEntity : class, IEntity<TIdentifier>
    {
        public async Task<TEntity> GetAsync(
            UserContext userContext,
            Specification<TEntity> specification,
            CancellationToken cancellationToken
        )
        {
            var result = await reader.FindAsync(userContext, specification, cancellationToken);

            if (result is null or ISpecificationForSoftDelete<TEntity> { IsDeleted: true })
                throw new EntityNotFoundException<TIdentifier, TEntity>();

            return result;
        }
    }
}
