# Relationships between aggregates

> **Always in effect.** This rule is loaded for every task in this repository and applies to
> every relation between aggregates, in every layer — domain, use case, repository, mapping,
> API contract and tests — no exceptions without asking first.

The model follows the DDD aggregate design rules (Evans; Vernon, *Effective Aggregate
Design*). An aggregate is a consistency boundary: everything inside it is consistent at the
end of every method, and nothing outside it can break that. Relations between aggregates are
**not** part of either boundary — they are references, checked by the application layer.

`Genre → Category` (many-to-many) is the reference implementation.

## 1. Reference other aggregates by identity only

```csharp
private readonly List<Guid> _categories;
public IReadOnlyCollection<Guid> Categories => _categories.AsReadOnly();
```

- An aggregate holds **ids** of other aggregates, never instances: no `Category` property on
  `Genre`, no `List<Category>`, no navigation property, no `CategoryId` FK property mirroring
  a table.
- The collection is a private backing `List<Guid>` initialised in the constructor and exposed
  as `IReadOnlyCollection<Guid>`. Nothing outside the aggregate can add, remove or reorder.
- Mutations are intent-named methods on the owning aggregate, each ending with `Validate()`:
  `Add<Relation>(Guid)`, `Remove<Relation>(Guid)`, `RemoveAll<Relation>s()`.
- The relation is owned by **one** side — the one whose use cases change it (`Genre` owns its
  categories; `Category` knows nothing about genres). Never model both directions.

Why: holding an instance would let one aggregate load, mutate or depend on the invariants of
another, merge two consistency boundaries into one and drag both into every query.

## 2. One aggregate modified per transaction

- A use case loads, changes and persists **one** aggregate, then calls
  `IUnitOfWork.Commit` once. `UpdateGenre` changes the genre's category ids; it never touches
  a `Category`.
- If a business rule seems to need two aggregates changed atomically, question the
  boundaries first and ask. The default answer is eventual consistency (a domain event
  handled in a separate transaction), not a bigger transaction.

## 3. Existence of related aggregates is checked in the use case

The domain cannot see the database, so "this category exists" is not a domain invariant —
it is validated by the use case before mutating the aggregate:

```csharp
var found = await categoryRepository.ListIdsByIds(categoryIds, cancellationToken);
var missing = categoryIds.Distinct().Except(found).ToList();
if (missing.Count > 0)
    throw new RelatedEntityException(
        $"Related category ids not found {string.Join(", ", missing)}");
categoryIds.Distinct().ToList().ForEach(genre.AddCategory);
```

- The **referenced** aggregate's repository exposes the lookup (`ICategoryRepository.ListIdsByIds`),
  returning ids only, `AsNoTracking`, deduping its input.
- Missing ids raise `Domain.Exceptions.RelatedEntityException`, which the API maps to
  `422 Unprocessable Entity`. It is not `NotFoundException` (that is for the aggregate
  addressed by the route).
- Validate **before** calling `Add<Relation>`, and compare against the deduplicated input —
  repeated ids are not "missing" ids.
- Replacing a relation set is `RemoveAll<Relation>s()` then `Add<Relation>` per id; a `null`
  list in the input means "leave unchanged", an empty list means "clear".

## 4. Reading related data is a query concern

- An aggregate's output DTO carries the related **ids**. When the response also needs names
  (`GetGenre` returns categories with `Name`), the use case fetches them with one batched call
  to the referenced repository (`ListByIds`) and fills them through a dictionary lookup —
  never one query per id, never `FirstOrDefault` inside a loop.
- Dedupe ids once, before querying, so duplicates reach neither the database nor the JSON.
- Listing use cases (`ListGenres`) return ids only unless the screen genuinely needs more; do
  not widen the list endpoint "just in case".

## 5. Persistence of a relation

The relation lives in a persistence-only join model in `Infra.Data.EF/Models/`
(`GenresCategories`), never in the aggregate's table. The owning aggregate's repository is
the only code that reads or writes it:

| Operation | What the owning repository does with the join rows |
|---|---|
| `Insert` | `AddRangeAsync` one row per related id |
| `Get` | load rows `AsNoTracking`, call `Add<Relation>` for each |
| `Update` | remove all rows of the aggregate, re-add the current ids |
| `Delete` | remove the aggregate's rows explicitly, then the aggregate |
| `Search` | load rows for the page's ids in **one** query, group, rehydrate |

Deleting a **referenced** aggregate (a `Category`) removes its join rows through the
database `ON DELETE CASCADE`; the referencing aggregate is not loaded or changed. If the
business requires blocking such a delete instead, that is a use-case rule driven by a test —
ask before adding it.

## 6. Tests for a relation

Driven one behaviour at a time per the TDD rule:

- **Domain** — `Add`, `Add` many, `Remove`, `RemoveAll` on the owning aggregate.
- **Use case (unit)** — related ids valid; some missing → `RelatedEntityException` listing
  them and repository/`Commit` never called; duplicate ids not reported as missing; `null` vs
  empty list on update.
- **Repository (integration)** — join rows written on insert, rehydrated on get, replaced on
  update, removed on delete; assert on a second `DbContext`.
- **End-to-end** — `422` on a missing related id; related ids (and names, where exposed)
  round-trip through the API.

## Do not copy these (current deviations)

- `Genre.AddCategory` does not dedupe and `CreateGenre`/`UpdateGenre.ValidateRelatedCategories`
  compare `found.Count < categoryIds.Count` against the raw list, so a repeated valid id
  raises a false `RelatedEntityException`. New code compares against the distinct ids.
- `GenreRepository.Search` resolves each group with `genres.Find(...)` inside a loop; build a
  dictionary by id instead.
