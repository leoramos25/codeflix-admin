---
name: add-test-suite
description: Write the test suite for a use case, repository or endpoint in this solution — the xUnit fixture/collection/trait protocol, which of the three test projects to touch, and the DbContext gotchas. Use when adding or extending tests for a new or existing use case, wiring an aggregate into the API, or when a test passes/fails for reasons that smell like the EF change tracker or shared-database state.
---

# Adding a test suite

This solution has three test projects with different rules. Pick the levels first,
then follow the same file protocol in each.

## 1. Pick the levels

| Level | Project | Add it when |
|---|---|---|
| Unit | `Codeflix.Catalog.UnitTests` | Always. Use case logic with Moq'd repos + `IUnitOfWork`, or domain entity behaviour. |
| Integration | `Codeflix.Catalog.IntegrationTests` | The use case touches EF — real repository, real `UnitOfWork`, EF Core InMemory. Also for repository classes themselves. |
| End-to-end | `Codeflix.Catalog.EndToEndTests` | A controller action exists for it. Real HTTP over real MySQL on port 33060. |

A use case that is not routed in a controller yet gets unit + integration only.

## 2. File protocol (identical in all three projects)

One folder per use case, two files:

```
<UseCase>/
  <UseCase>Test.cs          # or <UseCase>ApiTest.cs in EndToEndTests
  <UseCase>TestFixture.cs   # or <UseCase>ApiTestFixture.cs
```

The fixture file declares **both** the collection definition and the fixture —
never split them into separate files:

```csharp
[CollectionDefinition(nameof(CreateGenreTestFixture))]
public class CreateGenreTestFixtureCollection : ICollectionFixture<CreateGenreTestFixture>;

public class CreateGenreTestFixture : GenreUseCaseBaseFixture { /* GetValidInput() etc. */ }
```

The test class carries the matching `[Collection(nameof(...))]` and takes the
fixture as a primary-constructor parameter.

Every test method:

```csharp
[Fact(DisplayName = nameof(MethodName))]   // or [Theory]
[Trait("<name>", "<value>")]
```

`DisplayName` must be `nameof` the method itself — `dotnet test --filter
"DisplayName=X"` depends on it.

See `references/templates.md` for full skeletons of all three levels.

## 3. Fixture inheritance

Three layers. Keep aggregate-specific helpers out of the global base:

```
Common/BaseFixture                       # Faker("pt_BR") + GetRandomBoolean only
  └─ <Aggregate>/Common/<X>BaseFixture   # GetValidGenre(), GetValidName(), mocks…
       └─ <UseCase>/<UseCase>TestFixture # GetValidInput() for this use case
```

`IntegrationTests/Common/BaseFixture` currently violates this — it carries
`GetValidCategory*` helpers. Do not copy that shape; put new helpers in the
per-aggregate base fixture.

In `EndToEndTests` the aggregate base fixture also owns a `<Aggregate>Persistence`
class that seeds rows directly through a `DbContext` (see `GenrePersistence`).
Seed through it, never through the API.

## 4. Traits

Trait **name** is the filter key: `dotnet test --filter "Application=..."`.

| Project / kind | Trait name | Value |
|---|---|---|
| Unit, domain entity | `Domain` | `<Aggregate> - Aggregates` |
| Unit, use case | `Application` | `<UseCase> - Use Cases` |
| Integration, use case | `Integration/Application` | `<UseCase> - Use Cases` |
| Integration, repository | `Integration/Infra.Data` | `<Repository> - Repositories` |
| End-to-end | `EndToEnd/Api` | `<UseCase> - Endpoints` |

Existing code is inconsistent here — `CreateGenre`/`UpdateGenre` unit tests use a
`Genre` trait name, and some values say `Use Case` singular. Use the table above
for new tests; do not propagate the outliers.

## 5. Gotchas

**Integration — always assert through a second context.** `CreateDbContext()`
wipes the InMemory store; `CreateDbContext(true)` reopens the same store with a
fresh change tracker. Acting on one and asserting on the other is what keeps a
tracked-but-unsaved entity from faking a pass:

```csharp
var dbContext = fixture.CreateDbContext();
// … act …
var assertDbContext = fixture.CreateDbContext(true);
var fromDb = await assertDbContext.Genres.FindAsync(output.Id);
```

**Integration + e2e share one database.** Both projects set
`parallelizeTestCollections: false` and `maxParallelThreads: 1` in
`xunit.runner.json`. Keep it that way — removing it produces flakes, not speed.

**E2E needs the containers up**: `docker compose up -d` before running. The
factory recreates the schema per boot via `EnsureDeleted`/`EnsureCreated`; call
`fixture.CleanPersistence()` when a test needs a clean slate mid-suite.

**E2E asserts on the envelope**, not the raw output — `ApiOutput<TOutput>` for a
single item, `ApiListOutput<TOutput>` for a list, `ProblemDetails` for errors
(check `Status`, `Title`, `Detail`, `Type`).

**`Genre.Categories` is a `List<Guid>`, not a navigation.** Seeding a
genre-category relation in a test means inserting `GenresCategories` rows
yourself.

## 6. Before finishing

```bash
dotnet csharpier format .   # required — CSharpier is the formatter
dotnet test --filter "FullyQualifiedName~<UseCase>"
```
