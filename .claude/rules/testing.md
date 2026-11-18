# Test configuration

> **Always in effect.** This rule is loaded for every task in this repository and applies to
> every file under `tests/`, to every test `.csproj` and `xunit.runner.json`, and to the
> `EndToEndTest` environment of the Api — no exceptions without asking first.

Three test projects, three isolation levels. Each level has **one** way of getting its
dependencies; mixing them (a mock in an integration test, InMemory in an end-to-end test) is
what turns a suite into a liar.

| Level | Project | Dependencies | Database |
|---|---|---|---|
| Unit | `Codeflix.Catalog.UnitTests` | Moq for every port (repositories, `IUnitOfWork`) | none |
| Integration | `Codeflix.Catalog.IntegrationTests` | real repositories, real `UnitOfWork` | EF Core InMemory |
| End-to-end | `Codeflix.Catalog.EndToEndTests` | the whole Api via `WebApplicationFactory<Program>` | MySQL 8 in Docker (`database-e2e`, port 33060) |

## Shared stack

Every test project uses the same packages at the same versions. When one is bumped, bump it
in all three in the same commit.

| Package | Purpose | Projects |
|---|---|---|
| `xunit` + `xunit.runner.visualstudio` | framework and runner | all |
| `Microsoft.NET.Test.Sdk`, `coverlet.collector` | `dotnet test` and coverage | all |
| `FluentAssertions` | every assertion — no `Assert.*` | all |
| `Bogus` | test data, always `new Faker("pt_BR")` | all |
| `Moq` | mocks | **Unit only** |
| `Microsoft.EntityFrameworkCore.InMemory` | database | **Integration only** |
| `Microsoft.AspNetCore.Mvc.Testing`, `Pomelo.EntityFrameworkCore.MySql` | host and seeding | **End-to-end only** |

- `<Using Include="Xunit" />` is a global using in each `.csproj`; do not repeat `using Xunit;`.
- `ImplicitUsings` and `Nullable` are enabled, `IsTestProject` true, `IsPackable` false.
- References follow the layer under test: Unit → `Domain` + `Application`; Integration →
  `Domain` + `Infra.Data.EF` (plus `Application` transitively); End-to-end → `Api` +
  `Infra.Data.EF`. A test project never references another test project.
- Adding a package to a test project is a `.csproj` change that no test can drive: say so
  and ask first (TDD rule).

## Fixtures and xUnit wiring

- State shared by a suite lives in a **collection fixture**: the fixture file declares both
  `[CollectionDefinition(nameof(XTestFixture))]` (a `…Collection : ICollectionFixture<XTestFixture>`)
  and the fixture class; the test class has `[Collection(nameof(XTestFixture))]` and takes the
  fixture through its primary constructor. No `IClassFixture`, no static state.
- Fixture inheritance: `Common/BaseFixture` (Faker + `GetRandomBoolean`) →
  `<Aggregate>/Common/<Aggregate>…BaseFixture` (aggregate builders, mocks, persistence) →
  `<UseCase>TestFixture` (`GetValidInput()` and friends). Aggregate helpers never go up into
  `Common/BaseFixture`.
- Data builders produce **valid** aggregates through their public constructor and methods,
  bounded by the domain's min/max constants. Invalid data is built explicitly per test.
- `[Fact(DisplayName = nameof(Method))]` / `[Theory(...)]` plus one `[Trait]` per test.
  The trait name is the level, the value names the suite with no spaces inside the name:

  | Suite | Trait name | Value |
  |---|---|---|
  | Domain entity | `Domain` | `<Aggregate> - Aggregates` (`Genre - Aggregates`) |
  | Unit, use case | `Application` | `<Verb><Aggregate> - Use Cases` (`CreateGenre - Use Cases`) |
  | Integration, use case | `Integration/Application` | `<Verb><Aggregate> - Use Cases` |
  | Integration, repository | `Integration/Infra.Data` | `<Aggregate>Repository - Repositories` |
  | End-to-end | `EndToEnd/Api` | `<Verb><Aggregate> - Endpoints` |
- `[MemberData]` with more than a couple of rows comes from a `<UseCase>TestDataGenerator`
  static class that instantiates its own fixture. **Theory rows must be distinct** — xUnit
  silently collapses duplicate rows, so random generators must guarantee uniqueness or the
  case count drifts between runs.

## Unit — xUnit + Moq

- Every dependency of the class under test is a `Mock<T>` created **fresh per test** by a
  fixture method named `Get<Port>Mock()` (`fixture.GetGenreRepositoryMock()`,
  `fixture.GetUnitOfWorkMock()`) returning a new `Mock<T>`. Never keep a
  mock in a fixture field: collection fixtures are shared, and so would be its setups and
  recorded calls.
- Default `MockBehavior.Loose`. `Setup(...).ReturnsAsync(...)` / `.ThrowsAsync(...)` only what
  the scenario needs; match ids with `It.Is<Guid>(x => x == id)` and the token with
  `It.IsAny<CancellationToken>()`.
- Assert interactions with `Verify(..., Times.Once)` on the write path **and**
  `Times.Never` on the error path: an invalid input, a missing aggregate or a missing related
  id must prove that `Insert`/`Update`/`Delete` and `IUnitOfWork.Commit` were **not** called.
- Only interfaces from `Domain/Repository` and `Application/Interfaces` are mocked. Never mock
  domain entities, DTOs, `DbContext` or anything from `Infra.Data.EF` — if a unit test needs
  EF, it is an integration test.
- Domain tests (`Domain/Entity/<Aggregate>/`) use no mocks at all.
- `TestModuleInitializer` pins the culture to `en-US` with `[ModuleInitializer]`, so number
  and date formatting in messages is stable. Keep it; a new test project that asserts
  formatted text gets its own.

## Integration — EF Core InMemory

- The context is built only by `BaseFixture.CreateDbContext(bool preserveData = false)` with
  `UseInMemoryDatabase("integration-tests-db")`. `CreateDbContext()` wipes the store
  (`EnsureDeleted`); `CreateDbContext(true)` reopens it with a fresh change tracker.
- Pattern per test: **arrange** on one context (`AddRange` + `SaveChangesAsync`), **act** with
  real `<Aggregate>Repository` + `UnitOfWork` on that context, **assert** on a second
  `CreateDbContext(true)`. Asserting on the acting context reads the change tracker, not the
  store, and passes even when `Commit` was never called.
- No mocks in this project — the point is the real repository against a real provider.
- Know what InMemory does **not** check, and cover it end-to-end instead: `HasMaxLength`,
  required columns, foreign keys and `ON DELETE CASCADE` (it cascades only to tracked rows),
  transactions/rollback, collation-dependent ordering and `datetime(6)` precision.
- `xunit.runner.json` (copied to output with `CopyToOutputDirectory=Always`):

  ```json
  { "parallelizeTestCollections": false, "maxParallelThreads": 1 }
  ```

  The store name is fixed, so collections share it; running them in parallel makes one
  suite's `EnsureDeleted` wipe another's data. Do not remove it or randomise the store name
  without asking.

## End-to-end — MySQL in a container

- Prerequisite: `docker compose up -d`. The suite talks to `database-e2e`
  (`catalogdb-e2e`, host port **33060**), never to `database` on 3306.
- The host is `CustomWebApplicationFactory<Program>`, which sets the environment to
  `EndToEndTest` (→ `appsettings.EndToEndTest.json`, `CatalogDb` on `localhost:33060`) and runs
  `EnsureDeleted` + `EnsureCreated` on boot. Because it drops the database, that connection
  string must only ever point at the e2e container.
- `EndToEndTests/Common/BaseFixture` owns the factory, `HttpClient`, `ApiClient` and
  `CreateDbContext()` (MySQL, same connection string the Api resolved), and `CleanPersistence()`.
  Each collection fixture boots its own factory, so each collection starts on an empty schema.
- Seed and read back through `<Aggregate>Persistence` (direct `DbContext`, `AsNoTracking` on
  reads), never through the Api. Call the Api only for the action under test.
- Suites that count or order rows implement `IDisposable` and call `fixture.CleanPersistence()`
  in `Dispose`, so every test starts empty.
- HTTP goes through `ApiClient` (`Get`/`Post`/`Put`/`Delete` → `(HttpResponseMessage, TOutput?)`,
  snake_case both ways). Deserialize into the Api's envelopes (`ApiOutput<T>`,
  `ApiListOutput<T>`) or `ProblemDetails`; assert status code and body.
- MySQL is not .NET: `datetime(6)` keeps microseconds (not 100 ns ticks) and Guids are
  `char(36)` compared as strings. When comparing order or timestamps, build the expectation
  from rows read back via `Persistence.GetAll()` and use an ordinal `Id.ToString()` tie-breaker.
- Same `xunit.runner.json` as integration — one database, serialized collections.
- Schema comes from `EnsureCreated`, not from migrations, so a green e2e run does **not**
  prove a migration exists (see the migrations check in `CLAUDE.md`).

## Running

```bash
dotnet test tests/Codeflix.Catalog.UnitTests            # no external dependency
dotnet test tests/Codeflix.Catalog.IntegrationTests     # no external dependency
docker compose up -d database-e2e && dotnet test tests/Codeflix.Catalog.EndToEndTests
dotnet test --filter "Application=CreateCategory - Use Cases"   # by trait
dotnet test --filter "FullyQualifiedName~CreateGenre"        # by suite
```

## Do not copy these (current deviations)

- `IntegrationTests/Common/BaseFixture` and `EndToEndTests/Common/BaseFixture` carry
  `GetValidCategory*` helpers and category constants; they belong in the Category base fixtures.
- `EndToEndTests.csproj` references `Microsoft.EntityFrameworkCore.InMemory`, which it does not
  use, and gets `Newtonsoft.Json` (used by `ApiClient.PrepareGetRoute`) only transitively from
  the Api instead of referencing it directly.
- `UnitTests.csproj` has an empty `<Folder Include="Application\Category\" />` item.
- Trait outliers — use the table above instead: `CreateGenre`/`UpdateGenre` unit tests use a
  `Genre` trait name; unit Genre and CastMember suites and integration Genre suites use spaced
  values (`Create Cast Member - Use Cases`, `List Genres - Use Case`, singular included);
  `GenreTest` uses `Genre - Aggregate` and part of the CastMember domain suite uses
  `CastMembers - Aggregates`; the Category list e2e suite uses `Category/List - Endpoints`.
- Mock factory outliers — new fixtures use `Get<Port>Mock()`: `GenreUseCaseBaseFixture` has
  `GetGenreRepository()`, `GetCategoryRepository()` and `GetUnitOfWork()`;
  `CategoryUseCaseBaseFixture` has `GetRepositoryMock()`.
