using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Ukinee.DbAccess.Extensions;
using Ukinee.Infrastructure.Ddd.Common.Specifications.Extensions;
using Ukinee.Infrastructure.Ddd.Common.UnitOfWork.Contacts;
using Ukinee.Infrastructure.Ddd.Local.Repositories;
using Ukinee.Infrastructure.Ddd.Tests.Common.Services.Repositories;
using Ukinee.Infrastructure.Ddd.Tests.Common.Utils.Mocks;
using Ukinee.Infrastructure.Ddd.Tests.Common.Utils.TestBases;
using Ukinee.Infrastructure.Ddd.Tests.Domain;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Borders;

namespace Ukinee.Infrastructure.Ddd.Tests.Features.Repositories.Specifications;

public class ExpressionsTests : BorderTestBase
{
    protected override void ConfigureTestServices(IServiceCollection services)
    {
        services.AddSingleton<InMemoryStore<BorderIdentifier, Border>>();
        services.AddSingleton<IEditableTrackedRepository<BorderIdentifier, Border>, TransactedInMemoryRepository<BorderIdentifier, Border, TestingTag>>();

        services.AddSingleton<UnitOfWorkFactoryMock>();
        services.AddSingleton<IUnitOfWorkProvider>(sp => sp.GetRequiredService<UnitOfWorkFactoryMock>());
        services.AddSingleton<IUnitOfWorkFactory>(sp => sp.GetRequiredService<UnitOfWorkFactoryMock>());
    }

    [Test]
    public void WhereIdIn()
    {
        IEnumerable<BorderIdentifier> identifiers = [Identifier1, Identifier2];
        var specification = Specification.For<Border>().WhereIdIn(identifiers).Specification;

        var result1 = specification.IsSatisfiedBy(Example1);
        var result2 = specification.IsSatisfiedBy(Example2);
        var result3 = specification.IsSatisfiedBy(Example3);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result1, Is.True);
            Assert.That(result2, Is.True);
            Assert.That(result3, Is.False);
        }
    }

    [Test]
    public void WhereIdEquals()
    {
        IEnumerable<Border> entities1 = [Example1, Example2, Example3];
        IEnumerable<Border> entities2 = [Example1, Example3];
        IEnumerable<Border> entities3 = [];

        var specification = Specification.For<Border>().WhereIdEquals(Identifier2).Specification;

        var result1 = specification.Evaluate(entities1).ToList();
        var result2 = specification.Evaluate(entities2).ToList();
        var result3 = specification.Evaluate(entities3).ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result1, Has.Count.EqualTo(1));
            Assert.That(result2, Has.Count.EqualTo(0));
            Assert.That(result3, Has.Count.EqualTo(0));

            Assert.That(result1.Single(), Is.EqualTo(Example2));
        }
    }
}
