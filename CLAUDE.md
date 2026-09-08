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

## Commands

```bash
dotnet build
dotnet test
docker compose up -d   # MySQL on port 33060, required by the end-to-end tests

# Always run the API on a port in the 5000-5099 range:
dotnet run --project src/Codeflix.Catalog.Api --urls "http://localhost:5000"
```

## Ports

**Always run the API on a port in the `5000-5099` range** (5000, 5001, ... 5099) — never on any
other port. If 5000 is taken, pick the next free port inside that range instead of letting the
tooling choose one. Pass it explicitly with `--urls` (or `ASPNETCORE_URLS`), because the `http`
profile in `Properties/launchSettings.json` still defaults to 5208, which is outside the range.
