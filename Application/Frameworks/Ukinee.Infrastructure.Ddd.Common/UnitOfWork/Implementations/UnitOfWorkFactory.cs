using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Ukinee.Infrastructure.Ddd.Common.UnitOfWork.Contacts;
using Ukinee.Infrastructure.Ddd.Common.UnitOfWork.Domain;

namespace Ukinee.Infrastructure.Ddd.Common.UnitOfWork.Implementations;

public class UnitOfWorkFactory(IServiceProvider serviceProvider) : IUnitOfWorkFactory, IUnitOfWorkProvider
{
    private AmbientUnitOfWork? _current;

    public IEditableUnitOfWork? Current => _current;

    public IUnitOfWork Create<TTag>()
    {
        if (Current is not null)
            throw new InvalidOperationException("Active transaction already exists.");

        var unitOfWork = AmbientUnitOfWork.Create<TTag>(Commit, Rollback);

        _current = unitOfWork;

        return unitOfWork;
    }

    private async Task Commit(AmbientUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        try
        {
            foreach (var part in unitOfWork.Parts)
                await part.PrepareToCommitAsync(cancellationToken);

            foreach (var part in unitOfWork.Parts.OrderBy(p => p.IsImportant))
                await part.CommitAsync(cancellationToken);
        }
        catch
        {
            foreach (var part in unitOfWork.Parts.OrderByDescending(p => p.IsImportant))
            {
                try
                {
                    await part.TryRollbackPartialCommitAsync(cancellationToken);
                }
                catch
                { /* log */
                }

                try
                {
                    await part.RollbackAsync(cancellationToken);
                }
                catch
                { /* log */
                }
            }

            throw;
        }
        finally
        {
            foreach (var part in unitOfWork.Parts)
            {
                try
                {
                    await part.DisposeAsync();
                }
                catch
                { /* log */
                }
            }

            _current = null;
        }

        //Publish after Current becomes null so it won't be captured by transaction

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var failures = new List<Exception>();

        foreach (var notification in unitOfWork.DeferredNotifications)
        {
            try
            {
                await mediator.Publish(notification, cancellationToken);
            }
            catch (Exception ex)
            { 
                // _logger.LogError(ex, "Deferred notification handler failed: {Notification}", n.GetType().Name);
                failures.Add(ex);
            }
        }

        if (failures.Count > 0)
            throw new AggregateException("One or more deferred notification handlers failed.", failures);
    }

    private async Task Rollback(AmbientUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        try
        {
            foreach (var part in unitOfWork.Parts.Reverse())
            {
                try
                {
                    await part.RollbackAsync(cancellationToken);
                }
                catch
                { /* log */
                }
            }
        }
        finally
        {
            foreach (var part in unitOfWork.Parts)
            {
                try
                {
                    await part.DisposeAsync();
                }
                catch
                { /* log */
                }
            }

            _current = null;
        }
    }
}
