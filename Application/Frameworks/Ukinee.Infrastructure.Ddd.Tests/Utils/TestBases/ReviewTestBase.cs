using System.Linq.Expressions;
using Examples.Common.Domain.Models.Entities;
using Examples.Common.Domain.Models.Identifiers;
using Ukinee.Infrastructure.Ddd.Local.Repositories;
using Ukinee.Infrastructure.Ddd.Tests.Utils.Factories;

namespace Ukinee.Infrastructure.Ddd.Tests.Utils.TestBases;

public abstract class ReviewTestBase : TestBase
{
    protected IEditableTrackedRepository<ReviewIdentifier, Review> Repository => GetService<IEditableTrackedRepository<ReviewIdentifier, Review>>();

    protected static Review Example1 => ReviewFactory.Example1;
    protected static ReviewIdentifier Identifier1 => ReviewFactory.Example1.Identifier;

    protected static Review Example2 => ReviewFactory.Example2;
    protected static ReviewIdentifier Identifier2 => ReviewFactory.Example2.Identifier;

    protected static Review Example3 => ReviewFactory.Example3;
    protected static ReviewIdentifier Identifier3 => ReviewFactory.Example3.Identifier;

    protected static CancellationToken CancellationToken => CancellationToken.None;

    protected static Expression<Func<Review, bool>> True => _ => true;
    protected static Expression<Func<Review, bool>> False => _ => false;
}
