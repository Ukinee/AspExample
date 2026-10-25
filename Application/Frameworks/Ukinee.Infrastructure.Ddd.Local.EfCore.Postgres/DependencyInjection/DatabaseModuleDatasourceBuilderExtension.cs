using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Ddd.DependencyInjection.DataSources.Database;
using Ukinee.Infrastructure.Ddd.Local.EfCore.Postgres.Services;
using Ukinee.Infrastructure.Ddd.Local.EfCore.Services;

namespace Ukinee.Infrastructure.Ddd.Local.EfCore.Postgres.DependencyInjection;

public static class PostgresDatabaseDatasourceModuleBuilderExtensions
{
    extension<TTag, TContext>(DatabaseDataSourceFactory<TTag, TContext> factory)
    where TContext : TaggedDbContext<TContext, TTag>
    {
        public DatabaseDataSourceFactory<TTag, TContext> RegisterPostgresVersioning()
        {
            factory.SetVersioning<PostgresVersionStrategy<TTag>>();

            factory.ConfigureContext((sp, builder) =>
                {
                    var interceptor = sp.GetRequiredService<SaveChangesVersionInterceptor<TTag>>();

                    builder.AddInterceptors(interceptor);
                }
            );

            return factory;
        }
    }
}
