using MediatR;
using Ukinee.Infrastructure.Ddd.Local.UnitOfWork.Contacts;
using Ukinee.Infrastructure.Ddd.Local.UnitOfWork.Domain;

namespace Ukinee.Infrastructure.Ddd.Local.UnitOfWork.Implementations;

public class UnitOfWorkFactory(IMediator mediator) : IUnitOfWorkFactory, IUnitOfWorkProvider
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
            foreach (var part in unitOfWork.Parts.Reverse())
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

        foreach (var notification in unitOfWork.DeferredNotifications)
        {
            try
            {
                await mediator.Publish(notification, cancellationToken);
            }
            catch
            { /* log */
            }
        }
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
