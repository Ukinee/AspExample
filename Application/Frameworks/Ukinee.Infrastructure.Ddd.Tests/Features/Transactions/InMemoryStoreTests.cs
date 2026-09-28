using Examples.Common.Domain.Models.Entities;
using Examples.Common.Domain.Models.Identifiers;
using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Ddd.Local.InMemory.Repositories;
using Ukinee.Infrastructure.Ddd.Tests.Utils.Factories;
using Ukinee.Infrastructure.Ddd.Tests.Utils.TestBases;

namespace Ukinee.Infrastructure.Ddd.Tests.Features.Transactions;

public class InMemoryStoreTests : TestBase
{
    protected override void ConfigureTestServices(IServiceCollection services)
    {
        services.AddSingleton<InMemoryStore<LocationIdentifier, Location>>();
    }

#region Positive
    [Test]
    public void AddExists_ShouldReturnFalseBeforeAndTrueAfter()
    {
        var store = GetService<InMemoryStore<LocationIdentifier, Location>>();
        var entity = LocationFactory.Example1;

        var existsBefore = store.Exists(entity.Identifier);
        store.AddRange([entity]);
        var existsAfter = store.Exists(entity.Identifier);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(existsBefore, Is.False);
            Assert.That(existsAfter, Is.True);
        }
    }
    
    [Test]
    public void RemoveExists_ShouldRemoveEntity_ExistsReturnsTrueBeforeAndFalseAfter()
    {
        var store = GetService<InMemoryStore<LocationIdentifier, Location>>();
        var entity = LocationFactory.Example1;
        store.AddRange([entity]);

        var existsBefore = store.Exists(entity.Identifier);
        store.RemoveRange([entity.Identifier], _ => true);
        var existsAfter = store.Exists(entity.Identifier);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(existsBefore, Is.True);
            Assert.That(existsAfter, Is.False);
        }
    }
#endregion
}
