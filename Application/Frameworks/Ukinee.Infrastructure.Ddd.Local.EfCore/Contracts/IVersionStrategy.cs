using Microsoft.EntityFrameworkCore;
using Ukinee.Infrastructure.Ddd.Local.EfCore.Services;

namespace Ukinee.Infrastructure.Ddd.Local.EfCore.Contracts;

public interface IVersionStrategy<TTag>
{
    ValueTask<long> GetNextVersionAsync(DbContext context, CancellationToken cancellationToken);
}
