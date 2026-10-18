DDDSL
===

Демонстрационный проект с примером построения DDD+CQRS приложений с симметричным клиентом и сервером. Концентрирует в
себе подход, используемый в личных приложениях.
Покрывает 90% типовых требований и может использоваться для быстрого прототипирования шаблонных CRUD приложений.

## Содержание

## Quick Start

## Используемые технологии // Обязательные зависимости

## Высокоуровневое описание фич // Что можно

Фреймворк предоставляет возможность работы с локальным (база данных и in-memory) и удаленными сущностями через общий
набор интерфейсов и настройкой через DSL.

Фреймворк построен на основе дженериков, поэтому рекомендуется использовать c# alias.

Во всех примерах сущность и на клиенте и на сервере одинакова, а общение происходит через ``TResponse``. Т.е в примерах
сервер хранит TEntity, мапит в TResponse, клиент восстанавливает TEntity из TResponse. Это сделано исключительно в
качестве
примера, в реальном сценарии модель клиента может отличаться от модели сервера.

### Сущности

Для всех операций в фреймворке, сущности требуется реализация интерфейса ``IEntity<TIdentifier>``, где TIdentifier может
быть комплексным типом и налииче атрибута ``[HasIdentifier(typeof(TIdentifier))]``.

Помимо этого, есть дополнительные интерфейсы, дополняющие сущность функционалом:

- ``IEntityWithSoftDelete`` - помечает сущность доступной для мягкого удаления;
- ``IEntityWithPublicRead`` - позволяет сущности быть прочитанной не только владельцем, но и другими пользователями (
  определяется политикой доступа);
- ``IEntityWithSharedAccess`` - дает доступ к сущности конкретным пользователям. Функционал определяется политикой
  доступа;

### Операции

Интерфейсы, применяемые в контексте транзакции для проведения CRUD операций (далее ``IEntity сервисы``):

- ``IEntityCreator``, ``IEntityEnsureExistsCreator``
- ``IIdentifierReader``, ``IAllEntityReader``, ``ISpecificationReader``*
- ``IEntityPayloadUpdater``, ``IEntityDeltaUpdater``*
- ``IEntityRemover``

Юзкейсы, выполняющие единичную операцию. Классы сами начинает транзакцию, выполняют операцию и коммитят изменения:

- ``ICreateEntityUseCase``
- ``IGetEntityUseCase``, ``IGetAllEntityUseCase``
- ``IUpdateEntityUseCase``
- ``IRemoveEntityUseCase``

Для создания транзакции используется IUnitOfWorkFactory.

\* Досупны только для локальных сущностей

### DSL как Guardrails

Все интерфейсы фреймворка, работающие с сущностями, используют дженерики. Для удобной регистрации в DI предусмотрен DSL
builder. Пример для удаленных сущностей (см. детали в [LocalContexts](#локальные_сущности)
и [ApiClientContexts](#удаленные_сущности), тут описание принципа):

```csharp
serviceCollection
    .RegisterModule<TestingTag>()
    .ApiDatasource.Register(httpClientBuilder =>
        ...
    )
    .ApiClientContexts.AddRemoteContext<AnnouncementIdentifier, Announcement>(contextConfigurator => contextConfigurator
        .Ddd.Register<AnnouncementIdentifierParams, AnnouncementResponse>(dddConfigurator => dddConfigurator
            ...
            .SetCreateUseCase<CreateAnnouncementRequest>()
            .AddUpdateUseCase<UpdateAnnouncementContentsRequest>()
        )
    )
```

Вся регистрация начинается с вызова RegisterModule. Это необходимо для задания модулю маркерного дженерика. Это пустой
класс, используемый для разделения сервисов по модулям и определения корректности транзакции.
При использовании провйдера данных (т.е. при использовании удаленных сущностей или хранимых в БД локальных сущностей),
их необходимо зарегистрировать, используя ``ApiDatasource.Register`` или ``DatabaseDatasource.Register``.

Сами сущности регистрируются в ``.ApiClientContexts`` или ``.LocalContexts``. Последующие методы определяют поведение
контекста.

Независимо от типа сущности (удаленная или локальная), при вызове ``Ddd.Register``, регистрируются сервисы для чтения и
удаления сущностей (для всего списка регистрируемых сервисов, смотри XML документацию вызываемого метода).

Метод ``SetCreateUseCase<>()`` вызывается один раз (он перезаписывает свое существующее поведение). Регистрирует сервисы
создания сущности.
Методов ``AddUpdateUseCase<>()`` может вызываться несколько, каждый их них регистрирует сервис для применения указанного
запроса к сущности.

### Локальные сущности // LocalContexts

Локальность сущности подразумевает ответственность за хранение, проверку прав доступа, валидацию операций.
Запросы на создание и обновление требуют валидаторов и фабрик, в которых происходит создание сущностей. Весь пайплайн
выглядит так:

[Тут нужна картинка с абстрактным пайплайном локальной сущности]

Локальные сущности реализованы в 2 видах:

- Хранимые в БД
- Хранимые в памяти

Хранимые в памяти сущности НЕ поддерживают участие в транзакиях. Реализация UseCase для них транзакцию не
начинает, а IEntity сервисы выполняют операцию сразу. Попытка использования IEntity сервиса для In-Memory сущности в
контексте транзакции приведет к исключению. Реализация ``TransactedInMemoryRepository`` пока существует только в тестах,
после реализации версионирования, репозиторий планируется перенести в основной DSL как опциональный вариант регистрации.

Хранимые в БД сущности. Использование IEntity сервисов вне транзакции не вызовет сохранение контекста (SaveChanges
вызывает только ``IUnitOfWork.CommitAsync``). Для выполнения одиночной операции следует использовать
UseCase (он самостоятельно открывает транзакцию, вызывает IEntity сервис, коммитит изменения).

У типа сущности есть вариации в поведении:

- Хранимые в БД сущности могут быть для мягкого или честного удаления;
- Хранимые в памяти мущности могут быть
    - ``RegisterAsInMemoryState`` - аналогично честной БД реализации: изначально сущностей нет, создание, удаление и
      обновление применяются как обычно. Транзакции не поддерживаются.
    - ``RegisterAsTransactedInMemoryState``* - аналогично RegisterAsInMemoryState. Транзакции поддерживаются.
    - ``RegisterWithLocalStaticDataSource<TWeight, TDataSource>`` - подразумевает существование неизменяемого списка
      сущностей, который загружается в репозиторий в старте приложения из TDataSource.

\* - планируется после реализации версионирования.

Регистрация пайплайна в DSL выглядит так:

```csharp
var announcementPolicy = AuthorizationPolicyDefinition.GuestReadAndOwnerEdit<AnnouncementIdentifier, Announcement>(identifier => identifier.UserGuid);

serviceCollection
    .RegisterModule<ServerTag>()
    .LocalContexts.AddInMemoryContext<AnnouncementIdentifier, Announcement>(contextConfigurator => contextConfigurator
        .Ddd.RegisterAsInMemoryState(dddConfigurator => dddConfigurator
            .SetAccessPolicy(announcementPolicy)
            .SetCreateUseCase<CreateAnnouncementRequest, CreateAnnouncementRequestValidator, AnnouncementFactory>()
            .AddUpdateUseCase<UpdateAnnouncementContentsRequest, UpdateAnnouncementContentsRequestValidator, AnnouncementFactory>()
        )
        .ApiServer.Register<AnnouncementIdentifierParams, AnnouncementResponse>(
            config.ApiBaseRoute,
            (idParams, _) => idParams.ToIdentifier(),
            announcementPolicy,
            apiServerConfigurator => apiServerConfigurator
                .WithNoAdditionalConfiguration()
                .RegisterCrud<CreateAnnouncementRequest, UpdateAnnouncementContentsRequest>()
        )
        .SignalRServer.Register<THub, AnnouncementSignalRRequest, AnnouncementResponse>(signalRConfigurator => signalRConfigurator
            .SetGroupResolver<AnnouncementSignalRRouteResolver>()
            .SetAccessValidationService<AnnouncementSignalRAccessValidator>()
        )
    )
    .Build();
```

Политики доступа и механизмы их применения разбираются в [отдельном блоке](#политики_доступа).

#### ApiServer

Опциональный модуль ApiServer (доступен только для локальных сущностей) позволяет добавить CRUD эндпоинты, которые
используют зарегистрированные DDD сервисы для выполнения операций. Политика доступа применяет правила ролей к эндпоинтам
автоматчиески.

DSL имеет обязательный шаг, конфигурирующий ``RouteGroupBuilder``. Можно либо вызвать ``WithGroupConfiguration`` с
пусотой лябдой, либо вызвать метод ``WithNoAdditionalConfiguration``. [//todo : может, сделать шаг опциональным? Тогда
нужно будет задокументировать, что последовательные вызовы добавляют функционал.]

Методы регистрации отдельных эндпоинтов:

- ``WithFindMany`` - получения множества сущностей по id; streaming POST эндпоинт; не возвращает ненайденные сущности;
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

Вызовы этих методов скрыты за методами:

- ``RegisterRead`` - вызывает ``WithFindMany``, ``WithGetAll``, ``WithGet``
- ``RegisterDelete`` - вызывает ``WithDelete``, ``WithDeleteRange``
- ``RegisterUpdate<TPayload>`` - вызывает ``AddUpdate``, ``AddUpdateMany``
- ``RegisterCreate<TPayload>`` - вызывает ``WithThrowingOnDuplicatesCreate``, ``WithThrowingOnDuplicatesCreateMany``
- ``RegisterCrud<TPayload>`` - вызывает ``RegisterRead``, ``RegisterDelete``, ``RegisterUpdate<TPayload>``,
  ``RegisterCreate<TPayload>``
- ``RegisterCrud<TCreatePayload, TUpdatePayload>`` - вызывает ``RegisterRead``, ``RegisterDelete``,
  ``RegisterUpdate<TUpdatePayload>``, ``RegisterCreate<TCreatePayload>``

Фреймворк не предостваляет реализации энпоинтов для произвольного чтения спецификаций. Они реализуются пользователем
отдельно. [//todo или через механизм .AddCustomRequestHandler<THandler> .AddCustomRequestHandler<THandler, TResponse>]

Метод ``ApiServer.Register`` требует 1 дженерик - тип, в который будет мапится TEntity при ответе клиенту.

#### SignalRServer

Опциональный модуль SignalRServer (только для локальных сущностей) регистрирует сервисы для real-time синхронизации
состояния.

Принцип работы:

1. Пользователь подключается к Hub - фреймворк не предоставляет методов подписки и отписки, только обработку
   существующей подписки.
2. Пользователь отправляет сообщение с данными для подписки (в примере это ``AnnouncementSignalRRequest``). Оно должно
   попасть в ``IHubService``;
3. Сообщение валидируется сервисом из ``SetAccessValidationService``;
4. Сообщение попадает в SetGroupResolver, где по определенному набору правил превращается в группы для SignalR;

SetGroupResolver должен регистрировать как правила для запроса на подписку (``RequestRule``), так и правила для
идентификатора сущности ``EntityRule``. RequestRule превращает запрос в правило, а при событии с сущностью (в рантайме),
ее идентификатор должен вызвать EntityRule, чтобы событие ушло пользователю.

#### Валидация данных

В качестве сервиса валидации используется FluentFalidation IValidator<IEnumerable<TRequest>>.
IEntity сервисы создания и обновления валидируют переданные запросы и бросают исключения, если запросы не прошли
валидацию.

Встроенный ExceptionHandler обрабатывает ошибку валидации и возвращает HttpValidationProblemDetails.

#### Политики доступа

Построчная валидация доступа основана на интерфейсах

- IEntityCreateAccessExpressionProvider
- IEntityReadAccessExpressionProvider
- IEntityUpdateAccessExpressionProvider
- IEntityDeleteAccessExpressionProvider

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

#### Транзакции

Транзакция начинается через метод ``IUnitOfWorkFactory.Create<TTag>``, где TTag тот же тэг, что использовался в
``.RegisterModule<TTag>()``. Это позволяет базе данных, зарегистрированной в рамках модуля, использовать транзакцию. При
этом, изменяющие операции над In-Memory или Remote сущностями бросают ошибку, если выполняются в
контексте транзакции (даже если зарегистрированы в том же модуле). Это достигается при помощи декоратора над IEntity
сервисами.

На данный момент, фреймворк не поддерживает транзакции между разными модулями, даже если они фактически настроены на
общий сервер БД.

Как писалось ранее, для In-Memory репозитория есть реализация транзакций (
``Ukinee.Infrastructure.Ddd.Tests.Common.Services.Repositories.TransactedInMemoryRepository``), но сделать его
симметричным транзакциям EFCore невозможно. Поэтому, репозиторий пока используется только для тестирования.

[Тут должен быть пример транзакции с несколькими БД сущностями и InMemory]

#### Honest & Soft Deletion

Сущности, помеченные ``IEntityWithSoftDelete`` и зарегистрированные через DSL меняют свое поведение при удалении.
Свойтсво ``DeletedAt`` становится заполненным и сущность перестает попадать в выборки чтения.

Никаких дополнительных оптимизаций фреймворк не предоставляет - сущность остается в той же таблице, просто пропадает из
выборок.

### Удаленные сущности // ApiClientContexts

#### Кеширование

### DSL

Необходимость - апелляция к детальному знанию всей архитектуры, громоздкий DI, который легко заменяется DSL

## Низкоуровневое описание фич // Как и почему так

### Идентификаторы

Идентификаторы и их кодогены. Истоки в EF и Expression выборки

### Декораторы

Подход к расширению через dsl, симметрии через декораторы с примером на транзакциях (неявное) и с примером на кеше (
явное)

### Транзакции

IUOWFactory IUOWProvider

### Описание полного пайплайна CRUD и AddRequestHandler

### DSL и как оно устроено

#### Все случаи магической магии о которых нужно и не нужно знать

#### RegistrationPolicy

#### Типы сущностей

#### Иерархия билдеров

## Компромиссы

- падение на невалидных транзакциях (Для InMemory или remote сущностей)
- отсутсвие поддержки между контекстных транзакций // пока что разные БД не поддерживаются. Есть вариант добавить
  транзакции, если они на одном сервере, но выглядит как дополнительная магия
- всегда полный респонс в SignalR. Может, можно отправлять примененный Payload, id и версию, если они есть, а если нет -
  полный респонс?

## TODO

Версионирование
Оптимистичные блокировки
Версионное кеширование
SAGA
Outbox
Observability
Api-Key auth для Service-Service взаимодействия