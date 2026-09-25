using Microsoft.Extensions.DependencyInjection;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.Core;

public static class ModuleBuilderExtensions
{
    public static ModuleBuilder<TTag> RegisterModule<TTag>(this IServiceCollection serviceCollection)
    {
        return new ModuleBuilder<TTag>(serviceCollection);
    }
}
