DDDSL
===

DDDSL - фреймворк для DDD приложений с единым набором индерфейсов для локальных и удаленных сущностей: любой хост
регистрирует сущности локально (In-Memory или Db) или удаленно (HTTP, SignalR) и работает с ними одинаково. Источники
данных, политики доступа, сущности и ее модули регистрируются декларативно через DSL.

Хост - процесс, илспользующий DSL для регистрации сущности (неважно, локально или удаленно);
Локальный контекст - модуль DSL, ответственный за регистрацию сервисов, существующих в памяти приложения или в базе
данных; Подробнее в [локальных сущностях](#локальные-сущности--localcontexts).
Удаленный контекст - модуль DSL, ответственный за регистрацию сервисов для получения доступа к локальной сущности
другого
процесса; Подробнее в [удаленных сущностях](#удаленные-сущности--apiclientcontexts).

## Содержание

## Quick Start

Предположим, мы хотим написать сервис для хранения публичных анонсов. Нам понадобятся две доменные модели:

```csharp
[ApiIdentifier]
[Identifier(nameof(UserGuid), nameof(Guid))]
public readonly partial record struct AnnouncementIdentifier
{
    public required Guid UserGuid { get; init; }
    public required Guid Guid { get; init; }

    public static AnnouncementIdentifier Create(UserContext userContext, Guid guid)
    {
        return new AnnouncementIdentifier {
            UserGuid = userContext.Guid,
            Guid = guid,
        };
    }
}
```

Необходимость атрибутов описана в [отдельной секции](#идентификаторы). Если коротко, то ``ApiIdentifierAttribute``
добавляет идетификатору возможность быть использованным в Api, а ``IdentifierAttribute(nameof(Guid))``
фиксирует порядок свойств (их может быть несколько) и генерирует методы ``Create``, ``ToString``, реализацию
``ISpanParsable``.

``UserGuid`` тут это guid пользователя, создавшего сущность.

```csharp
[HasIdentifier(typeof(AnnouncementIdentifier))]
public partial record Announcement : IEntity<AnnouncementIdentifier>, IEntityWithPublicRead<Announcement>
{
    public required partial AnnouncementIdentifier Identifier { get; init; }
    public required bool IsAvailableForPublicRead { get; init; }

    public required string Contents { get; init; }

    public Announcement OpenPublicRead()
    {
        return this with {
            IsAvailableForPublicRead = true,
        };
    }

    public Announcement ClosePublicRead()
    {
        return this with {
            IsAvailableForPublicRead = false,
        };
    }
}
```

HasIdentifier необходимо указывать из за особенновстей EFCore и ComplexProperty, которые не позволяют Identifier быть
первичным ключом. Поэтому, мы генерируем все свойства ``AnnouncementIdentifier`` внутри [Announcement](#идентификаторы),
а само свойство Identifier игнорируем на стороне БД.

Наша задача - показать наши анонсы миру - поэтому, мы добавляем интерфейс ``IEntityWithPublicRead`` и реалезуем
``OpenPublicRead`` и ``ClosePublicRead``. Пока что реализация методов - бойлерплейт, но уже в [планах](#todo) перенести
ее в генератор кода!

Долбавим пару DTO, которые будут использоваться как запросы от пользователя:

```csharp
public class CreateAnnouncementRequest
{
    public required string Contents { get; init; }
}

public class UpdateAnnouncementContentsRequest
{
    public required string Contents { get; init; }
}
```

И заодно напишем DbContext для всего модуля:

```csharp
public class ServerDbContext(DbContextOptions<ServerDbContext> options) : TaggedDbContext<ServerDbContext, ServerTag>(options)
{
    public DbSet<Announcement> Announcements { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder
            .Entity<Announcement>()
            .ToTable("announcements")
            .ConfigureComplexIdentifierKey<AnnouncementIdentifier, Announcement>();
    }
}
```

В этом блоке кода необычное только ``TaggedDbContext`` и ``ConfigureComplexIdentifierKey``.

Начнем с ``TaggedDbContext`` и ``ServerTag``. ``ServerTag`` - пустой маркерный класс, он используется для разделения
разных модулей приложения. Разные модули могут использовать разные контексты баз данных! ``TaggedDbContext`` - пустой
базовый класс, необходимый для указания тэга и разделения контекстов.

``ConfigureComplexIdentifierKey`` как раз и отвечает за указание первичного ключа по всем свойствам ``TIdentifier``.

``ServerTag`` в фреймворке повсеместно, не только в БД. Он нужен как раз для того, чтобы интерфейсы разных модулей не
пересекались.

Перед регистрацией модуля, необходимо зарегистрировать зависимости:

```csharp
private static void SetupMapper(IServiceCollection serviceCollection)
{
    serviceCollection
        .AddSingleton<TypeAdapterConfig>(_ =>
            {
                var config = TypeAdapterConfig.GlobalSettings;
                config.Default.RequireDestinationMemberSource(true);
                config.Compile(true);
                return config;
            }
        )
        .AddSingleton<IMapper, ServiceMapper>();
}

public static IServiceCollection SetupMediatR(IServiceCollection serviceCollection, MediatRConfig config)
{
    serviceCollection
        .AddMediatR(builder =>
            {
                builder.TypeEvaluator = type => false;
                builder.Lifetime = ServiceLifetime.Scoped;
                builder.RegisterGenericHandlers = false;
                builder.NotificationPublisher = new TaskWhenAllPublisher();
                builder.AutoRegisterRequestProcessors = false;

                foreach (var assembly in config.AssembliesToScanHandlers)
                    builder.RegisterServicesFromAssemblies(assembly);
            }
        )
        .Decorate<IMediator, TransactionMediatorDecorator>();

    return serviceCollection;
}
```

Настройки могут быть произвольными, но медиатор нужно задекорировать в ``TransactionMediatorDecorator``. Он
перенаправляет события в транзакцию, если она запущена. В момент завершения транзакции, она резолвит медиатор и
отправляет все удерживаемые уведомления.

Авторизация:

```csharp
private static void SetupAuthorization(IServiceCollection serviceCollection)
{
    serviceCollection
        .AddAuthorization(authBuilder =>
            {
                authBuilder.AddPolicy("LoggedInPolicy", policy => policy.RequireAuthenticatedUser());
            }
        )
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer();
}
```

Хотя в этом гите и есть проекты ``Ukinee.Users.*``, это MVP. Подразумевается, что вы реализуете пользователей
самостоятельно. Фрейворк использует интерфейс ``IUserContextProvider`` для изначального получения ``UserContext``:

```csharp
public readonly record struct UserContext
{
    public required Guid Guid { get; init; }
    public required bool IsAuthenticated { get; init; }

    public required ImmutableArray<string> Roles { get; init; }

    public required string? Name { get; init; }
    
    ...
```

Фреймворку нужна только эта информация о пользователе. Контекст должен жить только в рамках запроса и не должен
храниться как есть. ``Guid`` - идентификатор пользователя.

### Введение в DSL

Теперь, к регистрации самого модуля.

```csharp
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection SetupServer(IConfiguration configuration, ServerConfig config)
        {
            var policy = RegistrationPolicy.Server;

            var mediatRConfig = new MediatRConfig {
                AssembliesToScanHandlers = [typeof(SetupMinimalServerExample).Assembly], // он ОЧЕНЬ хочет сканировать проекты
            };

            SetupMapper(serviceCollection);
            SetupMediatR(serviceCollection, mediatRConfig);
            SetupAuthorization(serviceCollection);
            serviceCollection.AddSignalR();

            serviceCollection.AddSingleton<TimeProvider>(_ => TimeProvider.System);

            serviceCollection
                .AddScoped<UnitOfWorkFactory>()
                .AddScoped<IUnitOfWorkProvider>(sp => sp.GetRequiredService<UnitOfWorkFactory>())
                .AddScoped<IUnitOfWorkFactory>(sp => sp.GetRequiredService<UnitOfWorkFactory>());

            configuration.AddJsonFile(config.ConntectionStringPath, optional: false, reloadOnChange: true); // любой способ регистрации ConntectionString в конфигурации 

            serviceCollection
                .AddScoped<IUserContextProvider, TestingUserContextProvider>(_ => new TestingUserContextProvider(UserContext.Test));
            
            var announcementPolicy = AuthorizationPolicyDefinition.GuestReadAndOwnerEdit<AnnouncementIdentifier, Announcement>(identifier => identifier.UserGuid);

            serviceCollection
                .RegisterModule<ServerTag>()
                .DatabaseDatasource.Register<ServerDbContext>((DbContextOptionsBuilder builder) =>
                    {
                        var connectionString = configuration.GetConnectionString(config.DatabaseName);

                        builder.UseNpgsql(connectionString);
                    }
                )
                .LocalContexts.AddHonestDeletionDatabaseContext<AnnouncementIdentifier, Announcement>(contextConfigurator => contextConfigurator
                    .Ddd.RegisterWithDatabaseRepository(dddConfigurator => dddConfigurator
                        .SetAccessPolicy(announcementPolicy)
                        .SetCreateUseCase<CreateAnnouncementRequest, AnnouncementFactory>()
                        .AddUpdateUseCase<UpdateAnnouncementContentsRequest, AnnouncementFactory>()
                    )
                    .ApiServer.Register<AnnouncementResponse>(
                        config.ApiBaseRoute,
                        announcementPolicy,
                        apiServerConfigurator => apiServerConfigurator
                            .RegisterCrud<CreateAnnouncementRequest, UpdateAnnouncementContentsRequest>()
                            .AddGroupConfiguration(builder => builder.AllowAnonymous())
                    )
                )
                .Build(policy);

            return serviceCollection;
        }
    }
```

Сначала обязательно нужно объявить политику доступа. Подробнее про них [тут](#политики-доступа). Сейчас достаточно
понимать, что реализация ``GuestReadAndOwnerEdit`` означает, что у сущности есть владелец, указанный в Id (нужно указать
где он именно в параметре метода) и что эндпоинты для чтения не требуют даже авторизации.

Весь DSL строгостипизирован, требует вызовов обязательных методов до вызова пользовательских на уровне компиляции.

Первая фабрика DSL создается через метод ``RegisterModule``. Он как раз и требует объявления тэга, который будет
использоваться сервисами. Регистрировать несколько модулей с одинаковыми тегами в одном приложении нельзя.
Переименовывание тэга после создания миграции может сломать верисонирование. Подробнее смотри в классе
``TaggedDbContext<TContext, TTag>``.

Далее, у фабрики, возвращаемой ``RegisterModule``, мы хотим определить источник данных. Это будет база данных. Обратимся
к ``DatabaseDatasource`` и выполним классическую настройку контекста. В контексте должны быть ``DbSet`` всех
сущностей, объявляемых в этом модуле DSL, и которые используют БД как хранилище. Компилятор не труебует вызова этого
метода, но если используется ``Add*DatabaseContext`` его вызов обязателен.

И, наконец, описание самой сущности.

Мы хотим, чтобы именно этот хост владел сущностью. Для этого мы объявим ее как
локальную - ``LocalContexts`` и пометим ее честным удалением - ``AddHonestDeletionDatabaseContext``. Сам по себе
метод ничего не регистрирует, если оставить лямбду ``AddHonestDeletionDatabaseContext`` пустой, это будет no-op. Вся
регистрация разбита мо модулям ``contextConfigurator``.

Глубокое описание принципа работы DSL и детали по ``RegistrationPolicy`` в [этом блоке](#dsl-и-как-оно-устроено),
а [здесь](#dsl) пользовательское описание.

Описание работы ``UnitOfWorkFactory`` и траназакций: [пользовательское](#транзакции), [глубокое](#транзакции-1).

### Описание Ddd в DSL

Базовые [операции](#операции) регистрируются в модуле ``Ddd``.
``Ddd``, в зависимости от контеста, содержит разные типы фабрик и, соответственно, разные методы, задающие разное
базовое поведение. У БД реализации есть только один метод - ``RegisterWithDatabaseRepository``. Его делегат оставить
пустым не получится - нам нужно настроить правила доступа - передадим политику. Это единственный обязательный шаг, если
ничего больше не вызывать, в DI попадут сервсы для чтения сущностей, обновления по делегату, а так же для удаления
сущностей (регистрируются внутри ``RegisterWithDatabaseRepository``).

Если оставить как есть, будут зарегистрированы следующие интерфейсы:

- ``IEditableTrackedRepository<TIdentifier, TEntity>``
- ``ITrackedRepository<TIdentifier, TEntity>``
- ``IDeltaUpdateEntityUseCase<TIdentifier, TEntity>``
- ``IEntityDeltaUpdater<TIdentifier, TEntity>``
- ``IRemoveEntityUseCase<TEntity>``
- ``IRemoveEntityUseCase<TIdentifier, TEntity>``
- ``IEntityRemover<TIdentifier, TEntity>``
- ``IGetAllEntityUseCase<TIdentifier, TEntity>``
- ``IAllEntityReader<TIdentifier, TEntity>``
- ``IGetEntityUseCase<TIdentifier, TEntity>``
- ``IIdentifierReader<TIdentifier, TEntity>``
- ``ITrackedReader<TIdentifier, TEntity>``
- ``ISpecificationReader<TIdentifier, TEntity>``

Для добавления возможности содать сущность из DTO, вызовем ``AddCreateUseCase``. У него есть 2 перегрузки. Первая
требует только DTO и фабрику, которая превращает этот DTO в сущность. Вторая добавляет еще и FluentValidator для DTO.
Так же, есть версии методов ``AddAsyncCreateUseCase``, если фабрика требует асинхронного контекста.

Каждый вызов ``AddCreateUseCase`` или ``AddAsyncCreateUseCase`` добавляет следующие интерфейсы в DI:

- ``IEntityCreator<TEntity, TCreatePayload>``
- ``ICreateEntityUseCase<TEntity, TCreatePayload>``

И то, что это локальный контекст регистрирует дополнительно

- ``ICreateEntityFactory<TCreatePayload, TEntity>`` / ``IEntityAsyncCreateFactory<TCreatePayload, TEntity>``
- ``IValidator<IEnumerable<TCreatePayload>>`` - если не указывать, будет использована no-op версия;

Аналогично с ``AddUpdateUseCase``, только без асинхронной версии. Тоже есть перегрузка без валидации, требует DTO и
фабрику.

При вызове регистрируется:

- ``IEntityPayloadUpdater<TIdentifier, TEntity, TUpdatePayload>``
- ``IUpdateEntityUseCase<TIdentifier, TEntity, TUpdatePayload>``

И локальный контекст регистрирует:

- ``IUpdateEntityFactory<TUpdatePayload, TEntity>``
- ``IValidator<IEnumerable<TUpdatePayload>>`` - если не указать - no-op версия;

Используемая фабрика:

```csharp
public class AnnouncementFactory : ICreateEntityFactory<CreateAnnouncementRequest, Announcement>, IUpdateEntityFactory<UpdateAnnouncementContentsRequest, Announcement>
{
    public Announcement Create(UserContext userContext, CreateAnnouncementRequest payload)
    {
        return new Announcement {
            Identifier = AnnouncementIdentifier.Create(userContext, Guid.NewGuid()),
            IsAvailableForPublicRead = true,
            Contents = payload.Contents,
        };
    }

    public Announcement Update(UserContext userContext, Announcement old, UpdateAnnouncementContentsRequest payload)
    {
        return old with {
            Contents = payload.Contents,
        };
    }
}
```

Подробнее про то, как и чем пользоваться, тут: [операции](#операции).

### Описание ApiServer в DSL

Теперь, ApiServer. Это опциональный модуль, конфигурация которого доступна только для локальных сущностей.

Метод ``Register`` с generic принимает DTO, в который будет мапится результат операции. Есть перегрузка ``Register`` без
generic, которая возвращает пользователь ``TEntity`` целиком. Если нужно ограничить пользователю видимость определенных
полей - нужно сделать отдельный DTO.

Первый параметр - base route. Например, ``api/v1``.

Второй - наша политика. Можно передать null, тогда фреймворк не
будет дополнительно настраивать авторизацию и поведение не будет отличаться от дефолтного MinimalApi.

Есть отдельный метод ``AddGroupConfiguration``, который настраивает ``RouteGroupBuilder``. Последовтаельные вызовы
аггрегируют Action. В примере выше я использую ``.AddGroupConfiguration(builder => builder.AllowAnonymous())`` только из
за того что нет зарегистрированной инфраструктуры аутетнтификации пользователя.

Сам билдер дает возможность настраивать эндпоинты отдельно:

- ``WithFindMany`` - получения множества сущностей по id; streaming POST* эндпоинт; не возвращает ненайденные сущности;
- ``WithGetAll`` - получение всех доступных сущностей; streaming GET эндпоинт;
- ``WithGet`` - получение конкретной сущности по id; GET эндпоинт;
- ``WithDelete`` - удаление одной сущнсоти; DELETE запрос;
- ``WithDeleteRange`` - удаление множества сущностей в рамках транзакции; DELETE запрос;
- ``WithThrowingOnDuplicatesCreate`` - он же GetOrCreate - создание одной сущности; POST запрос;
- ``WithThrowingOnDuplicatesCreateMany`` - создание множества сущностей в рамках транзакции; POST запрос;
- ``WithEnsureExistsCreate`` - создание одной сущности; ID необходимо знать до создания, no-op, если сущность
  существует; POST запрос;
- ``WithEnsureExistsCreateMany`` - создание множества сущностей в рамках транзакции; создает отсутствующие сущности;
  POST запрос;
- ``AddUpdate`` - обновление одной сущности по id; PUT запрос;
- ``AddUpdateMany`` - обновление множества сущностей; PUT запрос;

\* - ``WithFindMany`` POST потому что принимает коллекцию ID в теле запроса.

Или использовать групповые вызовы:

- ``RegisterRead`` - вызывает ``WithFindMany``, ``WithGetAll``, ``WithGet``
- ``RegisterDelete`` - вызывает ``WithDelete``, ``WithDeleteRange``
- ``RegisterUpdate<TPayload>`` - вызывает ``AddUpdate``, ``AddUpdateMany``
- ``RegisterCreate<TPayload>`` - вызывает ``WithThrowingOnDuplicatesCreate``, ``WithThrowingOnDuplicatesCreateMany``
- ``RegisterCrud<TPayload>`` - вызывает ``RegisterRead``, ``RegisterDelete``, ``RegisterUpdate<TPayload>``,
  ``RegisterCreate<TPayload>``
- ``RegisterCrud<TCreatePayload, TUpdatePayload>`` - вызывает ``RegisterRead``, ``RegisterDelete``,
  ``RegisterUpdate<TUpdatePayload>``, ``RegisterCreate<TCreatePayload>``

Теперь, после запуска приложения, Scalar (или Swagger) показывает следующие эндпоинты:

```
post   /api/v1/Announcement/find
get    /api/v1/Announcement/{UserGuid}/{Guid}
delete /api/v1/Announcement/{UserGuid}/{Guid}
get    /api/v1/Announcement
delete /api/v1/Announcement/deleteMany
post   /api/v1/Announcement/createManyWithCreateAnnouncementRequest
post   /api/v1/Announcement/createWithCreateAnnouncementRequest
put    /api/v1/Announcement/updateWithUpdateAnnouncementContentsRequest/{UserGuid}/{Guid}
put    /api/v1/Announcement/updateManyWithUpdateAnnouncementContentsRequest
```

## Используемые технологии // Обязательные зависимости

- Mapster - Используется для мапинга сущностей при ответе пользователю. Важно знать как мапит сущности по совпадению
  имен;
- MediatR - Используется только как In-Memory signal bus для связи модулей; Прямое использование необязательно;
- SignalR - Зависимость обязательная, но используется в опциональном модуле SignalR. Используется и клиент и
  сервер;
- FluentValidation - для создания валидаторов сущностей; Валидаторы опциональны;
- Scrutor - в проекте с DSL используется только для декорирования сервисов;
- LinqKit - необходим фреймворку для объединения Expression;
- Ardalis.Specification - спецификации для репозиториев и сервисов сущностей;

## Высокоуровневое описание фич // Что можно

Основной функционал фреймворка заключается в бесшовной работе над сущностями, независимо от места их хранения. Что
локальные, что удаленные сущности имеют общий набор [интерфейсов](#операции) для работы.

### Сущности

Для всех операций в фреймворке, сущности требуется реализация интерфейса ``IEntity<TIdentifier>``, где TIdentifier может
быть комплексным типом, и налииче атрибута ``[HasIdentifier(typeof(TIdentifier))]``.

Помимо этого, есть дополнительные интерфейсы, дополняющие сущность функционалом:

- ``IEntityWithSoftDelete`` - помечает сущность доступной для мягкого удаления;
- ``IEntityWithPublicRead`` - добавляет свойство ``IsAvailableForPublicRead`` и позволяет быть прочитанной не
  только владельцем, но и другими пользователями (определяется политикой доступа);
- ``IEntityWithSharedAccess`` - добавляет свойство ``SharedUserGuids`` - дает доступ к сущности конкретным
  пользователям. Функционал определяется политикой доступа;

### Операции

Интерфейсы, применяемые в контексте транзакции для проведения CRUD операций (далее ``EntityService``):

- ``IEntityCreator``, ``IEntityEnsureExistsCreator``
- ``IIdentifierReader``, ``IAllEntityReader``, ``ISpecificationReader``\*, ``ITrackedReader``\*
- ``IEntityPayloadUpdater``, ``IEntityDeltaUpdater``\*
- ``IEntityRemover``

\* Досупны только для локальных сущностей

Юзкейсы, выполняющие единичную операцию. Классы сами начинает транзакцию, выполняют операцию и коммитят изменения (далее
``UseCase``):

- ``ICreateEntityUseCase``
- ``IGetEntityUseCase``, ``IGetAllEntityUseCase``
- ``IUpdateEntityUseCase``
- ``IRemoveEntityUseCase``

Если задача - выполняить атомарную операцию здесь и сейчас, достаточно использовать UseCase.
Если нужно выполнить сложную бизнес операцию, сначала необходимо запустить транзакцию и использовать ``IEntityServie``:

```csharp
public class TransactionExampleUseCase(
    IUnitOfWorkFactory unitOfWorkFactory,
    IIdentifierReader<ConversationIdentifier, Conversation> conversationReader,
    IEntityDeltaUpdater<ContactIdentifier, Contact> contactUpdater,
    IEntityPayloadUpdater<ConversationIdentifier, Conversation, SetConversationCounterRequest> conversationUpdater
)
{
    public async Task Execute(UserContext userContext, ConversationIdentifier identifier, CancellationToken token)
    {
        var conversation = await conversationReader.GetByIdAsync(userContext, identifier, token);

        var counter = 1;

        await using (var uow = unitOfWorkFactory.Create<MessengerTag>())
        {
            foreach (var contactIdentifier in conversation.ContactIdentifiers)
            {
                await contactUpdater.Update(
                    userContext,
                    contactIdentifier,
                    old => old with { Profile = old.Profile with { Username = $"{old.Profile.Username}_{counter}", }, },
                    token
                );

                counter++;
            }

            var conversationRequest = new SetConversationCounterRequest {
                Count = counter,
            };

            await conversationUpdater.UpdateAsync(userContext, identifier, conversationRequest, token);

            await uow.CommitAsync(token);
        }
    }
}
```

Транзакция начинается через метод ``IUnitOfWorkFactory.Create<TTag>``, где TTag тот же тэг, что использовался в
``.RegisterModule<TTag>()``. Это позволяет базе данных и Transacted-In-Memory, зарегистрированных в рамках модуля,
использовать одну транзакцию. При этом, изменяющие операции над обычными In-Memory или Remote сущностями, бросают
ошибку, если выполняются в контексте транзакции (даже если зарегистрированы в том же модуле). Подробнее про механизм
в [декораторах](#декораторы).

На данный момент, фреймворк не поддерживает транзакции между разными модулями, даже если они фактически настроены на
общий сервер БД или другой модуль зарегистрировал сущность как Transacted-In-Memory.

В примере выше, если ``Conversation`` зарегистрирован как обычный ``InMemory`` или ``Remote``, операция упадет с
``NotSupportedException``. Тогда вызов необходимо вынести за транзакцию.

Транзакции реализованы через Ambient Scoped сервис.

### Локальные сущности // LocalContexts

В этом блоке более подробное объяснение примерма из [quick-start](#quick-start).

Локальность сущности подразумевает ответственность за хранение, проверку прав доступа, валидацию операций.
Запросы на создание и обновление требуют фабрик и опциональных валидаторо, в которых происходит создание сущностей.
Весь пайплайн выглядит так:

[Тут нужна картинка с абстрактным пайплайном локальной сущности]

Локальные сущности реализованы в 2 видах:

- Хранимые в БД
- Хранимые в памяти

Хранимые в БД сущности. Использование IEntity сервисов вне транзакции не вызовет сохранение контекста (SaveChanges
вызывает только ``IUnitOfWork.CommitAsync``). Для выполнения одиночной операции следует использовать
UseCase (он самостоятельно открывает транзакцию, вызывает IEntity сервис, коммитит изменения).

У типа сущности есть вариации в поведении:

- Хранимые в БД сущности могут быть для мягкого или честного удаления;
- Хранимые в памяти мущности могут быть
    - ``RegisterAsInMemoryState`` - аналогично честной БД реализации: изначально сущностей нет, создание, удаление и
      обновление применяются как обычно. Транзакции не поддерживаются.
    - ``RegisterAsTransactedInMemoryState`` - аналогично ``RegisterAsInMemoryState``. Транзакции поддерживаются, хотя и
      не симметричны EFCore. Пока без поддержки оптимистичных блокировок - LastWriteWins.
    - ``RegisterWithLocalStaticDataSource<TWeight, TDataSource>`` - подразумевает существование неизменяемого списка
      сущностей, который загружается в репозиторий в старте приложения из TDataSource.

Хранимые в памяти сущности, зарегистрированные через ``RegisterAsInMemoryState`` НЕ поддерживают участие в транзакиях.
Реализация UseCase для них транзакцию не начинает, а IEntity сервисы выполняют операцию сразу. Попытка использования
IEntity сервиса для In-Memory сущности в контексте транзакции приведет к исключению.

- Для примера DSL смотри [quick-start](#quick-start);
- Для разбора локального Ddd смотри [Описание Ddd в DSL](#описание-ddd-в-dsl);
- Для разбора ApiServer смотри [Описание ApiServer в DSL](#описание-apiserver-в-dsl);

#### Описание SignalRServer в DSL

Опциональный модуль SignalRServer (только для локальных сущностей) регистрирует сервисы для real-time синхронизации
состояния.

Пример DSL:

```csharp
.LocalContexts.AddHonestDeletionDatabaseContext<AnnouncementIdentifier, Announcement>(contextConfigurator => contextConfigurator
    .Ddd.RegisterWithDatabaseRepository(...)
    .ApiServer.Register<AnnouncementResponse>(...)
    .SignalRServer.Register<THub, AnnouncementSignalRRequest, AnnouncementResponse>(signalRConfigurator => signalRConfigurator
        .SetGroupResolver<AnnouncementSignalRRouteResolver>()
        .SetAccessValidationService<AnnouncementSignalRAccessValidator>()
    )
)
```

Принцип работы:

1. Пользователь подключается к Hub.
2. Пользователь отправляет сообщение с данными для подписки (в примере это ``AnnouncementSignalRRequest``). Оно должно
   попасть в ``IHubService``. Поскольку делать несколько Hub - плохая практика (нельзя через фреймворк создать несколько
   дженерик хабов), донести событие о подписке до сервиса - задача разработчика;
3. Сообщение валидируется сервисом из ``SetAccessValidationService``;
4. Сообщение попадает в сервис из ``SetGroupResolver``, где по определенному набору правил превращается в группы для
   SignalR;

``SetGroupResolver`` должен регистрировать как правила для запроса на подписку (``RequestRule``), так и правила для
сущности ``EntityRule``. RequestRule превращает запрос в правило, а при событии с сущностью (в рантайме),
она в ``EntityRule`` должна создать те же группы, что и ``RequestRule``, чтобы событие ушло пользователю.

Пример реквеста и резолвера:

[Нужно переписать SignalR  привести пример]

#### Валидация данных

В качестве сервиса валидации используется FluentFalidation IValidator<IEnumerable<TRequest>>.
IEntity сервисы создания и обновления валидируют переданные запросы и бросают исключения, если запросы не прошли
валидацию.

Встроенный ExceptionHandler обрабатывает ошибку валидации и возвращает HttpValidationProblemDetails.

#### Политики доступа

Построчная валидация доступа основана на интерфейсах

- ``IEntityCreateAccessExpressionProvider``
- ``IEntityReadAccessExpressionProvider``
- ``IEntityUpdateAccessExpressionProvider``
- ``IEntityDeleteAccessExpressionProvider``

Каждый из интерфейсов имеет один метод -
``public Task<Expression<Func<TEntity, bool>>> GetXXXExpression(UserContext userContext)`` - асинхронную
фабрику Expression. Это выражение обязано быть компилируемым в SQL код (кроме Create версии - она выполняется в памяти).

При чтении, эти фильтры применяются до переданных спецификаций.

Предоставляемая фреймворком версия сервисов строит условия на основе создаваемых политик (статический класс
``AuthorizationPolicyDefinition`` или ручное создание ``AuthorizationPolicyDefinition<TIdentifier, TEntity>``).

Для работы с сущностями есть несколько поддерживаемых режимов:

- Guest - произвольный доступ. Только для чтения;
- LoggedIn - пользователю достаточно быть авторизованным (``UserContext.IsAuthenticated``). Только для чтения и
  создания;
- OwnerOnly - в id сущности указан id владельца. Если этот id совпадает с ``UserContext.Guid`` - доступ есть;
- SharedUsers - в сущности есть свойство ``SharedUserGuids``. Если ``UserContext.Guid`` есть в списке - доступ есть;
- Roles - доступно всем в указанных группах ролей (``UserContext.Roles``);

Guest и LoggedIn требуют наличия ``IEntityWithPublicRead`` на сущности и ``IsAvailableForPublicRead == true``.

Каждый из указанных режимов поддерживает дополнительную фильтрацию по роли (кроме Guest).
На каждую категорию функционала можно повесить несколько правил.
Правила проверяются через OR (достаточно одного правила для получения доступа), а роли в рамках правила
проверяются через AND (проверяется И LoggedIn И наличие правил).

Примеры.

* LoggedIn с ролью [VIP] означает, что сущность могут читать все, у кого есть роль [VIP] И у сущности
  ``IsAvailableForPublicRead == true`` (LoggedIn и Guest ТРЕБУЮТ налчие IEntityWithPublicRead).
* Roles с ролью админа означает, что сущность могут читать все пользователи с ролью админа.
* Roles c [Admin], LoggedIn с [VIP], и OwnerOnly для чтения означает:
    * Все [Admin] могут читать сущность;
    * Все [VIP] могут читать сущность, если у нее ``IsAvailableForPublicRead == true``;
    * Владелец может читать сущность;

Эти же фильтры применяются к остальным операциям - созданию, обновлению, удалению.

Важно: Этот же механизм проверок добавляет проверку ``DddExpressionFactory.Exists`` для soft-delete сущностей.
Поэтому, при использвании ``SetAccessExpressionProvider<>`` вместо ``SetAccessPolicy`` необходимо убедиться, что эта
проверка есть.

#### Honest & Soft Deletion

Сущности, помеченные ``IEntityWithSoftDelete`` и зарегистрированные через DSL меняют свое поведение при удалении.
Свойтсво ``DeletedAt`` становится заполненным и сущность перестает попадать в выборки чтения.

Никаких дополнительных оптимизаций фреймворк не предоставляет - сущность остается в той же таблице, просто пропадает из
выборок.

### Удаленные сущности // ApiClientContexts

#### Кэширование

### Детально про DSL

Необходимость - апелляция к детальному знанию всей архитектуры, громоздкий DI, который легко заменяется DSL

#### Все случаи магической магии о которых нужно и не нужно знать

#### RegistrationPolicy

#### Иерархия билдеров

## Низкоуровневое описание фич // Как и почему так

### Идентификаторы

Идентификаторы и их кодогены. Истоки в EF и Expression выборки

```csharp
public partial record Announcement
{
    public System.Guid UserGuid { get; private set; }
    public System.Guid Guid { get; private set; }
    public required partial Ukinee.Infrastructure.Ddd.Tests.Domain.Announcements.AnnouncementIdentifier Identifier
    {
        get => new() { UserGuid = this.UserGuid, Guid = this.Guid };
        init => (this.UserGuid, this.Guid) = (value.UserGuid, value.Guid);
    }
}
```

### Декораторы

Подход к расширению через dsl, симметрии через декораторы с примером на транзакциях (неявное) и с примером на кеше (
явное)

### Транзакции

IUOWFactory IUOWProvider

## Компромиссы

- падение на невалидных транзакциях (Для InMemory или remote сущностей)
- отсутсвие поддержки между контекстных транзакций // пока что разные БД не поддерживаются. Есть вариант добавить
  транзакции, если они на одном сервере, но выглядит как дополнительная магия
- всегда полный респонс в SignalR. Может, можно отправлять примененный Payload, id и версию, если они есть, а если нет -
  полный респонс?
- Сейчас нет возможности дать дополнительное имя эндпоинту, поэтому при попытке в одном приложении сделать эндпоинт и
  /v1 и /v2 , приложение упадет с ошибкой о дублирующимя имени MinimalApi, хотя route будет отличаться.

## TODO

Анализатор кода, требующий вызов ``DatabaseDatasource`` для БД и ApiDatasource для API.

Генератор кода валидации транзакций. Он должен смотреть все use case, где есть транзакции, собирать какие entity
редактируются, генерировать что то, с помощью чего при запуске можно проверить корректность используемых сущностей (
гарантировать что они из одного модуля / разных модулей с одной БД, что используется транзакционная in memory bd и не
api)

Поддержка IEntityWithVersion. Это позволит добавить в транзакционный in memory репозиторий оптимистичные блокировки, не
инвалидирующий кэш на клиенте, оптимизацию SignalR (присылать версию, id)

Outbox / Inbox + возможно, поддержку для transacted in memory write behind (хотя тут проблема с тем что у in memory нет
валидации отдельных полей и мы упадем в рантайме, если несовпадает структуа или провалится валидация). Что на счет WAL?
Типа, применяем X в транзакции с inMemory. Но тогда проблема с последовательностью применений операций.

Observability

Api-Key auth для Service-Service взаимодействия

Кодоген реализации интерфейсов, удалить HasIdentifier и перейти на IEntity<AnnouncementIdentifier>

Так же на клиенте нужно переподписываться при переподключении. Добавить тесты DefaultRouteResolver. 

Уточнить, что SoftDeleted сущности все еще эмитят события.
