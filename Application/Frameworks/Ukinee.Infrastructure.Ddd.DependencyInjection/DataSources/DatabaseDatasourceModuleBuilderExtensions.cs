using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Core;
using Ukinee.Infrastructure.Ddd.Local.EfCore.Services;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.DataSources;

public class DatabaseModuleDatasourceBuilder<TTag>(ModuleBuilder<TTag> module)
{
    public ModuleBuilder<TTag> Register<TContext>(Action<DbContextOptionsBuilder> configuration)
    where TContext : TaggedDbContext<TTag>
    {
        if (typeof(TContext).IsAbstract)
        {
            throw new ArgumentException("Cannot register abstract class.", nameof(TContext));
        }
        
        module.AddAction(collection => collection
            .AddScoped<TaggedDbContext<TTag>>(sp => sp.GetRequiredService<TContext>())
            .AddSingleton<IDbContextFactory<TaggedDbContext<TTag>>>(sp => new TaggedDbContextFactoryWrapper<TContext>(sp.GetRequiredService<IDbContextFactory<TContext>>()))
            .AddDbContextFactory<TContext>(configuration.Invoke)
        );

        return module;
    }

    private class TaggedDbContextFactoryWrapper<TContext>(IDbContextFactory<TContext> factory) : IDbContextFactory<TaggedDbContext<TTag>>
    where TContext : TaggedDbContext<TTag>
    {
        public TaggedDbContext<TTag> CreateDbContext()
        {
            return factory.CreateDbContext();
        }
    }
}

public static class DatabaseDatasourceModuleBuilderExtensions
{
    extension<TTag>(ModuleBuilder<TTag> module)
    {
        public DatabaseModuleDatasourceBuilder<TTag> DatabaseDatasource => new DatabaseModuleDatasourceBuilder<TTag>(module);
    }
}
