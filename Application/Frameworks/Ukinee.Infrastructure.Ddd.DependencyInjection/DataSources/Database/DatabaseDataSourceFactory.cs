using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Utils;
using Ukinee.Infrastructure.Ddd.Local.EfCore.Contracts;
using Ukinee.Infrastructure.Ddd.Local.EfCore.Services;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.DataSources.Database;

public class DatabaseDataSourceFactory<TTag, TContext>
{
    private ServiceLifetime _serviceLifetime = ServiceLifetime.Scoped;
    
    public DatabaseDataSourceFactory(DatabaseDataSourceDefinition feature)
    {
        Feature = feature;
    }

    public DatabaseDataSourceDefinition Feature { get; }

    public DatabaseDataSourceFactory<TTag, TContext> ConfigureContext(Action<IServiceProvider, DbContextOptionsBuilder> configuration)
    {
        Feature.Configurations.Add(configuration);

        return this;
    }

    public DatabaseDataSourceFactory<TTag, TContext> SetVersioning<T>()
    where T : class, IVersionStrategy<TTag>
    {
        Feature.Versioning.Clear();

        IEnumerable<ServiceDescriptor> descriptors = [
            ..PayloadHelper.Service<IVersionStrategy<TTag>, T>(_serviceLifetime),
            ..PayloadHelper.Service<SaveChangesVersionInterceptor<TTag>>(_serviceLifetime),
        ];
        
        Feature.Versioning.AddRange(descriptors);

        return this;
    }
}
