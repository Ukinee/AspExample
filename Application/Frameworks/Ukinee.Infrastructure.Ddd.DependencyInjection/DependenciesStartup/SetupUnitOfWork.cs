using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Ddd.Common.UnitOfWork.Contacts;
using Ukinee.Infrastructure.Ddd.Common.UnitOfWork.Implementations;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.DependenciesStartup;

public static class SetupUnitOfWorkExtension
{
    public static IServiceCollection SetupUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped<UnitOfWorkFactory>();
        services.AddScoped<IUnitOfWorkProvider>(sp => sp.GetRequiredService<UnitOfWorkFactory>());
        services.AddScoped<IUnitOfWorkFactory>(sp => sp.GetRequiredService<UnitOfWorkFactory>());
        
        return services;
    }
}
