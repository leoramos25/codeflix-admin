# Codeflix.Catalog

.NET 8 catalog admin API built with Clean Architecture: `Codeflix.Catalog.Domain`,
`.Application`, `.Infra.Data.EF` and `.Api` under `src/`, with unit, integration and
end-to-end test projects under `tests/`.

## Rules — ALWAYS LOADED, ALWAYS APPLY

Every file under `.claude/rules/` is **mandatory context for every task in this repository**.
Load all of them at the start of the session, before planning or touching any file, and keep
following them for the whole session — they are not optional reference material and are not
to be consulted only when something seems relevant. When a rule conflicts with a habit,
a general convention or a suggestion in the prompt, **the rule wins**; if you believe a rule
must be broken, say so explicitly and ask first.

- @.claude/rules/folder-structure.md — where every file goes, project by project, and the
  naming conventions for use cases, controllers and test suites. Applies to every new file,
  folder, namespace or project you create or move.
- @.claude/rules/tdd.md — the mandatory RED/GREEN/BLUE cycle. No production code is written
  while there is no failing test that requires it. Applies to every change that touches
  source code.
- @.claude/rules/ef-core.md — how persistence is mapped: DbContext shape, one explicit
  `IEntityTypeConfiguration` per persisted type, how EF builds rich aggregates without
  setters, join models, repositories that never save, and migrations. Applies to every
  change in `Infra.Data.EF` and to any domain change that alters what is persisted.
- @.claude/rules/aggregate-relationships.md — DDD rules for relations between aggregates:
  reference by id only, one aggregate per transaction, related-id validation in the use
  case, batched reads, join-table persistence and the tests a relation needs.
- @.claude/rules/api-configuration.md — the Api as composition root: `Program.cs` as a
  chain of `Add<Concern>`/`Use<Concern>` extensions, one `<Concern>Configuration` class per
  concern, DI lifetimes, the HTTP contract (snake_case, string enums, envelopes, exception
  mapping), and appsettings per environment. Applies to every change in `Program.cs`,
  `Configurations/`, `Filters/`, `appsettings*.json` and the Api `.csproj`.

## Commands

```bash
dotnet tool restore                # CSharpier (pinned in dotnet-tools.json)
dotnet build
dotnet test
dotnet csharpier format .          # the formatter; a hook also runs it on every edited .cs
docker compose up -d               # two MySQL: `database` on 3306 (dev / migrations),
                                   #            `database-e2e` on 33060 (end-to-end tests)

# Always run the API on a port in the 5000-5099 range:
dotnet run --project src/Codeflix.Catalog.Api --urls "http://localhost:5000"

# Migrations (generated code — ask before creating one, see tdd.md):
dotnet ef migrations has-pending-model-changes --project src/Codeflix.Catalog.Infra.Data.EF \
  --startup-project src/Codeflix.Catalog.Api   # non-zero exit = model changed, migration missing
dotnet ef migrations add <Name> --project src/Codeflix.Catalog.Infra.Data.EF \
  --startup-project src/Codeflix.Catalog.Api
```

## Migrations check

Before declaring any task done or committing, run `dotnet ef migrations has-pending-model-changes`
(above) whenever the change touches persistence: an entity property, a `DbSet` in
`CodeflixCatalogDbContext`, a file in `Infra.Data.EF/Configurations/` or `Models/`. If it reports
pending changes, say so and ask before running `migrations add`. The end-to-end tests do **not**
catch a missing migration — `CustomWebApplicationFactory` builds the schema with `EnsureCreated`,
so they pass while the dev database (`database`, port 3306) is missing the table.

## Harness

`.claude/settings.json` (committed) pre-approves build/test/format/read-only git and
wires two hooks in `.claude/hooks/`:

- `format-csharp.py` — PostToolUse on Write/Edit: runs CSharpier on the edited `.cs`
  file (skips `Migrations/`).
- `guard-api-port.py` — PreToolUse on Bash: blocks `dotnet run` without an explicit
  `--urls`/`ASPNETCORE_URLS` port in 5000-5099.

Direct edits under `Infra.Data.EF/Migrations/` are denied; regenerate instead.

## Commits

One commit per use case or per layer step, Conventional Commits in English:
`feat: add <verb> <aggregate> use case`, `fix: ...`, `test: ...`, `chore: ...`.

## Ports

**Always run the API on a port in the `5000-5099` range** (5000, 5001, ... 5099) — never on any
other port. If 5000 is taken, pick the next free port inside that range instead of letting the
tooling choose one. Pass it explicitly with `--urls` (or `ASPNETCORE_URLS`), because the `http`
profile in `Properties/launchSettings.json` still defaults to 5208, which is outside the range.
The `guard-api-port.py` hook enforces this.
