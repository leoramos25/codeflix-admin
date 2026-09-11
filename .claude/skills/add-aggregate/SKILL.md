---
name: add-aggregate
description: Build a new aggregate in Codeflix.Catalog.Domain as a rich DDD entity — the AggregateRoot/Validate protocol, encapsulation rules (private setters, private backing collections, state mutated only by the aggregate itself), the repository interface shape, and the TDD order to grow it. Use when creating a new aggregate or entity, adding behaviour or a relation to an existing one, or reviewing whether a domain class respects the project's encapsulation rules. Domain layer only.
---

# Adding an aggregate to the Domain

Scope: `src/Codeflix.Catalog.Domain` and its unit tests, nothing else. The Application,
Infra and Api layers are out of scope for this skill — do not write them here.

`Genre` is the canonical aggregate of this repository. `Category` is older and carries
outliers listed in §6; copy `Genre`.

## 1. What a new aggregate adds to the Domain

Exactly two files, plus tests:

```
src/Codeflix.Catalog.Domain/Entity/<Aggregate>.cs
src/Codeflix.Catalog.Domain/Repository/I<Aggregate>Repository.cs
tests/Codeflix.Catalog.UnitTests/Domain/Entity/<Aggregate>/<Aggregate>Test.cs
tests/Codeflix.Catalog.UnitTests/Domain/Entity/<Aggregate>/<Aggregate>TestFixture.cs
```

`<Aggregate>` is singular. Add a file to `Exceptions/` or a rule to
`Validation/DomainValidation.cs` only when an existing one genuinely does not fit, and
only driven by a failing test. Never add a new top-level folder to the Domain project.

`Domain` references nothing. No EF, no MediatR, no FluentValidation, no `Microsoft.*`.
If an aggregate seems to need one of those, the design is wrong — ask before proceeding.

## 2. The rich-entity rules

These are the non-negotiable part. An aggregate owns its state and is the only thing
allowed to change it.

- **Inherit `AggregateRoot`.** `Id` comes from `Entity` and is assigned in its
  constructor. Never declare, accept or assign an `Id` yourself.
- **Every property is `{ get; private set; }`.** No public setter, ever. No
  `init`, no `required`, no object-initializer construction.
- **One public constructor taking the state the aggregate cannot exist without.**
  Optional state gets a default parameter. There is no public parameterless
  constructor and no separate constructor for persistence.
- **The constructor ends with `Validate()`.** So does every mutator. An aggregate is
  never observable in an invalid state.
- **`Validate()` is `private void`.** Callers outside the aggregate must not be able
  to trigger or skip validation. It calls `DomainValidation` helpers, which throw
  `EntityValidationException`.
- **Behaviour methods are named by intent**, not by field: `Activate`, `Deactivate`,
  `Update`, `AddCategory`. No `SetName`, no `ChangeIsActive`.
- **`CreatedAt` is `DateTime.UtcNow`**, assigned in the constructor and never
  reassigned.
- **Size limits are `private const`** on the entity, used by `Validate()` and named
  `Min<Field>Size` / `Max<Field>Size`.

### Collections and relations

A relation to another aggregate is stored **as ids only**. The aggregate never holds a
reference to another aggregate instance and never exposes a mutable collection.

```csharp
private readonly List<Guid> _categories;                                  // backing field
public IReadOnlyCollection<Guid> Categories => _categories.AsReadOnly();  // read-only view
```

Initialise the backing list in the constructor. Expose exactly three mutators:
`Add<Relation>`, `Remove<Relation>`, `RemoveAll<Relation>s`. Each one ends with
`Validate()`. Nothing outside the aggregate may add, remove or reorder an element.

## 3. Repository interface

Interface only, in `Repository/`. The implementation lives in `Infra.Data.EF` and is
out of scope here.

```csharp
public interface I<Aggregate>Repository
    : IGenericRepository<<Aggregate>>, ISearchableRepository<<Aggregate>>;
```

That covers `Insert`, `Get`, `Delete`, `Update` and `Search`. Declare an extra method
only when a concrete use case needs it, and name it for what it returns —
`ICategoryRepository` adds `ListByIds` and `ListIdsByIds` because genre reads need
category names and because relation ids must be validated before a write.

Drop `ISearchableRepository` only if the aggregate genuinely will never be listed with
paging, search and ordering. Both current aggregates are searchable.

## 4. TDD order

One behaviour per RED/GREEN/BLUE cycle, in this order, each cycle starting from a test
that was written first and seen failing for the right reason:

1. `Instantiate` — required state, defaults, `Id` and `CreatedAt` populated.
2. `InstantiateWithIsActive` — the optional constructor parameter.
3. One cycle per invalid-input rule, asserting `EntityValidationException` and the
   exact message from `DomainValidation`.
4. `Activate` / `Deactivate`.
5. `Update`.
6. Relations, one cycle each: `Add`, `Add` many, `Remove`, `RemoveAll`.

Run a single suite while iterating, the whole suite before declaring done:

```bash
dotnet test --filter "FullyQualifiedName~<Aggregate>Test"
dotnet test
dotnet csharpier format .
```

## 5. Test fixture

The aggregate's tests live in `Codeflix.Catalog.UnitTests` under
`Domain/Entity/<Aggregate>/`, two files:

- **`<Aggregate>TestFixture.cs`** declares the collection definition *and* the fixture,
  never split across files. The fixture inherits `Common/BaseFixture`, which already
  provides `Faker("pt_BR")` and `GetRandomBoolean()`. It exposes `GetValidName()`,
  `GetValid<Aggregate>()` and one `GetInvalid...` helper per validation rule the suite
  exercises. Keep helpers that are not about this aggregate out of it.
- **`<Aggregate>Test.cs`** carries `[Collection(nameof(<Aggregate>TestFixture))]` and
  takes the fixture as a primary-constructor parameter. Every method is
  `[Fact(DisplayName = nameof(TheMethod))]` or `[Theory(...)]` plus
  `[Trait("Domain", "<Aggregate> - Aggregate")]`. `DisplayName` must be `nameof` the
  method itself, because `dotnet test --filter "DisplayName=X"` depends on it.

The test file aliases the entity namespace, because the test namespace collides with
the entity name: `using DomainEntity = Codeflix.Catalog.Domain.Entity;`.

Assertions use FluentAssertions. Full skeletons in `references/templates.md`.

## 6. Do not copy these

- **`Category` uses `DateTime.Now` for `CreatedAt`.** Use `DateTime.UtcNow`, as
  `Genre` does.
- **`Category` has FluentValidation input validators.** Those live in the Application
  layer, are registered nowhere and are invoked by nothing. An aggregate's validation
  is `Validate()` and nothing else.
- **`GenreTestFixture` redeclares `GetRandomBoolean()`**, already inherited from
  `BaseFixture`. Do not duplicate it.
- **No navigation properties, no join model, no foreign-key property** on the entity.
  Persistence rehydrates relations by calling the public `Add<Relation>` mutator, so
  that mutator must stay callable repeatedly without corrupting state, and it does not
  deduplicate. Readers dedupe.

## 7. Before finishing

- [ ] Entity inherits `AggregateRoot`, no `Id` declared or assigned by hand.
- [ ] Every property has a private setter; no collection is exposed mutable.
- [ ] `Validate()` is private and is the last statement of the constructor and of
      every mutator.
- [ ] No dependency outside `Codeflix.Catalog.Domain`.
- [ ] Repository interface declares only what a use case actually needs.
- [ ] Every behaviour was driven by a test seen failing first.
- [ ] `dotnet test` green, `dotnet csharpier format .` run.
