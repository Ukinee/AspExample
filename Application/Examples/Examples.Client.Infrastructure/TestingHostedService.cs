using Examples.Common.Domain.Models.Entities;
using Examples.Common.Domain.Models.Identifiers;
using Examples.Common.Domain.Presentation.Responses;
using Microsoft.Extensions.Logging;
using Ukinee.Infrastructure.Common.Contracts;
using Ukinee.Infrastructure.Ddd.Common.LocalCache.Implementations;
using Ukinee.Infrastructure.Ddd.Common.UseCaseServices.Contracts;
using Ukinee.Infrastructure.Ddd.External.Api.UseCaseServicesAdapters;
using Ukinee.Infrastructure.Ddd.Synchronization.Contracts;

namespace Examples.Client.Infrastructure;

public class TestingHostedService(
    ILogger<TestingHostedService> logger,
    IIdentifierReader<ReviewIdentifier, Review> reader,
    CachingIdentifierReader<ReviewIdentifier, Review, ExternalGatewayAdapter<ReviewIdentifier, Review, ReviewResponse>> cachingReader
) : IOrderedHostedService
{
    public int OrderByPriority => SynchronizationOrderByPriority999.Value;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting testing hosted service");

        logger.LogInformation(reader.GetType().Name);
        logger.LogInformation(cachingReader.GetType().Name);
    }

    public async Task StopAsync(CancellationToken cancellationToken) { }
}
