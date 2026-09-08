# Folder structure

> **Always in effect.** This rule is loaded for every task in this repository and applies
> to every file, folder and namespace you create, move or rename — no exceptions without
> asking first.

Clean Architecture solution: `Codeflix.Catalog.sln` with production projects under `src/`
and test projects under `tests/`. Every project is named `Codeflix.Catalog.<Layer>` and the
**folder path mirrors the namespace exactly** (`src/Codeflix.Catalog.Application/UseCases/Genre/Get/`
→ `namespace Codeflix.Catalog.Application.UseCases.Genre.Get`). Never place a file in a folder
whose path does not match the namespace it declares.

## Dependency direction

```
Api ──► Application ──► Domain ◄── Infra.Data.EF
```

`Domain` depends on nothing. `Application` depends only on `Domain` (repository interfaces live
in the domain, implementations in `Infra.Data.EF`). Never reference `Infra.Data.EF` from
`Application` or `Domain`.

## src/

### `Codeflix.Catalog.Domain`

```
Entity/                        # one file per aggregate: Category.cs, Genre.cs
Exceptions/                    # EntityValidationException.cs, RelatedEntityException.cs
Repository/                    # I<Aggregate>Repository.cs — interfaces only
SeedWork/                      # Entity, AggregateRoot, IRepository, IGenericRepository
SeedWork/SearchableRepository/ # ISearchableRepository, SearchInput/Output, SearchOrder
Validation/                    # DomainValidation.cs
```

### `Codeflix.Catalog.Application`

```
Common/                        # cross-use-case DTOs: PaginatedListInput/Output
Exceptions/                    # NotFoundException.cs
Interfaces/                    # IUnitOfWork.cs
UseCases/<Aggregate>/<Verb>/   # one folder per use case
```

`<Aggregate>` is the singular entity name (`Category`, `Genre`). `<Verb>` is the **short** verb
folder: `Create`, `Get`, `List`, `Update`, `Delete`. Inside it, files are named
`<Verb><Aggregate>*` (the long form), one type per file:

```
UseCases/Genre/Create/
  ICreateGenre.cs              # interface — always
  CreateGenre.cs               # handler — always
  CreateGenreInput.cs          # always
  CreateGenreOutput.cs         # omit for Delete (returns void)
  CreateGenreInputValidator.cs # only when FluentValidation is used
```

`List` additionally carries `List<Aggregate>sItemOutput.cs` next to `List<Aggregate>sOutput.cs`,
and the plural is in the type name (`ListGenres`, `ListCategories`), not the folder (`List`).

### `Codeflix.Catalog.Infra.Data.EF`

```
CodeflixCatalogDbContext.cs    # at project root
UnitOfWork.cs                  # at project root
Configurations/                # <Entity>Configuration.cs — IEntityTypeConfiguration
Models/                        # persistence-only models (e.g. GenresCategories join)
Repositories/                  # <Aggregate>Repository.cs — implements the domain interface
Migrations/                    # EF-generated, do not hand-organize
```

### `Codeflix.Catalog.Api`

```
Program.cs                     # at project root
ApiModels/                     # shared envelopes: ApiOutput, ApiListOutput, ApiMetaOutput
ApiModels/<Aggregate>/         # request models that differ from the use-case input
Configurations/                # *Configuration.cs extension methods called from Program.cs
Controllers/                   # <Aggregate>sController.cs — plural
Filters/                       # GlobalExceptionFilter.cs
```

Controllers are plural (`GenresController`), everything else stays singular. A new use case is
registered in `Configurations/UseCasesConfiguration.cs`, not inline in `Program.cs`.

## tests/

Three projects, each mirroring the layer it exercises. Test folders use the **long** use-case
name (`CreateGenre`), unlike the source `Create` folder.

```
Codeflix.Catalog.UnitTests/
  Common/BaseFixture.cs
  TestModuleInitializer.cs
  Domain/Entity/<Aggregate>/<Aggregate>Test.cs + <Aggregate>TestFixture.cs
  Domain/Validation/
  Application/<Aggregate>/Common/<Aggregate>UseCaseBaseFixture.cs
  Application/<Aggregate>/<Verb><Aggregate>/     # NOTE: no UseCases/ level here

Codeflix.Catalog.IntegrationTests/
  Common/BaseFixture.cs
  Application/UseCases/<Aggregate>/Common/
  Application/UseCases/<Aggregate>/<Verb><Aggregate>/
  Infra.Data.EF/Repositories/<Aggregate>Repository/
  Infra.Data.EF/UnitOfWork/

Codeflix.Catalog.EndToEndTests/
  Common/                      # ApiClient, BaseFixture, CustomWebApplicationFactory
  Models/                      # TestApiResponseList and friends
  Api/<Aggregate>/Common/      # <Aggregate>BaseFixture.cs + <Aggregate>Persistence.cs
  Api/<Aggregate>/<Verb><Aggregate>/
```

Per use-case test folder, one type per file:

```
<Verb><Aggregate>Test.cs                 # <Verb><Aggregate>ApiTest.cs in EndToEndTests
<Verb><Aggregate>TestFixture.cs          # ...ApiTestFixture.cs
<Verb><Aggregate>TestDataGenerator.cs    # only when the suite uses [MemberData]
```

Validator tests live beside the use-case test in the same folder
(`GetCategoryInputValidatorTest.cs`).

## Adding a new aggregate

Create, in order, and keep every path parallel to `Genre`:

1. `Domain/Entity/<Aggregate>.cs` + `Domain/Repository/I<Aggregate>Repository.cs`
2. `Application/UseCases/<Aggregate>/{Create,Get,List,Update,Delete}/`
3. `Infra.Data.EF/Configurations/<Aggregate>Configuration.cs` + `Repositories/<Aggregate>Repository.cs`
4. `Api/Controllers/<Aggregate>sController.cs`, registered in `Configurations/UseCasesConfiguration.cs`
5. Test folders in all three test projects (see the `add-test-suite` skill for their contents)

Never introduce a new top-level folder in a project without a matching precedent above.
