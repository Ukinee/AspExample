using System.Collections.Concurrent;
using MapsterMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.Utils.Extensions;
using Ukinee.Infrastructure.SignalR.Common;
using Ukinee.Infrastructure.SignalR.Server.Contracts;

namespace Ukinee.Infrastructure.SignalR.Server.Services;

public class BatchedSignalRSink<TRequest, TIdentifier, TEntity, TViewModel, THub> : ISignalRSink<TIdentifier, TEntity>, IDisposable
where TEntity : IEntity<TIdentifier>
where THub : Hub
where TIdentifier : notnull
{
    private readonly IMapper _mapper;
    private readonly IHubContext<THub> _hub;
    private readonly IRouteResolver<TIdentifier, TEntity, TRequest> _routeResolver;
    private readonly ILogger<BatchedSignalRSink<TRequest, TIdentifier, TEntity, TViewModel, THub>> _logger;

    private readonly ConcurrentDictionary<TIdentifier, TEntity> _entityBuffer = new ConcurrentDictionary<TIdentifier, TEntity>();
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();
    private readonly TimeSpan _flushInterval = TimeSpan.FromMilliseconds(300); // todo: to configs

    public BatchedSignalRSink(
        IMapper mapper,
        IHubContext<THub> hub,
        IRouteResolver<TIdentifier, TEntity, TRequest> routeResolver,
        ILogger<BatchedSignalRSink<TRequest, TIdentifier, TEntity, TViewModel, THub>> logger
    )
    {
        _mapper = mapper;
        _hub = hub;
        _routeResolver = routeResolver;
        _logger = logger;

        _ = FlushLoopAsync();
    }

    public Task NotifyCreation(IReadOnlyCollection<TEntity> entities)
    {
        foreach (var entity in entities)
            Enqueue(entity);

        return Task.CompletedTask;
    }

    public Task NotifyUpdate(IReadOnlyCollection<TEntity> entities)
    {
        foreach (var entity in entities)
            Enqueue(entity);

        return Task.CompletedTask;
    }

    public async Task NotifyRemoval(IReadOnlyCollection<TIdentifier> identifiers)
    {
        var method = SignalRMethodNamesUtils.Removed<TEntity>();

        var groupsMap = identifiers.ToLookupMany(
            identifier => _routeResolver.GetGroupsForIdentifier(identifier, EntityEventType.Removed),
            identifier => identifier
        );

        var sendTasks = groupsMap.Select(grouping => _hub.Clients.Group(grouping.Key).SendAsync(method, grouping.ToList()));
        await Task.WhenAll(sendTasks);
    }

    private void Enqueue(TEntity entity)
    {
        _entityBuffer[entity.Identifier] = entity;
    }

    private async Task FlushLoopAsync()
    {
        using var timer = new PeriodicTimer(_flushInterval);

        while (await timer.WaitForNextTickAsync(_cts.Token))
        {
            try
            {
                await ProcessBuffer();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error flushing SignalR buffer");
            }
        }
    }

    private async Task ProcessBuffer()
    {
        if (_entityBuffer.IsEmpty)
            return;

        var keys = _entityBuffer.Keys.ToList();
        var entities = new List<TEntity>(keys.Count);

        foreach (var key in keys)
        {
            if (_entityBuffer.TryRemove(key, out var entity))
            {
                entities.Add(entity);
            }
        }

        if (entities.Count == 0)
            return;

        var method = SignalRMethodNamesUtils.Upsert<TEntity>();

        var groupsMap = entities.ToLookupMany(
            entity => _routeResolver.GetGroupsForIdentifier(entity.Identifier, EntityEventType.Updated),
            entity => _mapper.Map<TViewModel>(entity)
        );

        var sendTasks = groupsMap.Select(grouping => _hub.Clients.Group(grouping.Key).SendAsync(method, grouping.ToList()));
        await Task.WhenAll(sendTasks);
    }

    public void Dispose()
    {
        _cts.Cancel();
    }
}
