# API configuration

> **Always in effect.** This rule is loaded for every task in this repository and applies to
> every change to `Program.cs`, `Configurations/`, `Filters/`, `appsettings*.json`,
> `launchSettings.json` and the Api `.csproj` — no exceptions without asking first.

`Codeflix.Catalog.Api` is the composition root: the only project that knows every layer, and
the only place where they are wired together. It holds no business rule — controllers
translate HTTP to a MediatR request and back, and everything else here is configuration.

## Program.cs stays a table of contents

```csharp
var builder = WebApplication.CreateBuilder(args);
builder
    .Services.AddControllersConfiguration()
    .AddUseCases()
    .AddApiConnections(builder.Configuration);
var app = builder.Build();
app.UseDocumentation();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program;   // required by WebApplicationFactory<Program> in EndToEndTests
```

- No `services.AddXxx(...)` with options, no lambdas, no `if (env...)` in `Program.cs`. Every
  registration goes through an extension method from `Configurations/`.
- Keep `public partial class Program;` — the end-to-end suite depends on it.
- New middleware goes into the pipeline section in the order ASP.NET Core requires
  (exception handling → HTTPS → routing/auth → endpoints); if it needs configuration, wrap it
  in a `Use<Concern>()` extension next to its `Add<Concern>()`.

## Configurations/: one static class per concern

```
Configurations/
  ControllersConfiguration.cs   # MVC, filters, JSON options, Swagger
  UseCasesConfiguration.cs      # MediatR, repositories, IUnitOfWork
  ConnectionsConfiguration.cs   # DbContext and any other external connection
```

- `public static class <Concern>Configuration` in namespace `Codeflix.Catalog.Api.Configurations`.
- One public entry point `Add<Concern>(this IServiceCollection services[, IConfiguration])`
  that **returns `services`** so `Program.cs` can chain. Sub-steps are `private static`
  extensions (`AddRepositories`, `AddDbConnection`, `AddDocumentation`) called from it.
- Pipeline counterparts are `Use<Concern>(this WebApplication app)` returning `app`, in the
  same class as their `Add` (`UseDocumentation` lives with `AddDocumentation`).
- A new concern (cache, messaging, auth, health checks) gets a new `<Concern>Configuration.cs`
  — do not grow an existing class with an unrelated concern.

### What goes where

| Registration | Class | Rule |
|---|---|---|
| Use cases | `UseCasesConfiguration` | `AddMediatR(c => c.RegisterServicesFromAssembly(typeof(CreateCategory).Assembly))` — handlers are discovered, never registered one by one. |
| Repositories | `UseCasesConfiguration.AddRepositories` | `AddScoped<I<Aggregate>Repository, <Aggregate>Repository>()`, one line per aggregate. |
| Unit of work | `UseCasesConfiguration.AddRepositories` | `AddScoped<IUnitOfWork, UnitOfWork>()`. |
| DbContext | `ConnectionsConfiguration.AddDbConnection` | `AddDbContext<CodeflixCatalogDbContext>(o => o.UseMySql(cs, ServerVersion.AutoDetect(cs)))` with `cs = configuration.GetConnectionString("CatalogDb")`. |
| MVC, filters, JSON, Swagger | `ControllersConfiguration` | see below. |

- **Lifetimes:** repositories, `IUnitOfWork` and the `DbContext` are all **scoped**, so one
  HTTP request shares one context and one transaction. Never register them as singleton or
  transient; never inject a scoped service into a singleton.
- A repository missing from `AddRepositories` only fails at request time (MediatR cannot build
  the handler → 500). Every new aggregate's end-to-end suite covers that; do not ship an
  aggregate without one.

## HTTP contract (ControllersConfiguration)

- **JSON is `snake_case`**: `JsonNamingPolicy.SnakeCaseLower` on the System.Text.Json options.
  Query parameters with more than one word declare their name explicitly
  (`[FromQuery(Name = "per_page")]`). System.Text.Json is the only serializer in the Api; do
  not add Newtonsoft to it.
- **Enums in bodies are written as strings and read as the member name (case-insensitive)
  or its defined integer value** (`"Director"`, `"director"`, `1`). Numeric strings (`"1"`)
  and undefined integers (`0`, `99`) are rejected with `400` on `$.<field>`. Use a per-enum
  `JsonConverter<TEnum>` class in `ApiModels/<Aggregate>/` (`CastMemberTypeJsonConverter`)
  applied with `[property: JsonConverter(...)]` on the API input — not
  `JsonStringEnumConverter(allowIntegerValues: true)`, which accepts any integer and numeric
  strings. Do not register a global enum converter, which would silently change every enum
  already in the contract.
- **Envelopes:** single resources return `ApiOutput<T>` (`{ "data": ... }`), lists return
  `ApiListOutput<T>` (`{ "data": [...], "meta": { "current_page", "per_page", "total" } }`).
  Errors are `ProblemDetails`. Never return a bare DTO.
- **Errors are mapped in one place**, `Filters/GlobalExceptionFilter`, registered globally in
  `AddControllers(o => o.Filters.Add(typeof(GlobalExceptionFilter)))`. Controllers and use
  cases never `try/catch` to build a response.

| Exception | Status | `Type` |
|---|---|---|
| `EntityValidationException` (domain) | 422 | `UnprocessableEntity` |
| `RelatedEntityException` (domain) | 422 | `RelatedAggregate` |
| `NotFoundException` (application) | 404 | `NotFound` |
| anything else | 500 | `InternalServerError` |

  A new exception type that should reach the client gets its own branch here, driven by an
  end-to-end test asserting status and `type`.
- **Swagger** (`AddEndpointsApiExplorer` + `AddSwaggerGen`) is registered always but exposed
  only in `Development` and `Docker` (`UseDocumentation`). Controllers document every outcome
  with `[ProducesResponseType]`.

## Settings and environments

| File | Environment | `CatalogDb` points to |
|---|---|---|
| `appsettings.json` | all (base) | — (logging, `AllowedHosts`) |
| `appsettings.Development.json` | `Development` | `localhost:3306` → compose `database` |
| `appsettings.Docker.json` | `Docker` | `catalogdb:3306` → compose network |
| `appsettings.EndToEndTest.json` | `EndToEndTest` (set by `CustomWebApplicationFactory`) | `localhost:33060` → compose `database-e2e` |

- A new environment gets its own `appsettings.<Environment>.json`; code reads only
  `IConfiguration` / `GetConnectionString`, never `Environment.GetEnvironmentVariable`.
- Connection strings are named after the bounded context's store (`CatalogDb`), not after the
  technology. The ones committed here are for the local Docker databases only; any real
  environment supplies `ConnectionStrings__CatalogDb` as an environment variable or secret —
  never commit a non-local credential.
- The end-to-end environment must point to the `database-e2e` instance; the factory runs
  `EnsureDeleted` on it, so pointing it at the dev database wipes it.
- **Ports:** the API always runs on 5000-5099 with an explicit `--urls`/`ASPNETCORE_URLS`
  (see `CLAUDE.md`); `launchSettings.json` is not a source of truth for ports.

## Testing configuration changes

Behaviour visible over HTTP — JSON naming, enum format, envelopes, status codes, problem
details, a newly registered repository — is driven by an end-to-end test seen failing first.
Pure wiring with no observable behaviour (a package reference, a logging level, Swagger UI
tweaks, a new appsettings file) cannot be driven by a test: say so and ask before changing it,
as the TDD rule requires.

## Do not copy these (current deviations)

- The Api `.csproj` references `Microsoft.EntityFrameworkCore.InMemory` and `Newtonsoft.Json`,
  neither used by the Api (InMemory belongs to IntegrationTests, Newtonsoft to EndToEndTests).
- `GlobalExceptionFilter` returns `exception.Message` in the 500 response in every
  environment. Only expose it (and the stack trace) in `Development`; elsewhere use a generic
  detail and log the exception.
- `app.UseAuthorization()` is in the pipeline with no authentication configured — add both
  together when auth arrives, not one without the other.
- `launchSettings.json` profiles default to port 5208 (outside the allowed range).
