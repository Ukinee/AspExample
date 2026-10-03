using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using NUnit.Framework;
using Ukinee.Infrastructure.Ddd.Common.UnitOfWork.Contacts;
using Ukinee.Infrastructure.Ddd.DependencyInjection.DependenciesStartup;
using Ukinee.Infrastructure.Ddd.Tests.Common.Services;
using Ukinee.Infrastructure.Ddd.Tests.Common.Utils.Mocks;
using Ukinee.Infrastructure.Ddd.Tests.Common.Utils.TestBases;
using Ukinee.Infrastructure.Ddd.Tests.Domain;

namespace Ukinee.Infrastructure.Ddd.Tests.Integration.MediatRUnitOfWork;

public class MediatRUnitOfWorkTests : TestBase
{
    protected override void ConfigureTestServices(IServiceCollection services)
    {
        var mediatrOptions = new MediatRConfig {
            AssembliesToScanHandlers = [typeof(Program).Assembly]
        };

        services
            .AddLogging()
            .SetupUnitOfWork()
            .SetupMediatR(mediatrOptions);

        services.AddScoped<TestingNotificationHandler>();
        services.AddScoped<INotificationHandler<TestingNotification>, TestingNotificationHandler>((sp) => sp.GetRequiredService<TestingNotificationHandler>());
    }

    [Test]
    public async Task UnitOfWorkHoldsNotifications_And_CommitSendsNotifications()
    {
        var unitOfWorkFactory = GetService<IUnitOfWorkFactory>();
        var mediator = GetService<IMediator>();
        var handler = GetService<TestingNotificationHandler>();

        var notification1 = new TestingNotification {
            Id = Guid.NewGuid(),
        };

        var notification2 = new TestingNotification {
            Id = Guid.NewGuid(),
        };

        await using (var unitOfWork = unitOfWorkFactory.Create<TestingTag>())
        {
            await mediator.Publish(notification1);
            Assert.That(handler.Notifications, Is.Empty);

            await mediator.Publish(notification2);
            Assert.That(handler.Notifications, Is.Empty);

            await unitOfWork.CommitAsync(CancellationToken.None);
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(handler.Notifications, Has.Count.EqualTo(2));
            Assert.That(handler.Notifications[0].Id, Is.EqualTo(notification1.Id));
            Assert.That(handler.Notifications[1].Id, Is.EqualTo(notification2.Id));
        }
    }

    [Test]
    public async Task UnitOfWorkHoldsNotifications_And_RollbackVoidsNotifications()
    {
        var unitOfWorkFactory = GetService<IUnitOfWorkFactory>();
        var mediator = GetService<IMediator>();
        var handler = GetService<TestingNotificationHandler>();

        var notification1 = new TestingNotification {
            Id = Guid.NewGuid(),
        };

        var notification2 = new TestingNotification {
            Id = Guid.NewGuid(),
        };

        await using (var unitOfWork = unitOfWorkFactory.Create<TestingTag>())
        {
            await mediator.Publish(notification1);
            Assert.That(handler.Notifications, Is.Empty);

            await mediator.Publish(notification2);
            Assert.That(handler.Notifications, Is.Empty);

            await unitOfWork.RollbackAsync(CancellationToken.None);
        }

        Assert.That(handler.Notifications, Is.Empty);
    }
}
