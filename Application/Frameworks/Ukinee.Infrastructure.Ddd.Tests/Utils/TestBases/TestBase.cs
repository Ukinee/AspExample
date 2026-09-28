using Microsoft.Extensions.DependencyInjection;

namespace Ukinee.Infrastructure.Ddd.Tests.Utils.TestBases;

public abstract class TestBase
{
    protected ServiceProvider Provider = null!;
    protected IServiceCollection Services = null!;

    [SetUp]
    public virtual void Setup()
    {
        Services = new ServiceCollection();

        ConfigureTestServices(Services);

        Provider = Services.BuildServiceProvider();
    }

    [TearDown]
    public void TearDown()
    {
        Provider.Dispose();
    }

    protected abstract void ConfigureTestServices(IServiceCollection services);

    protected T GetService<T>()
    where T : notnull =>
        Provider.GetRequiredService<T>();
}
