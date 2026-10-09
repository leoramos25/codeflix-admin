---
name: add-endpoint
description: Expose use cases over HTTP in Codeflix.Catalog.Api — the plural controller over IMediator, routes and status codes, the ApiOutput/ApiListOutput envelopes, request models in ApiModels/<Aggregate>, DI registration in UseCasesConfiguration, exception-to-ProblemDetails mapping, and the end-to-end tests over real MySQL that drive it. Use when adding or changing a controller action, an API request model, a repository registration, or an end-to-end test.
---

# Adding an endpoint

Scope: `src/Codeflix.Catalog.Api` and `tests/Codeflix.Catalog.EndToEndTests/`. The use
case and the repository already exist and are tested; this task wires them to HTTP.

`GenresController` is the reference.

## 1. Files touched

```
src/Codeflix.Catalog.Api/
  Controllers/<Aggregate>sController.cs        # plural
  ApiModels/<Aggregate>/<Verb><Aggregate>ApiInput.cs   # only when the body differs from the use-case input
  Configurations/UseCasesConfiguration.cs      # + AddScoped<I<Aggregate>Repository, <Aggregate>Repository>()
  Filters/GlobalExceptionFilter.cs             # only for a new exception type
```

MediatR already scans the Application assembly (`RegisterServicesFromAssembly`), so
handlers need no registration — **repositories do**. A missing `AddScoped` shows up as
a 500 in the e2e test, not as a build error. Never register anything inline in
`Program.cs`.

## 2. Controller

```csharp
[ApiController]
[Route("[controller]")]          // → /castmembers, see §6
[Produces("application/json")]
public class CastMembersController(IMediator mediator) : ControllerBase
```

| Action | Verb + route | Success | Body | Errors declared |
|---|---|---|---|---|
| Create | `POST /` | `CreatedAtAction(nameof(Get), new { output.Id }, new ApiOutput<T>(output))` | use-case input | 400, 422 |
| Get | `GET /{id:guid}` | `Ok(new ApiOutput<T>(output))` | — | 404 |
| List | `GET /` | `Ok(new ApiListOutput<TItem>(output))` | — | — |
| Update | `PUT /{id:guid}` | `Ok(new ApiOutput<T>(output))` | `<Verb><Aggregate>ApiInput` | 404, 422 |
| Delete | `DELETE /{id:guid}` | `NoContent()` | — | 404 |

- Every action takes `CancellationToken cancellationToken` and passes it to `mediator.Send`.
- Declare each response with `[ProducesResponseType(typeof(ApiOutput<T>), StatusCodes.Status200OK)]`
  and `typeof(ProblemDetails)` for errors.
- **Update**: the id comes from the route, not the body. The body is an
  `ApiModels/<Aggregate>/Update<Aggregate>ApiInput` with a
  `To<Verb><Aggregate>Input(Guid id)` method.
- **List**: query params are all optional — `page`, `per_page` (`[FromQuery(Name = "per_page")]`),
  `search`, `sort`, `dir` (`SearchOrder?`). Start from `new List<X>sInput()` and override
  only what was sent.
- JSON is `snake_case_lower` both ways (`ControllersConfiguration`).

## 3. Errors

The controller has no `try/catch`. `GlobalExceptionFilter` maps:

| Exception | Status | `Type` |
|---|---|---|
| `EntityValidationException` | 422 | `UnprocessableEntity` |
| `RelatedEntityException` | 422 | `RelatedAggregate` |
| `NotFoundException` | 404 | `NotFound` |
| anything else | 500 | `InternalServerError` |

A new exception type gets a new branch there, driven by an e2e test.

## 4. End-to-end tests (TDD)

Folder: `tests/Codeflix.Catalog.EndToEndTests/Api/<Aggregate>/`

```
Common/<Aggregate>BaseFixture.cs     # inherits Common/BaseFixture; owns Persistence + GetValid*()
Common/<Aggregate>Persistence.cs     # seeds/reads rows directly through the DbContext
<Verb><Aggregate>/<Verb><Aggregate>ApiTest.cs
<Verb><Aggregate>/<Verb><Aggregate>ApiTestFixture.cs   # collection definition + fixture
```

- Requires MySQL on 33060: `docker compose up -d`. The factory runs `EnsureDeleted` +
  `EnsureCreated` per boot (environment `EndToEndTest`).
- Trait: `[Trait("EndToEnd/Api", "<Verb><Aggregate> - Endpoints")]`.
- **Arrange through `Persistence`, never through the API**; assert the HTTP result *and*
  the row in the database through `Persistence`.
- Deserialize the envelope: `ApiOutput<TOutput>`, `ApiListOutput<TItem>`
  (check `Meta.CurrentPage/PerPage/Total`), or `ProblemDetails` for errors
  (check `Status`, `Title`, `Type`, `Detail`).
- One test per cycle: happy path, then 404, then 422 per validation rule, then list
  pagination/search/ordering cases.

```bash
dotnet test tests/Codeflix.Catalog.EndToEndTests --filter "FullyQualifiedName~<Verb><Aggregate>ApiTest"
```

## 5. Running the API by hand

Only on a port in 5000-5099, always explicit (the `http` launch profile defaults to
5208, and a hook blocks `dotnet run` without a valid `--urls`):

```bash
dotnet run --project src/Codeflix.Catalog.Api --urls "http://localhost:5000"
```

## 6. Do not copy these

- **Route casing.** `[Route("[controller]")]` yields `/castmembers` for
  `CastMembersController`. If the public route must be `/cast_members`, use an explicit
  `[Route("cast_members")]` and say so; do not rename the controller.

## 7. Before finishing

- [ ] Controller plural, in `Controllers/`; request models in `ApiModels/<Aggregate>/`.
- [ ] Repository registered in `UseCasesConfiguration.AddRepositories`.
- [ ] Every action has `ProducesResponseType` for its success and error statuses.
- [ ] Every action driven by an e2e test seen failing first (MySQL up).
- [ ] `dotnet test` green.
