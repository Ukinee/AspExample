using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using NUnit.Framework;
using Ukinee.Infrastructure.Ddd.Common.Authorization.Domain;
using Ukinee.Infrastructure.Ddd.Common.Entities;
using Ukinee.Infrastructure.Ddd.Common.Exceptions;
using Ukinee.Infrastructure.Ddd.Common.UseCases.Contracts;
using Ukinee.Infrastructure.Ddd.DependencyInjection.Core;
using Ukinee.Infrastructure.Ddd.DependencyInjection.DataSources;
using Ukinee.Infrastructure.Ddd.DependencyInjection.DependenciesStartup;
using Ukinee.Infrastructure.Ddd.DependencyInjection.EntityFeatures.Ddd.Local;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation.Exceptions;
using Ukinee.Infrastructure.Ddd.Local.Repositories;
using Ukinee.Infrastructure.Ddd.Tests.Common.Utils;
using Ukinee.Infrastructure.Ddd.Tests.Common.Utils.TestBases;
using Ukinee.Infrastructure.Ddd.Tests.Domain;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Announcements;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Borders;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Cookies;
using Ukinee.Infrastructure.Ddd.Tests.Domain.Users;

namespace Ukinee.Infrastructure.Ddd.Tests.Integration.CrudOperations;

public class CrudCreationServiceTests : TestBase
{
    private CancellationToken CancellationToken => CancellationToken.None;

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        var mediatrOptions = new MediatRConfig {
            AssembliesToScanHandlers = [typeof(Program).Assembly]
        };

        var registrationPolicy = new RegistrationPolicy();

        var announcementPolicy = AuthorizationPolicyDefinition.GuestReadAndOwnerEdit<AnnouncementIdentifier, Announcement>(identifier => identifier.UserGuid);
        var borderPolicy = AuthorizationPolicyDefinition.AdministratorOnly<BorderIdentifier, Border>();
        var cookiePolicy = TestingAuthorizationPolicyDefinition.VipOwnerOnly<CookieIdentifier, Cookie>(identifier => identifier.UserGuid);

        services
            .SetupUnitOfWork()
            .SetupMediatR(mediatrOptions)
            .SetupJson();

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
                    .SetAccessPolicy(announcementPolicy)
                    .SetCreateUseCase<CreateAnnouncementRequest, CreateAnnouncementRequestValidator, AnnouncementFactory>()
                    .AddUpdateUseCase<UpdateAnnouncementRequest, UpdateAnnouncementRequestValidator, AnnouncementFactory>()
                )
            )
            .LocalContexts.AddInMemoryContext<BorderIdentifier, Border>(contextConfigurator => contextConfigurator
                .Ddd.RegisterAsInMemoryState(dddConfigurator => dddConfigurator
                    .SetAccessPolicy(borderPolicy)
                    .SetCreateUseCase<CreateBorderRequest, CreateBorderRequestValidator, BorderFactory>()
                    .AddUpdateUseCase<UpdateBorderRequest, UpdateBorderRequestValidator, BorderFactory>()
                )
            )
            .LocalContexts.AddInMemoryContext<CookieIdentifier, Cookie>(contextConfigurator => contextConfigurator
                .Ddd.RegisterAsInMemoryState(dddConfigurator => dddConfigurator
                    .SetAccessPolicy(cookiePolicy)
                    .SetCreateUseCase<CreateCookieRequest, CreateCookieRequestValidator, CookieFactory>()
                    .AddUpdateUseCase<UpdateCookieRequest, UpdateCookieRequestValidator, CookieFactory>()
                )
            )
            .Build(registrationPolicy);
    }

#region Create
    [Test]
    public async Task CreateUseCase_LoggedIn_CanCreateEntity_And_ItCanBeReadByGuests()
    {
        var getUseCase = GetService<IGetEntityUseCase<AnnouncementIdentifier, Announcement>>();
        var createUseCase = GetService<ICreateEntityUseCase<Announcement, CreateAnnouncementRequest>>();
        var request = ExampleAnnouncementFactory.CreateRequest1;

        var actor1 = ExampleUserFactory.User1;
        var actorAdmin = ExampleUserFactory.UserAdmin1;
        var actorGuest = ExampleUserFactory.UserGuest;

        var creationResult1 = await createUseCase.Execute(actor1, request, CancellationToken);
        var creationResultAdmin = await createUseCase.Execute(actorAdmin, request, CancellationToken);
        var creationResultGuest = createUseCase.Execute(actorGuest, request, CancellationToken);

        var foundResult11 = await getUseCase.Execute(actor1, creationResult1.Identifier, CancellationToken);
        var foundResult22 = await getUseCase.Execute(actorAdmin, creationResultAdmin.Identifier, CancellationToken);
        var foundResult21 = await getUseCase.Execute(actorAdmin, creationResult1.Identifier, CancellationToken);
        var foundResult12 = await getUseCase.Execute(actor1, creationResultAdmin.Identifier, CancellationToken);
        var foundResultGuest = await getUseCase.Execute(actorGuest, creationResultAdmin.Identifier, CancellationToken);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(foundResult11, Is.SameAs(creationResult1));
            Assert.That(foundResult22, Is.SameAs(creationResultAdmin));
            Assert.That(foundResult21, Is.SameAs(creationResult1));
            Assert.That(foundResult12, Is.SameAs(creationResultAdmin));
            Assert.That(foundResultGuest, Is.SameAs(creationResultAdmin));

            Assert.ThrowsAsync<EntityAccessDeniedException<AnnouncementIdentifier, Announcement>>(() => creationResultGuest);
        }
    }

    [Test]
    public async Task CreateUseCase_Group_CanCreateEntity_And_ItCanBeReadByGuests()
    {
        var getUseCase = GetService<IGetEntityUseCase<BorderIdentifier, Border>>();
        var createUseCase = GetService<ICreateEntityUseCase<Border, CreateBorderRequest>>();
        var request = ExampleBorderFactory.CreateRequest1;

        var actor1 = ExampleUserFactory.User1;
        var actorAdmin = ExampleUserFactory.UserAdmin1;
        var actorGuest = ExampleUserFactory.UserGuest;

        var creationResult1 = createUseCase.Execute(actor1, request, CancellationToken);
        var creationResultAdmin = await createUseCase.Execute(actorAdmin, request, CancellationToken);
        var creationResultGuest = createUseCase.Execute(actorGuest, request, CancellationToken);

        var foundResult1 = getUseCase.Execute(actor1, creationResultAdmin.Identifier, CancellationToken);
        var foundResultAdmin = await getUseCase.Execute(actorAdmin, creationResultAdmin.Identifier, CancellationToken);
        var foundResultGuest = getUseCase.Execute(actorGuest, creationResultAdmin.Identifier, CancellationToken);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(foundResultAdmin, Is.SameAs(creationResultAdmin));

            Assert.ThrowsAsync<EntityAccessDeniedException<BorderIdentifier, Border>>(() => creationResultGuest);
            Assert.ThrowsAsync<EntityAccessDeniedException<BorderIdentifier, Border>>(() => creationResult1);

            Assert.ThrowsAsync<EntityNotFoundException<BorderIdentifier, Border>>(() => foundResult1);
            Assert.ThrowsAsync<EntityNotFoundException<BorderIdentifier, Border>>(() => foundResultGuest);
        }
    }

    [Test]
    public async Task CreateUseCase_GroupOwner_CanCreateEntity_And_ItCanBeReadByOwner()
    {
        var getUseCase = GetService<IGetEntityUseCase<CookieIdentifier, Cookie>>();
        var createUseCase = GetService<ICreateEntityUseCase<Cookie, CreateCookieRequest>>();
        var request = ExampleCookieFactory.CreateRequest1;

        var actor2 = ExampleUserFactory.User2;
        var actorAdmin = ExampleUserFactory.UserAdmin1;
        var actorGuest = ExampleUserFactory.UserGuest;

        var creationResult2 = await createUseCase.Execute(actor2, request, CancellationToken);
        var creationResultAdmin = createUseCase.Execute(actorAdmin, request, CancellationToken);
        var creationResultGuest = createUseCase.Execute(actorGuest, request, CancellationToken);

        var foundResult2 = await getUseCase.Execute(actor2, creationResult2.Identifier, CancellationToken);
        var foundResultAdmin = getUseCase.Execute(actorAdmin, creationResult2.Identifier, CancellationToken);
        var foundResultGuest = getUseCase.Execute(actorGuest, creationResult2.Identifier, CancellationToken);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(foundResult2, Is.SameAs(creationResult2));

            Assert.ThrowsAsync<EntityAccessDeniedException<CookieIdentifier, Cookie>>(() => creationResultAdmin);
            Assert.ThrowsAsync<EntityAccessDeniedException<CookieIdentifier, Cookie>>(() => creationResultGuest);

            Assert.ThrowsAsync<EntityNotFoundException<CookieIdentifier, Cookie>>(() => foundResultAdmin);
            Assert.ThrowsAsync<EntityNotFoundException<CookieIdentifier, Cookie>>(() => foundResultGuest);
        }
    }
#endregion

#region Update
    [Test]
    public async Task UpdateUseCase_Owner_CanEditEntity()
    {
        var updateUseCase = GetService<IUpdateEntityUseCase<AnnouncementIdentifier, Announcement, UpdateAnnouncementRequest>>();

        Seed<AnnouncementIdentifier, Announcement>(ExampleAnnouncementFactory.Example1, ExampleAnnouncementFactory.Example2, ExampleAnnouncementFactory.Example3);

        var actor1 = ExampleUserFactory.User1;
        var actor2 = ExampleUserFactory.User2;

        var identifier = ExampleAnnouncementFactory.Example2.Identifier;
        var request = ExampleAnnouncementFactory.UpdateRequest1;

        var resultActor1 = updateUseCase.Execute(actor1, identifier, request, CancellationToken);
        var resultActor2 = await updateUseCase.Execute(actor2, identifier, request, CancellationToken);

        Assert.ThrowsAsync<EntityNotFoundException<AnnouncementIdentifier, Announcement>>(() => resultActor1);
        Assert.That(resultActor2.Contents, Is.EqualTo(request.Contents));
    }

    [Test]
    public async Task UpdateUseCase_Group_CanEditEntity()
    {
        var updateUseCase = GetService<IUpdateEntityUseCase<BorderIdentifier, Border, UpdateBorderRequest>>();

        Seed<BorderIdentifier, Border>(ExampleBorderFactory.Example1, ExampleBorderFactory.Example2, ExampleBorderFactory.Example3);

        var actor1 = ExampleUserFactory.User1;
        var actor2 = ExampleUserFactory.User2;
        var actorAdmin1 = ExampleUserFactory.UserAdmin1;
        var actorAdmin2 = ExampleUserFactory.UserAdmin2;

        var identifier = ExampleBorderFactory.Example2.Identifier;
        var request = ExampleBorderFactory.UpdateRequest1;

        var resultActor1 = updateUseCase.Execute(actor1, identifier, request, CancellationToken);
        var resultActor2 = updateUseCase.Execute(actor2, identifier, request, CancellationToken);
        var resultActorAdmin1 = await updateUseCase.Execute(actorAdmin1, identifier, request, CancellationToken);
        var resultActorAdmin2 = await updateUseCase.Execute(actorAdmin2, identifier, request, CancellationToken);

        Assert.ThrowsAsync<EntityNotFoundException<BorderIdentifier, Border>>(() => resultActor1);
        Assert.ThrowsAsync<EntityNotFoundException<BorderIdentifier, Border>>(() => resultActor2);
        Assert.That(resultActorAdmin1.Size, Is.EqualTo(request.Size));
        Assert.That(resultActorAdmin2.Size, Is.EqualTo(request.Size));
    }

    [Test]
    public async Task UpdateUseCase_GroupOwner_CanEditEntity()
    {
        var updateUseCase = GetService<IUpdateEntityUseCase<CookieIdentifier, Cookie, UpdateCookieRequest>>();

        Seed<CookieIdentifier, Cookie>(ExampleCookieFactory.Example1, ExampleCookieFactory.Example2, ExampleCookieFactory.Example3);

        var actor1 = ExampleUserFactory.User1;
        var actor3 = ExampleUserFactory.User3;
        var actorAdmin1 = ExampleUserFactory.UserAdmin1;
        var actorAdmin2 = ExampleUserFactory.UserAdmin2;

        var identifier = ExampleCookieFactory.Example3.Identifier;
        var request = ExampleCookieFactory.UpdateRequest1;

        var resultActor1 = updateUseCase.Execute(actor1, identifier, request, CancellationToken);
        var resultActor3 = await updateUseCase.Execute(actor3, identifier, request, CancellationToken);
        var resultActorAdmin1 = updateUseCase.Execute(actorAdmin1, identifier, request, CancellationToken);
        var resultActorAdmin2 = updateUseCase.Execute(actorAdmin2, identifier, request, CancellationToken);

        Assert.ThrowsAsync<EntityNotFoundException<CookieIdentifier, Cookie>>(() => resultActor1);
        Assert.That(resultActor3.Calories, Is.EqualTo(request.Calories));
        Assert.ThrowsAsync<EntityNotFoundException<CookieIdentifier, Cookie>>(() => resultActorAdmin1);
        Assert.ThrowsAsync<EntityNotFoundException<CookieIdentifier, Cookie>>(() => resultActorAdmin2);
    }
#endregion

#region Delete
    [Test]
    public async Task DeleteUseCase_Owner_CanDeleteEntity()
    {
        var useCase = GetService<IRemoveEntityUseCase<AnnouncementIdentifier, Announcement>>();

        Seed<AnnouncementIdentifier, Announcement>(ExampleAnnouncementFactory.Example1, ExampleAnnouncementFactory.Example2, ExampleAnnouncementFactory.Example3);

        var actor1 = ExampleUserFactory.User1;
        var actor2 = ExampleUserFactory.User2;

        var identifier = ExampleAnnouncementFactory.Example1.Identifier;

        var actor2Task = useCase.Execute(actor2, identifier, CancellationToken);
        await useCase.Execute(actor1, identifier, CancellationToken);
        var actor1Task = useCase.Execute(actor1, identifier, CancellationToken);

        Assert.ThrowsAsync<EntityNotFoundException<AnnouncementIdentifier, Announcement>>(() => actor2Task);
        Assert.ThrowsAsync<EntityNotFoundException<AnnouncementIdentifier, Announcement>>(() => actor1Task);
    }

    [Test]
    public async Task DeleteUseCase_Group_CanDeleteEntity()
    {
        var useCase = GetService<IRemoveEntityUseCase<BorderIdentifier, Border>>();

        Seed<BorderIdentifier, Border>(ExampleBorderFactory.Example1, ExampleBorderFactory.Example2, ExampleBorderFactory.Example3);

        var actor1 = ExampleUserFactory.User1;
        var actor2 = ExampleUserFactory.User2;
        var actorAdmin1 = ExampleUserFactory.UserAdmin1;
        var actorAdmin2 = ExampleUserFactory.UserAdmin2;

        var identifier = ExampleBorderFactory.Example1.Identifier;

        var actor1Task = useCase.Execute(actor1, identifier, CancellationToken);
        var actor2Task = useCase.Execute(actor2, identifier, CancellationToken);
        await useCase.Execute(actorAdmin1, identifier, CancellationToken);
        var actorAdmin2Task = useCase.Execute(actorAdmin2, identifier, CancellationToken);

        Assert.ThrowsAsync<EntityNotFoundException<BorderIdentifier, Border>>(() => actor2Task);
        Assert.ThrowsAsync<EntityNotFoundException<BorderIdentifier, Border>>(() => actor1Task);
        Assert.ThrowsAsync<EntityNotFoundException<BorderIdentifier, Border>>(() => actorAdmin2Task);
    }

    [Test]
    public async Task DeleteUseCase_GroupOwner_CanDeleteEntity()
    {
        var useCase = GetService<IRemoveEntityUseCase<CookieIdentifier, Cookie>>();

        Seed<CookieIdentifier, Cookie>(ExampleCookieFactory.Example1, ExampleCookieFactory.Example2, ExampleCookieFactory.Example3);

        var actor1 = ExampleUserFactory.User1;
        var actor2 = ExampleUserFactory.User2;
        var actor3 = ExampleUserFactory.User3;
        var actorAdmin1 = ExampleUserFactory.UserAdmin1;
        var actorAdmin2 = ExampleUserFactory.UserAdmin2;

        var identifier = ExampleCookieFactory.Example3.Identifier;

        var actor1Task = useCase.Execute(actor1, identifier, CancellationToken);
        var actor2Task = useCase.Execute(actor2, identifier, CancellationToken);
        var actorAdmin1Task = useCase.Execute(actorAdmin1, identifier, CancellationToken);
        var actorAdmin2Task = useCase.Execute(actorAdmin2, identifier, CancellationToken);
        await useCase.Execute(actor3, identifier, CancellationToken);
        var actor3Task = useCase.Execute(actor3, identifier, CancellationToken);

        Assert.ThrowsAsync<EntityNotFoundException<CookieIdentifier, Cookie>>(() => actor2Task);
        Assert.ThrowsAsync<EntityNotFoundException<CookieIdentifier, Cookie>>(() => actor1Task);
        Assert.ThrowsAsync<EntityNotFoundException<CookieIdentifier, Cookie>>(() => actorAdmin1Task);
        Assert.ThrowsAsync<EntityNotFoundException<CookieIdentifier, Cookie>>(() => actorAdmin2Task);
        Assert.ThrowsAsync<EntityNotFoundException<CookieIdentifier, Cookie>>(() => actor3Task);
    }
#endregion

#region Read
    [Test]
    public async Task GetUseCase_Guest_AllCanReadPublicEntities_AndOwnerCanReadIts()
    {
        Announcement[] examples = [ExampleAnnouncementFactory.Example1, ExampleAnnouncementFactory.Example2, ExampleAnnouncementFactory.Example3];
        var identifiers = examples.Select(x => x.Identifier).ToArray();

        Seed<AnnouncementIdentifier, Announcement>(examples);

        var useCase = GetService<IGetEntityUseCase<AnnouncementIdentifier, Announcement>>();

        var actor1 = ExampleUserFactory.User1;
        var actor2 = ExampleUserFactory.User2;
        var actorAdmin = ExampleUserFactory.UserAdmin1;
        var actorGuest = ExampleUserFactory.UserGuest;

        var singleEntity1Actor1 = await useCase.Execute(actor1, ExampleAnnouncementFactory.Example1.Identifier, CancellationToken);
        var singleEntity2Actor1 = useCase.Execute(actor1, ExampleAnnouncementFactory.Example2.Identifier, CancellationToken);
        var singleEntity3Actor1 = await useCase.Execute(actor1, ExampleAnnouncementFactory.Example3.Identifier, CancellationToken);

        var singleEntity1Actor2 = await useCase.Execute(actor2, ExampleAnnouncementFactory.Example1.Identifier, CancellationToken);
        var singleEntity2Actor2 = await useCase.Execute(actor2, ExampleAnnouncementFactory.Example2.Identifier, CancellationToken);
        var singleEntity3Actor2 = await useCase.Execute(actor2, ExampleAnnouncementFactory.Example3.Identifier, CancellationToken);

        var singleEntity1ActorAdmin = await useCase.Execute(actorAdmin, ExampleAnnouncementFactory.Example1.Identifier, CancellationToken);
        var singleEntity2ActorAdmin = useCase.Execute(actorAdmin, ExampleAnnouncementFactory.Example2.Identifier, CancellationToken);
        var singleEntity3ActorAdmin = await useCase.Execute(actorAdmin, ExampleAnnouncementFactory.Example3.Identifier, CancellationToken);

        var singleEntity1ActorGuest = await useCase.Execute(actorGuest, ExampleAnnouncementFactory.Example1.Identifier, CancellationToken);
        var singleEntity2ActorGuest = useCase.Execute(actorGuest, ExampleAnnouncementFactory.Example2.Identifier, CancellationToken);
        var singleEntity3ActorGuest = await useCase.Execute(actorGuest, ExampleAnnouncementFactory.Example3.Identifier, CancellationToken);

        var softActor1 = await useCase.ExecuteSoft(actor1, identifiers, CancellationToken).ToListAsync(CancellationToken);
        var softActor2 = await useCase.ExecuteSoft(actor2, identifiers, CancellationToken).ToListAsync(CancellationToken);
        var softActorAdmin = await useCase.ExecuteSoft(actorAdmin, identifiers, CancellationToken).ToListAsync(CancellationToken);
        var softActorGuest = await useCase.ExecuteSoft(actorGuest, identifiers, CancellationToken).ToListAsync(CancellationToken);

        var strictActor1 = useCase.ExecuteStrict(actor1, identifiers, CancellationToken);
        var strictActor2 = await useCase.ExecuteStrict(actor2, identifiers, CancellationToken);
        var strictActorAdmin = useCase.ExecuteStrict(actorAdmin, identifiers, CancellationToken);
        var strictActorGuest = useCase.ExecuteStrict(actorGuest, identifiers, CancellationToken);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(singleEntity1Actor1, Is.EqualTo(ExampleAnnouncementFactory.Example1));
            Assert.That(singleEntity1Actor2, Is.EqualTo(ExampleAnnouncementFactory.Example1));
            Assert.That(singleEntity1ActorAdmin, Is.EqualTo(ExampleAnnouncementFactory.Example1));
            Assert.That(singleEntity1ActorGuest, Is.EqualTo(ExampleAnnouncementFactory.Example1));

            Assert.ThrowsAsync<EntityNotFoundException<AnnouncementIdentifier, Announcement>>(() => singleEntity2Actor1);
            Assert.That(singleEntity2Actor2, Is.EqualTo(ExampleAnnouncementFactory.Example2));
            Assert.ThrowsAsync<EntityNotFoundException<AnnouncementIdentifier, Announcement>>(() => singleEntity2ActorAdmin);
            Assert.ThrowsAsync<EntityNotFoundException<AnnouncementIdentifier, Announcement>>(() => singleEntity2ActorGuest);

            Assert.That(singleEntity3Actor1, Is.EqualTo(ExampleAnnouncementFactory.Example3));
            Assert.That(singleEntity3Actor2, Is.EqualTo(ExampleAnnouncementFactory.Example3));
            Assert.That(singleEntity3ActorAdmin, Is.EqualTo(ExampleAnnouncementFactory.Example3));
            Assert.That(singleEntity3ActorGuest, Is.EqualTo(ExampleAnnouncementFactory.Example3));
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(softActor1, Has.Count.EqualTo(2));
            Assert.That(softActor2, Has.Count.EqualTo(3));
            Assert.That(softActorAdmin, Has.Count.EqualTo(2));
            Assert.That(softActorGuest, Has.Count.EqualTo(2));
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.ThrowsAsync<EntityNotFoundException<AnnouncementIdentifier, Announcement>>(() => strictActor1);
            Assert.That(strictActor2, Has.Count.EqualTo(3));
            Assert.ThrowsAsync<EntityNotFoundException<AnnouncementIdentifier, Announcement>>(() => strictActorAdmin);
            Assert.ThrowsAsync<EntityNotFoundException<AnnouncementIdentifier, Announcement>>(() => strictActorGuest);
        }
    }
#endregion

    private void Seed<TIdentifier, TEntity>(params TEntity[] entities)
    where TEntity : IEntity<TIdentifier>
    where TIdentifier : notnull
    {
        var repository = GetService<IEditableTrackedRepository<TIdentifier, TEntity>>();
        repository.AddRange(entities, CancellationToken);
    }
}
