using Common.Infrastructure.Contracts;
using Microsoft.Extensions.Options;
using Ukinee.Infrastructure.Common.Models;

namespace Ukinee.Infrastructure.Common.Services;

public class ApplicationPathProviderService : IApplicationPathProviderService
{
    private readonly IOptions<ApplicationPathOptions> _options;

    public ApplicationPathProviderService(IOptions<ApplicationPathOptions> options)
    {
        _options = options;
    }

    private ApplicationPathOptions Options => _options.Value;

    public string GetRootPath()
    {
        return Options.GetAppDataFolder();
    }
}
