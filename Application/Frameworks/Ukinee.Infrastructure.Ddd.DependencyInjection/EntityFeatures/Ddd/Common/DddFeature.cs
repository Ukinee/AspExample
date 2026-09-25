using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Core;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Utils;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.Common;

public class DddFeature<TIdentifier, TEntity> : IBuildable
{
    public static DddFeature<TIdentifier, TEntity> Empty => new DddFeature<TIdentifier, TEntity> {
        Extensions = [],
    };
    public required List<Action<IServiceCollection>> Extensions { get; init; }

    public List<ServiceDescriptor> Repository { get; } = [];
    public List<ServiceDescriptor> WriteBack { get; } = [];
    public List<ServiceDescriptor> AccessValidator { get; } = [];
    public List<ServiceDescriptor> EntityCreator { get; } = [];
    public List<ServiceDescriptor> DeltaEntityUpdater { get; } = [];
    public List<ServiceDescriptor> EntityRemover { get; } = [];
    public List<ServiceDescriptor> IdentifierReader { get; } = [];
    public List<ServiceDescriptor> EntityReader { get; } = [];
    public List<ServiceDescriptor> SpecificationReader { get; } = [];
    public List<ServiceDescriptor> GetOrCreate { get; } = [];
    public List<ServiceDescriptor> PayloadEntityUpdaters { get; } = [];
    public List<ServiceDescriptor> CustomRequestHandlers { get; } = [];

    public void Build(RegistrationPolicy policy, IServiceCollection serviceCollection)
    {
        PayloadHelper.AddFiltered(serviceCollection, policy, Repository);
        PayloadHelper.AddFiltered(serviceCollection, policy, WriteBack);
        PayloadHelper.AddFiltered(serviceCollection, policy, AccessValidator);
        PayloadHelper.AddFiltered(serviceCollection, policy, EntityCreator);
        PayloadHelper.AddFiltered(serviceCollection, policy, DeltaEntityUpdater);
        PayloadHelper.AddFiltered(serviceCollection, policy, EntityRemover);
        PayloadHelper.AddFiltered(serviceCollection, policy, IdentifierReader);
        PayloadHelper.AddFiltered(serviceCollection, policy, EntityReader);
        PayloadHelper.AddFiltered(serviceCollection, policy, SpecificationReader);
        PayloadHelper.AddFiltered(serviceCollection, policy, GetOrCreate);
        PayloadHelper.AddFiltered(serviceCollection, policy, PayloadEntityUpdaters);
        PayloadHelper.AddFiltered(serviceCollection, policy, CustomRequestHandlers);

        foreach (var extension in Extensions)
        {
            extension.Invoke(serviceCollection);
        }
    }
}
