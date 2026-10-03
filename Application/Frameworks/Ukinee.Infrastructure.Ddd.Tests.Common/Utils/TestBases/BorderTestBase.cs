using System.Linq.Expressions;
using Ukinee.Infrastructure.Ddd.Local.Repositories;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Borders;

namespace Ukinee.Infrastructure.Ddd.Tests.Common.Utils.TestBases;

public abstract class BorderTestBase : TestBase
{
    protected IEditableTrackedRepository<BorderIdentifier, Border> Repository => GetService<IEditableTrackedRepository<BorderIdentifier, Border>>();

    protected static Border Example1 => ExampleBorderFactory.Example1;
    protected static BorderIdentifier Identifier1 => ExampleBorderFactory.Example1.Identifier;

    protected static Border Example2 => ExampleBorderFactory.Example2;
    protected static BorderIdentifier Identifier2 => ExampleBorderFactory.Example2.Identifier;

    protected static Border Example3 => ExampleBorderFactory.Example3;
    protected static BorderIdentifier Identifier3 => ExampleBorderFactory.Example3.Identifier;

    protected static CancellationToken CancellationToken => CancellationToken.None;

    protected static Expression<Func<Border, bool>> True => _ => true;
    protected static Expression<Func<Border, bool>> False => _ => false;
}
