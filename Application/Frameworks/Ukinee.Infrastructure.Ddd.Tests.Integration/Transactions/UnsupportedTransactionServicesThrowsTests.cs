using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using NUnit.Framework;
using Ukinee.Infrastructure.Ddd.Common.Authorization.Domain;
using Ukinee.Infrastructure.Ddd.Common.UnitOfWork.Contacts;
using Ukinee.Infrastructure.Ddd.Common.UseCases.Contracts;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Core;
using Ukinee.Infrastructure.Ddd.DependencyInjection.DataSources;
using Ukinee.Infrastructure.Ddd.DependencyInjection.DependenciesStartup;
using Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.External;
using Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.Local;
using Ukinee.Infrastructure.Ddd.Local.UseCases.Contracts;
using Ukinee.Infrastructure.Ddd.Tests.Common.Extensions.ServiceCollectionExtensions;
using Ukinee.Infrastructure.Ddd.Tests.Common.Utils.TestBases;
using Ukinee.Infrastructure.Ddd.Tests.Domain;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Announcements;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Borders;
using Ukinee.Users.Common.ValueObjects;

namespace Ukinee.Infrastructure.Ddd.Tests.Integration.Transactions;

public class UnsupportedTransactionServicesThrowsTests : TestBase
{
    protected override void ConfigureTestServices(IServiceCollection services)
    {
        var mediatrOptions = new MediatRConfig {
            AssembliesToScanHandlers = [typeof(Program).Assembly]
        };

        var registrationPolicy = new RegistrationPolicy();

        var eventPolicy = AuthorizationPolicyDefinition.GuestReadAndOwnerEdit<AnnouncementIdentifier, Announcement>(identifier => identifier.UserGuid);

        services
            .SetupUnitOfWork()
            .SetupCommonServices(mediatrOptions);

        services
            .RegisterModule<TestingTag>()
            .ApiDatasource.Register(httpClientBuilder =>
                {
                    httpClientBuilder
                        .ConfigureHttpClient(client =>
                            {
                                client.Timeout = TimeSpan.FromMinutes(3);
                                client.BaseAddress = new Uri("options.BaseServerAddress");
                            }
                        )
                        .AddDefaultLogger()
                        .SetHandlerLifetime(TimeSpan.FromMinutes(3));
                }
            )
            .LocalContexts.AddInMemoryContext<AnnouncementIdentifier, Announcement>(contextConfigurator => contextConfigurator
                .Ddd.RegisterAsInMemoryState(dddConfigurator => dddConfigurator
                    .SetAccessPolicy(eventPolicy)
                    .SetCreateUseCase<CreateAnnouncementRequest, CreateAnnouncementRequestValidator, AnnouncementFactory>()
                    .AddUpdateUseCase<UpdateAnnouncementRequest, UpdateAnnouncementRequestValidator, AnnouncementFactory>()
                )
            )
            .ApiClientContexts.AddRemoteContext<BorderIdentifier, Border>(contextConfigurator => contextConfigurator
                .Ddd.Register<BorderIdentifierParams, BorderResponse>(dddConfigurator => dddConfigurator
                    .WithMapsterResponseToEntityMap()
                    .SetCreateUseCase<CreateBorderRequest>()
                    .AddUpdateUseCase<UpdateBorderRequest>()
                    .AddLocalCache()
                )
            )
            .Build(registrationPolicy);
    }

    private static CancellationToken CancellationToken => CancellationToken.None;
    private static UserContext UserContext => UserContext.Test;

    [Test]
    public async Task CreateUseCase_InMemory_ShouldFail_TransactionNotSupported()
    {
        var useCase = GetService<ICreateEntityUseCase<Announcement, CreateAnnouncementRequest>>();

        Assert.ThrowsAsync<NotSupportedException>(async () => await ExecuteInTransactionContext(async () => await useCase.Execute(
                    UserContext,
                    ExampleAnnouncementFactory.CreateRequest1,
                    CancellationToken
                )
            )
        );
    }

    [Test]
    public async Task CreateUseCase_Api_ShouldFail_TransactionNotSupported()
    {
        var useCase = GetService<ICreateEntityUseCase<Border, CreateBorderRequest>>();

        Assert.ThrowsAsync<NotSupportedException>(async () => await ExecuteInTransactionContext(async () => await useCase.Execute(
                    UserContext,
                    ExampleBorderFactory.CreateRequest1,
                    CancellationToken
                )
            )
        );
    }

    [Test]
    public async Task UpdateUseCase_InMemory_ShouldFail_TransactionNotSupported()
    {
        var useCase = GetService<IUpdateEntityUseCase<AnnouncementIdentifier, Announcement, UpdateAnnouncementRequest>>();

        Assert.ThrowsAsync<NotSupportedException>(async () => await ExecuteInTransactionContext(async () => await useCase.Execute(
                    UserContext,
                    ExampleAnnouncementFactory.Example1.Identifier,
                    ExampleAnnouncementFactory.UpdateRequest1,
                    CancellationToken
                )
            )
        );
    }

    [Test]
    public async Task UpdateUseCase_Api_ShouldFail_TransactionNotSupported()
    {
        var useCase = GetService<IUpdateEntityUseCase<BorderIdentifier, Border, UpdateBorderRequest>>();

        Assert.ThrowsAsync<NotSupportedException>(async () => await ExecuteInTransactionContext(async () => await useCase.Execute(
                    UserContext,
                    ExampleBorderFactory.Example1.Identifier,
                    ExampleBorderFactory.UpdateRequest1,
                    CancellationToken
                )
            )
        );
    }

    [Test]
    public async Task DeleteUseCase_InMemory_ShouldFail_TransactionNotSupported()
    {
        var useCase = GetService<IRemoveEntityUseCase<AnnouncementIdentifier, Announcement>>();

        Assert.ThrowsAsync<NotSupportedException>(async () => await ExecuteInTransactionContext(async () => await useCase.Execute(
                    UserContext,
                    ExampleAnnouncementFactory.Example1.Identifier,
                    CancellationToken
                )
            )
        );
    }

    [Test]
    public async Task DeleteUseCase_Api_ShouldFail_TransactionNotSupported()
    {
        var useCase = GetService<IRemoveEntityUseCase<BorderIdentifier, Border>>();

        Assert.ThrowsAsync<NotSupportedException>(async () => await ExecuteInTransactionContext(async () => await useCase.Execute(
                    UserContext,
                    ExampleBorderFactory.Example1.Identifier,
                    CancellationToken
                )
            )
        );
    }

    [Test]
    public async Task DeltaUpdateUseCase_InMemory_ShouldFail_TransactionNotSupported()
    {
        var useCase = GetService<IDeltaUpdateEntityUseCase<AnnouncementIdentifier, Announcement>>();

        Assert.ThrowsAsync<NotSupportedException>(async () => await ExecuteInTransactionContext(async () => await useCase.Execute(
                    UserContext,
                    ExampleAnnouncementFactory.Example1.Identifier,
                    (old) => old,
                    CancellationToken
                )
            )
        );
    }

    private async Task ExecuteInTransactionContext(AsyncTestDelegate action)
    {
        var uowFactory = GetService<IUnitOfWorkFactory>();

        await using var uow = uowFactory.Create<TestingTag>();

        await action.Invoke();

        await uow.RollbackAsync(CancellationToken.None);
    }
}
