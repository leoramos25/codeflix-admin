---
name: add-repository
description: Persist an aggregate in Codeflix.Catalog.Infra.Data.EF — the IEntityTypeConfiguration, the DbSet, the repository implementing the domain interface (Insert/Get/Update/Delete/Search, ordering, relation tables), the EF migration, and the integration tests over EF InMemory that drive it. Use when adding or changing a repository, an entity configuration, a join model or a migration, or when a repository test fails because of change-tracker state. Infra layer only — no controller.
---

# Adding a repository

Scope: `src/Codeflix.Catalog.Infra.Data.EF` and
`tests/Codeflix.Catalog.IntegrationTests/Infra.Data.EF/`. The domain interface
`I<Aggregate>Repository` already exists in `Codeflix.Catalog.Domain/Repository/`; this
task implements it, it does not redesign it.

`GenreRepository` is the reference for an aggregate with relations,
`CastMemberRepository` for one without. See §6 before copying either.

## 1. Files touched

```
src/Codeflix.Catalog.Infra.Data.EF/
  CodeflixCatalogDbContext.cs                  # + DbSet<X> Xs => Set<X>();
  Configurations/<Aggregate>Configuration.cs   # IEntityTypeConfiguration<X>
  Models/<A>s<B>s.cs                           # only for a join table (GenresCategories)
  Repositories/<Aggregate>Repository.cs
  Migrations/                                  # generated, see §4
```

`OnModelCreating` already calls `ApplyConfigurationsFromAssembly`; a new configuration
class is picked up without touching the context beyond the `DbSet`.

## 2. Configuration

```csharp
public class CastMemberConfiguration : IEntityTypeConfiguration<CastMember>
{
    public void Configure(EntityTypeBuilder<CastMember> builder)
    {
        builder.HasKey(castMember => castMember.Id);
        builder.Property(castMember => castMember.Name).HasMaxLength(255);
    }
}
```

The entity has no navigation properties. Relations are a join model in `Models/`
with a composite key (`GenresCategoriesConfiguration`), and the repository syncs it
by hand.

## 3. Repository

```csharp
public class CastMemberRepository(CodeflixCatalogDbContext context) : ICastMemberRepository
{
    private readonly DbSet<CastMember> _castMembers = context.CastMembers;
    ...
}
```

| Method | Rule |
|---|---|
| `Insert` | `AddAsync`; for relations, `AddRangeAsync` the join rows too. Never `SaveChanges` — `UnitOfWork.Commit` does that. |
| `Get` | `FindAsync([id], cancellationToken)` then `NotFoundException.ThrowIfNull(x, $"<Aggregate> '{id}' not found.")`. Rehydrate relations by calling the aggregate's public `Add<Relation>` for each join row. |
| `Update` | `Update(aggregate)`; for relations, remove all join rows of the aggregate then re-add the current ones. |
| `Delete` | `Remove(aggregate)`; remove its join rows first. Return `Task.CompletedTask`, no fake `async`. |
| `Search` | `AsNoTracking()` → filter `Search` with `Name.Contains` when not blank → order → `CountAsync` → `Skip((Page-1)*PerPage).Take(PerPage)`. |

Ordering is a `switch` on `(orderBy.ToLower(), order)` with a default of `Name` ascending,
always followed by `.ThenBy(x => x.Id)` so pages are stable.

Lookups by id lists (`ListByIds`, `ListIdsByIds`) dedupe the ids first
(`[.. ids.Distinct()]`) and use `AsNoTracking()`.

## 4. Migration

A migration is generated code and cannot be driven by a test. Per the project's TDD
rule, **say so and ask before generating it**. When approved:

```bash
dotnet ef migrations add Create<Aggregate>Table \
  --project src/Codeflix.Catalog.Infra.Data.EF \
  --startup-project src/Codeflix.Catalog.Api
```

The design-time connection is `appsettings.json` → MySQL on port 3306 (`database`
service in `docker-compose.yml`). Do not hand-edit files under `Migrations/` — direct
edits there are denied by the project settings. The end-to-end suite does not use
migrations (it calls `EnsureCreated`), so a green e2e run does not prove the migration
is correct; check the generated `Up()` by reading it.

## 5. Integration tests (TDD)

Folder: `tests/Codeflix.Catalog.IntegrationTests/Infra.Data.EF/Repositories/<Aggregate>Repository/`
with `<Aggregate>RepositoryTest.cs` + `<Aggregate>RepositoryTestFixture.cs` (collection
definition and fixture in the same file).

- Trait: `[Trait("Integration/Infra.Data", "<Aggregate>Repository - Repositories")]`.
- One method per cycle: Insert → Get → Get not found → Update → Delete → Search
  (paginated, by search text, each ordering, empty result).
- **Arrange on one context, assert on another.** `CreateDbContext()` wipes the InMemory
  store; `CreateDbContext(true)` reopens it with a fresh change tracker. Asserting on the
  same context that did the work passes on tracked-but-unsaved entities.
- Commit explicitly in the test (`await dbContext.SaveChangesAsync()` or a real
  `UnitOfWork`) — the repository never saves.
- Put aggregate-specific helpers (`GetValid<Aggregate>()`, lists, names) in the
  repository fixture, not in `Common/BaseFixture`.

```bash
dotnet test tests/Codeflix.Catalog.IntegrationTests --filter "FullyQualifiedName~<Aggregate>RepositoryTest"
```

## 6. Do not copy these

- **`CategoryRepository.Get` and `CastMemberRepository.Get` call
  `FindAsync([id, cancellationToken], ...)`.** That passes the token as a second key
  value. Use `FindAsync([id], cancellationToken)` as `GenreRepository` does.
- **`CastMemberRepository.Search` ignores `input.Search`.** Every searchable repository
  filters by the search text.
- **`GenreRepository.Search` only orders when `OrderBy` is not empty**, so an empty sort
  returns an unordered page. Always order (default `Name`) and finish with `ThenBy(Id)`.
- **`async` methods that only wrap a sync call in `Task.FromResult`** (`Delete`, `Update`
  in Category/CastMember) — return `Task.CompletedTask` instead.

## 7. Before finishing

- [ ] `DbSet` added, configuration class in `Configurations/`, repository in `Repositories/`.
- [ ] Repository never calls `SaveChanges`.
- [ ] Every method driven by an integration test seen failing first, asserted through a
      second context.
- [ ] Migration generated only after asking, and its `Up()` read.
- [ ] Registration in `Api/Configurations/UseCasesConfiguration.cs` left for the
      endpoint task, unless asked.
- [ ] `dotnet test` green.
