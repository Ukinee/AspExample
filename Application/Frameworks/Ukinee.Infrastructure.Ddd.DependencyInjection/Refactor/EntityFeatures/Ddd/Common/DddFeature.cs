using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Core;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Utils;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.Refactor.Features.Ddd.Common;

public class DddFeature<TIdentifier, TEntity> : IBuildable
{
    public static DddFeature<TIdentifier, TEntity> Empty => new DddFeature<TIdentifier, TEntity> {
        Repository = null!,
        WriteBack = null!,
        DeltaEntityUpdater = null!,
        EntityRemover = null!,
        EntityCreator = null!,
        IdentifierReader = null!,
        EntityReader = null!,
        SpecificationReader = null!,
        GetOrCreate = null!,
        PayloadEntityUpdaters = [],
        Extensions = [],
    };

    public class RepositoryDefinition
    {
        public required Type Type { get; init; }
    }

    public class PayloadEntityUpdaterDefinition
    {
        public required ImmutableArray<Type> Types { get; init; }
        public required Type UseCase { get; init; }
    }

    public class DeltaEntityUpdaterDefinition
    {
        public required Type Type { get; init; }
        public required Type UseCase { get; init; }
    }

    public class EntityRemoverDefinition
    {
        public required Type Type { get; init; }
        public required Type UseCase { get; init; }
    }

    public class EntityCreatorDefinition
    {
        public required ImmutableArray<Type> Types { get; init; }
        public required Type UseCase { get; init; }
    }

    public class GetOrCreateFeatureDefinition
    {
        public required ImmutableArray<Type> Types { get; init; }
        public required Type Creator { get; init; }
    }

    public record IdentifierReaderDefinition
    {
        public required Type Type { get; init; }
        public required Type UseCase { get; init; }
    }

    public class EntityReaderDefinition
    {
        public required Type Type { get; init; }
    }

    public class SpecificationReaderDefinition
    {
        public required Type Type { get; init; }
    }

    public class WriteBackDefinition
    {
        public required Type Type { get; init; }
    }

    public required RepositoryDefinition? Repository { get; set; }
    public required WriteBackDefinition? WriteBack { get; set; }

    public required List<PayloadEntityUpdaterDefinition> PayloadEntityUpdaters { get; init; }
    public required DeltaEntityUpdaterDefinition? DeltaEntityUpdater { get; set; }

    public required EntityRemoverDefinition? EntityRemover { get; set; }

    public required EntityCreatorDefinition? EntityCreator { get; set; }

    public required IdentifierReaderDefinition IdentifierReader { get; set; }
    public required EntityReaderDefinition EntityReader { get; set; }
    public required SpecificationReaderDefinition? SpecificationReader { get; set; }

    public required List<Action<IServiceCollection>> Extensions { get; init; }
    public required GetOrCreateFeatureDefinition? GetOrCreate { get; set; }

    public void Build(IReadOnlyCollection<Type> allowedInterfaces, IServiceCollection serviceCollection)
    {
        List<ServiceDescriptor> result = [];

        if (Repository != null)
            result.AddRange(PayloadHelper.GetSingletonDescriptors(Repository.Type, allowedInterfaces));

        if (WriteBack != null)
            result.AddRange(PayloadHelper.GetSingletonDescriptors(WriteBack.Type, allowedInterfaces));

        foreach (var payloadEntityUpdater in PayloadEntityUpdaters)
        {
            result.AddRange(PayloadHelper.GetSingletonDescriptors(payloadEntityUpdater.Types, allowedInterfaces));
            result.AddRange(PayloadHelper.GetSingletonDescriptors(payloadEntityUpdater.UseCase, allowedInterfaces));
        }

        if (DeltaEntityUpdater != null)
        {
            result.AddRange(PayloadHelper.GetSingletonDescriptors(DeltaEntityUpdater.Type, allowedInterfaces));
            result.AddRange(PayloadHelper.GetSingletonDescriptors(DeltaEntityUpdater.UseCase, allowedInterfaces));
        }

        if (EntityRemover != null)
        {
            result.AddRange(PayloadHelper.GetSingletonDescriptors(EntityRemover.Type, allowedInterfaces));
            result.AddRange(PayloadHelper.GetSingletonDescriptors(EntityRemover.UseCase, allowedInterfaces));
        }

        if (EntityCreator != null)
        {
            result.AddRange(PayloadHelper.GetSingletonDescriptors(EntityCreator.Types, allowedInterfaces));
            result.AddRange(PayloadHelper.GetSingletonDescriptors(EntityCreator.UseCase, allowedInterfaces));
        }

        if (GetOrCreate != null)
        {
            result.AddRange(PayloadHelper.GetSingletonDescriptors(GetOrCreate.Types, allowedInterfaces));
            result.AddRange(PayloadHelper.GetSingletonDescriptors(GetOrCreate.Creator, allowedInterfaces));
        }

        result.AddRange(PayloadHelper.GetSingletonDescriptors(IdentifierReader.Type, allowedInterfaces));
        result.AddRange(PayloadHelper.GetSingletonDescriptors(IdentifierReader.UseCase, allowedInterfaces));

        result.AddRange(PayloadHelper.GetSingletonDescriptors(EntityReader.Type, allowedInterfaces));

        if (SpecificationReader != null)
            result.AddRange(PayloadHelper.GetSingletonDescriptors(SpecificationReader.Type, allowedInterfaces));

        foreach (var extension in Extensions)
        {
            extension.Invoke(serviceCollection);
        }

        serviceCollection.TryAdd(result);
    }
}
