# Codeflix.Catalog

.NET 8 catalog admin API built with Clean Architecture: `Codeflix.Catalog.Domain`,
`.Application`, `.Infra.Data.EF` and `.Api` under `src/`, with unit, integration and
end-to-end test projects under `tests/`.

## Rules — ALWAYS LOADED, ALWAYS APPLY

Every file under `.claude/rules/` is **mandatory context for every task in this repository**.
They are imported below with `@`, so they are already in context when the session starts — do
not re-read them; follow them for the whole session, before planning or touching any file.
They are not optional reference material and are not to be consulted only when something
seems relevant. When a rule conflicts with a habit,
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
- @.claude/rules/api-configuration.md — the Api as composition root: `Program.cs` as a
  chain of `Add<Concern>`/`Use<Concern>` extensions, one `<Concern>Configuration` class per
  concern, DI lifetimes, the HTTP contract (snake_case, string enums, envelopes, exception
  mapping), and appsettings per environment. Applies to every change in `Program.cs`,
  `Configurations/`, `Filters/`, `appsettings*.json` and the Api `.csproj`.
- @.claude/rules/testing.md — how the three test projects are configured: shared xUnit stack,
  collection fixtures, Moq in unit tests, EF InMemory in integration, MySQL container behind
  `WebApplicationFactory` in end-to-end, serialized collections and what each level cannot
  catch. Applies to every file under `tests/`, every test `.csproj` and `xunit.runner.json`.

## Commands

```bash
dotnet tool restore                # CSharpier + dotnet-ef (both pinned in dotnet-tools.json)
dotnet build
dotnet test
dotnet csharpier format .          # the formatter; a hook also runs it on every edited .cs
docker compose up -d               # two MySQL: `database` on 3306 (dev / migrations),
                                   #            `database-e2e` on 33060 (end-to-end tests)

dotnet run --project src/Codeflix.Catalog.Api --urls "http://localhost:5000"   # see Ports

# Migrations (generated code — ask before creating one, see tdd.md). Both need `database`
# (3306) up: `ServerVersion.AutoDetect` connects to MySQL at design time.
dotnet ef migrations has-pending-model-changes --project src/Codeflix.Catalog.Infra.Data.EF \
  --startup-project src/Codeflix.Catalog.Api
dotnet ef migrations add <Name> --project src/Codeflix.Catalog.Infra.Data.EF \
  --startup-project src/Codeflix.Catalog.Api
```

## Migrations check

Before declaring any task done or committing, run `dotnet ef migrations has-pending-model-changes`
(above) whenever the change touches persistence: an entity property, a `DbSet` in
`CodeflixCatalogDbContext`, a file in `Infra.Data.EF/Configurations/` or `Models/`.

Start `database` first (`docker compose up -d database`), then read the output, not only the
exit code — a non-zero exit has two causes:

- `Changes have been made to the model since the last migration` → a migration is missing.
  Say so and ask before running `migrations add`.
- `Unable to connect to any of the specified MySQL hosts` → `database` is down. This says
  nothing about the model; start it and run the check again. Never report a missing migration
  from this output.

The end-to-end tests do **not** catch a missing migration — `CustomWebApplicationFactory` builds
the schema with `EnsureCreated`, so they pass while the dev database (`database`, port 3306) is
missing the table.

## Harness

`.claude/settings.json` (committed) pre-approves build/test/format/read-only git and the
pending-migrations check, and wires two hooks in `.claude/hooks/`:

- `format-csharp.py` — PostToolUse on Write/Edit: runs CSharpier on the edited `.cs`
  file (skips `Migrations/`). If CSharpier fails, the error is fed back to Claude (exit 2) —
  usually `dotnet tool restore` was not run. Files written through Bash are not formatted;
  run `dotnet csharpier format .` before finishing.
- `guard-api-port.py` — PreToolUse on Bash: blocks `dotnet run`, `dotnet watch` and
  `dotnet <app>.dll` unless every `--urls`/`ASPNETCORE_URLS` URL has an explicit port in
  5000-5099. Text inside quotes and heredoc bodies is ignored.

Direct edits under `Infra.Data.EF/Migrations/` are denied; regenerate instead.

## Commits

One commit per use case or per layer step, Conventional Commits in English:
`feat: add <verb> <aggregate> use case`, `fix: ...`, `test: ...`, `chore: ...`.

## Ports

**Always run the API on a port in the `5000-5099` range** (5000, 5001, ... 5099) — never on any
other port. If 5000 is taken, pick the next free port inside that range instead of letting the
tooling choose one. Pass it explicitly with `--urls` (or `ASPNETCORE_URLS`) even though the
`http`/`https` profiles in `Properties/launchSettings.json` use 5000/5001 — the explicit port is
what lets you move to the next free one. The `guard-api-port.py` hook enforces this.
