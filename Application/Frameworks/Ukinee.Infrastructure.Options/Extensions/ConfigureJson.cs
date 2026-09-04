using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ukinee.Infrastructure.Options.Extensions;

public static partial class OptionsInfrastructureServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection ConfigureJson<T>(IConfigurationManager configuration, string sectionName, string jsonFilePath) where T : class
        {
            configuration.AddJsonFile(jsonFilePath, optional: false, reloadOnChange: true);

            var configurationSection = configuration.GetSection(sectionName);

            return services.Configure<T>(configurationSection);
        }
    }
}
