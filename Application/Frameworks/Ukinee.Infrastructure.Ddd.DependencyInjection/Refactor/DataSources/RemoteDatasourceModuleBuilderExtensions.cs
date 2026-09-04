using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Core;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.DataSources;

public class RemoteModuleExtensionsBuilder<TTag>(ModuleBuilder<TTag> module)
{
    public ModuleBuilder<TTag> Register(Action<IHttpClientBuilder> httpClientBuilder)
    {
        module.AddAction(serviceCollection =>
            {
                var builder = serviceCollection.AddHttpClient(typeof(TTag).Name);

                httpClientBuilder.Invoke(builder);
            }
        );

        return module;
    }
}

public static class RemoteDatasourceModuleBuilderExtensions
{
    extension<TTag>(ModuleBuilder<TTag> module)
    {
        public RemoteModuleExtensionsBuilder<TTag> ApiDatasource => new RemoteModuleExtensionsBuilder<TTag>(module);
    }
}
