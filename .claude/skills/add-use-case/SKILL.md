---
name: add-use-case
description: Build a use case in Codeflix.Catalog.Application — the MediatR file set (interface, handler, input, output), the handler shapes for Create/Get/List/Update/Delete, output mapping, NotFound/related-entity errors, and the unit-test-first order with Moq'd repositories and IUnitOfWork. Use when adding or changing a use case, its input/output DTOs, or the unit tests that drive it. Application layer only — no EF, no controller.
---

# Adding a use case

Scope: `src/Codeflix.Catalog.Application` and its unit tests in
`tests/Codeflix.Catalog.UnitTests/Application/`. Persistence, DI registration and HTTP
are out of scope here.

`Genre` (with relations) and `CastMember` (without) are the canonical aggregates.
`Category` carries the outliers listed in §6.

## 1. File set

```
src/Codeflix.Catalog.Application/UseCases/<Aggregate>/<Verb>/
  I<Verb><Aggregate>.cs        # interface
  <Verb><Aggregate>.cs         # handler
  <Verb><Aggregate>Input.cs
  <Verb><Aggregate>Output.cs   # not for Delete
```

`<Verb>` folder is the short form (`Create`, `Get`, `List`, `Update`, `Delete`); type
names are the long form. `List` uses the plural in type names
(`ListCastMembers`, `ListCastMembersInput`, `ListCastMembersOutput`,
`ListCastMembersItemOutput`), folder stays `List`.

Namespace = folder path: `Codeflix.Catalog.Application.UseCases.<Aggregate>.<Verb>`.
Since the namespace segment collides with the entity name, alias the entity:
`using DomainEntity = Codeflix.Catalog.Domain.Entity;`.

## 2. Shapes

```csharp
// Interface — one line, MediatR
public interface ICreateCastMember : IRequestHandler<CreateCastMemberInput, CreateCastMemberOutput>;
public interface IDeleteCastMember : IRequestHandler<DeleteCastMemberInput>;      // void

// Input — positional record implementing IRequest<TOutput>
public record CreateCastMemberInput(string Name, CastMemberType Type) : IRequest<CreateCastMemberOutput>;
public record DeleteCastMemberInput(Guid Id) : IRequest;

// Output — positional record with a static factory named From<Aggregate>
public record CreateCastMemberOutput(Guid Id, string Name, CastMemberType Type, DateTime CreatedAt)
{
    public static CreateCastMemberOutput FromCastMember(DomainEntity.CastMember castMember) =>
        new(castMember.Id, castMember.Name, castMember.Type, castMember.CreatedAt);
}

// Handler — primary constructor, dependencies are domain interfaces + IUnitOfWork
public class CreateCastMember(ICastMemberRepository castMemberRepository, IUnitOfWork unitOfWork)
    : ICreateCastMember
{
    public async Task<CreateCastMemberOutput> Handle(CreateCastMemberInput request, CancellationToken cancellationToken)
    { ... }
}
```

## 3. Handler body per verb

| Verb | Body | Commit? |
|---|---|---|
| Create | `new DomainEntity.X(...)` → `repo.Insert` → `unitOfWork.Commit` → `Output.FromX` | yes |
| Get | `repo.Get(id)` → `Output.FromX` | no |
| List | `repo.Search(request.ToSearchInput())` → `Output.FromSearchOutput` | no |
| Update | `repo.Get(id)` → mutate via aggregate methods → `repo.Update` → `Commit` → output | yes |
| Delete | `repo.Get(id)` → `repo.Delete` → `Commit` | yes |

- **Not found is the repository's job.** `repo.Get` throws `NotFoundException`; the
  handler never null-checks.
- **Update with optional fields**: nullable input properties (`bool? IsActive`,
  `List<Guid>? Categories`). `null` means "leave as is". Toggle state only through
  `Activate()`/`Deactivate()`, never by assigning.
- **Related ids** (e.g. categories of a genre): validate with
  `repo.ListIdsByIds(ids)` and throw `RelatedEntityException` listing the missing ids
  *before* adding them to the aggregate. Dedupe ids before querying **and compare the
  result against the distinct count** — `CreateGenre`/`UpdateGenre` compare against the
  raw list, so a repeated id raises a false `RelatedEntityException`.
- **List**: input inherits `PaginatedListInput` and implements `IRequest<TOutput>`, with a
  parameterless constructor defaulting to `(1, 15, "", "", SearchOrder.Asc)`; output
  inherits `PaginatedListOutput<TItem>` and exposes `static FromSearchOutput(SearchOutput<T>)`.
  Names of related entities are filled in a second query over the **distinct** ids, matched
  by dictionary, not by `FirstOrDefault` inside a loop.

## 4. TDD order

One behaviour per cycle, unit test first, in
`tests/Codeflix.Catalog.UnitTests/Application/<Aggregate>/<Verb><Aggregate>/`
(no `UseCases/` level in the unit project):

1. Happy path — RED fails because the type does not exist yet; add only the
   interface/input/output/handler skeleton needed to compile, then see it fail on
   the assertion, then make it pass.
2. Each error path (`NotFoundException` from the mocked repo, `EntityValidationException`
   from the entity, `RelatedEntityException`).
3. Each optional-field branch for Update; each pagination/empty-result case for List.

Unit-test conventions:

- Fixture chain: `Common/BaseFixture` → `<Aggregate>/Common/<Aggregate>UseCaseBaseFixture`
  (repository mock factories, `GetUnitOfWorkMock()`, `GetValid<Aggregate>()`) →
  `<Verb><Aggregate>TestFixture` (collection definition + fixture in the same file).
- Alias the use-case namespace: `using UseCases = Codeflix.Catalog.Application.UseCases.<Aggregate>.<Verb>;`
- `[Fact(DisplayName = nameof(Method))]` + `[Trait("Application", "<Verb> <Aggregate> - Use Cases")]`.
- Verify interactions, not just the output: `Insert/Update/Delete ... Times.Once` and
  `unitOfWork.Commit ... Times.Once`; on an error path, `Commit` is `Times.Never`.

```bash
dotnet test tests/Codeflix.Catalog.UnitTests --filter "FullyQualifiedName~<Verb><Aggregate>Test"
```

## 5. After the unit suite is green

The use case is not usable until its repository method exists in Infra and it is
reachable over HTTP. Those are separate tasks with their own integration and
end-to-end tests; list them as next steps instead of writing them here.

## 6. Do not copy these

- **`Category` FluentValidation validators** (`*InputValidator.cs`) are registered nowhere
  and run by nothing. Validation lives in the entity's `Validate()`.
- **`Category` outputs are mutable classes** (`CreateCategoryOutput`, `GetCategoryOutput`, …);
  new outputs are positional records.
- **`ListGenresOutput.FillCategoriesWithName`** does a linear search per item; use a
  dictionary keyed by id.
- **`CastMemberUseCaseBaseFixture.GetRandomCastMemberType()`** uses `Next(1, 2)`, which
  always returns 1. Upper bound is exclusive — use `Next(1, 3)` or pick from
  `Enum.GetValues<CastMemberType>()`.

## 7. Before finishing

- [ ] Every file in the right `<Verb>` folder, namespace matches the path.
- [ ] Handler depends only on Domain interfaces and `IUnitOfWork` — no `Infra.Data.EF`.
- [ ] Write use cases commit exactly once; read use cases never commit.
- [ ] Every behaviour was driven by a unit test seen failing first.
- [ ] `dotnet test` green.
