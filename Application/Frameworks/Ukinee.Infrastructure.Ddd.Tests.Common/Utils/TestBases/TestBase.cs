using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ukinee.Infrastructure.Ddd.Tests.Common.Utils.TestBases;

public abstract class TestBase
{
    protected ServiceProvider Provider = null!;
    protected ConfigurationManager Configuration = null!;
    protected IServiceCollection Services = null!;

    [SetUp]
    public virtual void Setup()
    {
        Services = new ServiceCollection();
        Configuration = new ConfigurationManager();

        ConfigureTestServices(Services);

        Provider = Services.BuildServiceProvider();
    }

    [TearDown]
    public void TearDown()
    {
        Configuration.Dispose();
        Provider.Dispose();
    }

    protected abstract void ConfigureTestServices(IServiceCollection services);

    protected T GetService<T>()
    where T : notnull =>
        Provider.GetRequiredService<T>();
}
