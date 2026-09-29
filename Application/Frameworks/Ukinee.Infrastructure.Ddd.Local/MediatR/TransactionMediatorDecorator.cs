using MediatR;
using Ukinee.Infrastructure.Ddd.Common.UnitOfWork.Contacts;

namespace Ukinee.Infrastructure.Ddd.Local.MediatR;

public class TransactionMediatorDecorator(
    IUnitOfWorkProvider ouwProvider,
    Mediator mediatorImplementation
) : IMediator
{
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = new CancellationToken())
    {
        return await mediatorImplementation.Send(request, cancellationToken);
    }

    public async Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = new CancellationToken())
    where TRequest : IRequest
    {
        await mediatorImplementation.Send(request, cancellationToken);
    }

    public async Task<object?> Send(object request, CancellationToken cancellationToken = new CancellationToken())
    {
        return await mediatorImplementation.Send(request, cancellationToken);
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = new CancellationToken())
    {
        return mediatorImplementation.CreateStream(request, cancellationToken);
    }

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = new CancellationToken())
    {
        return mediatorImplementation.CreateStream(request, cancellationToken);
    }

    public Task Publish(object notification, CancellationToken cancellationToken = new CancellationToken())
    {
        return notification switch {
            null => throw new ArgumentNullException(nameof(notification)),
            INotification instance => Publish(instance, cancellationToken),
            _ => throw new ArgumentException($"{nameof(notification)} does not implement ${nameof(INotification)}")
        };
    }

    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = new CancellationToken())
    where TNotification : INotification
    {
        var uow = ouwProvider.Current;

        if (uow == null)
        {
            await mediatorImplementation.Publish(notification, cancellationToken);

            return;
        }

        uow.RegisterNotification(notification);
    }
}
