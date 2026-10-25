using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Core;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Utils;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.DataSources.Database;

public class DatabaseDataSourceDefinition : IBuildable
{
    public List<ServiceDescriptor> Versioning { get; } = [];
    public List<Action<IServiceProvider, DbContextOptionsBuilder>> Configurations { get; } = [];
    public List<Action<IServiceCollection>> Extensions { get; } = [];

    public void Build(RegistrationPolicy policy, IServiceCollection serviceCollection)
    {
        PayloadHelper.AddFiltered(serviceCollection, policy, Versioning);

        foreach (var extension in Extensions)
        {
            extension.Invoke(serviceCollection);
        }
    }
}
