using MediatR;

namespace Ukinee.Infrastructure.Ddd.Tests.Common.Services;

public record TestingNotification : INotification
{
    public required Guid Id { get; init; }
}

public class TestingNotificationHandler : INotificationHandler<TestingNotification>
{
    public readonly List<TestingNotification> Notifications = [];

    public Task Handle(TestingNotification notification, CancellationToken cancellationToken)
    {
        Notifications.Add(notification);

        return Task.CompletedTask;
    }
}
