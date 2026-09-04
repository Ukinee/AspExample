using System.Collections.Concurrent;
using MapsterMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.SignalR.Contracts;

namespace Ukinee.Infrastructure.SignalR.Services;

public class BatchedSignalRSink<TRequest, TIdentifier, TEntity, TViewModel, THub> : ISignalRSink<TEntity>, IDisposable
where TEntity : IEntity<TIdentifier>
where THub : Hub
where TIdentifier : notnull
{
    private readonly IMapper _mapper;
    private readonly IHubContext<THub> _hub;
    private readonly IRouteResolver<TEntity, TRequest> _routeResolver;
    private readonly ILogger<BatchedSignalRSink<TRequest, TIdentifier, TEntity, TViewModel, THub>> _logger;

    private readonly ConcurrentDictionary<TIdentifier, TEntity> _entityBuffer = new ConcurrentDictionary<TIdentifier, TEntity>();
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();
    private readonly TimeSpan _flushInterval = TimeSpan.FromMilliseconds(300); // todo: to configs

    public BatchedSignalRSink(
        IMapper mapper,
        IHubContext<THub> hub,
        IRouteResolver<TEntity, TRequest> routeResolver,
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
        {
            Enqueue(entity);
        }

        return Task.CompletedTask;
    }

    public Task NotifyUpdate(IReadOnlyCollection<TEntity> entities)
    {
        foreach (var entity in entities)
            Enqueue(entity);

        return Task.CompletedTask;
    }

    public async Task NotifyRemoval(IReadOnlyCollection<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            await Remove(entity);
        }
    }

    private void Enqueue(TEntity entity)
    {
        _entityBuffer[entity.Identifier] = entity;
    }

    private Task Remove(TEntity entity)
    {
        var identifier = entity.Identifier;
        _entityBuffer.TryRemove(identifier, out _);

        var groups = _routeResolver.GetGroupsForEntity(entity, EntityEventType.Removed).ToList();

        if (groups.Count > 0)
        {
            return _hub.Clients.Groups(groups).SendAsync($"{typeof(TEntity).Name}OnRemoved", identifier);
        }

        return Task.CompletedTask;
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

        var entities = _entityBuffer.Values.ToList();
        _entityBuffer.Clear();

        var groupPayloads = new Dictionary<string, List<TViewModel>>();

        foreach (var entity in entities)
        {
            var groups = _routeResolver.GetGroupsForEntity(entity, EntityEventType.Updated);
            var viewModel = _mapper.Map<TViewModel>(entity);

            foreach (var groupName in groups)
            {
                if (!groupPayloads.TryGetValue(groupName, out var list))
                {
                    list = new List<TViewModel>();
                    groupPayloads[groupName] = list;
                }

                list.Add(viewModel);
            }
        }

        var methodName = $"{typeof(TEntity).Name}BatchUpdated";

        var sendTasks = groupPayloads.Select(pair => _hub.Clients.Group(pair.Key).SendAsync(methodName, pair.Value));

        await Task.WhenAll(sendTasks);

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(
                "Batched {EntityCount} entities into {GroupCount} group messages",
                entities.Count,
                groupPayloads.Count
            );
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
    }
}
