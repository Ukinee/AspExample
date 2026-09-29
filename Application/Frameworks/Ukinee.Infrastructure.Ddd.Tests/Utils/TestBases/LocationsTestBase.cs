using System.Linq.Expressions;
using Examples.Common.Domain.Models.Entities;
using Examples.Common.Domain.Models.Identifiers;
using Ukinee.Infrastructure.Ddd.Local.InMemory.Repositories;
using Ukinee.Infrastructure.Ddd.Local.Repositories;
using Ukinee.Infrastructure.Ddd.Tests.Utils.Factories;

namespace Ukinee.Infrastructure.Ddd.Tests.Utils.TestBases;

public abstract class LocationsTestBase : TestBase
{
    protected IEditableTrackedRepository<LocationIdentifier, Location> Repository => GetService<IEditableTrackedRepository<LocationIdentifier, Location>>();
    protected InMemoryStore<LocationIdentifier, Location> Store => GetService<InMemoryStore<LocationIdentifier, Location>>();

    protected static Location Example1 => LocationFactory.Example1;
    protected static LocationIdentifier Identifier1 => LocationFactory.Example1.Identifier;

    protected static Location Example2 => LocationFactory.Example2;
    protected static LocationIdentifier Identifier2 => LocationFactory.Example2.Identifier;

    protected static Location Example3 => LocationFactory.Example3;
    protected static LocationIdentifier Identifier3 => LocationFactory.Example3.Identifier;

    protected static CancellationToken CancellationToken => CancellationToken.None;

    protected static Expression<Func<Location, bool>> True => _ => true;
    protected static Expression<Func<Location, bool>> False => _ => false;
}
