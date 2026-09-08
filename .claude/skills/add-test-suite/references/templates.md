# Skeletons

Replace `<Aggregate>` (e.g. `Genre`), `<UseCase>` (e.g. `CreateGenre`),
`<Verb>` (the `UseCases/<Aggregate>/<Verb>` folder, e.g. `Create`).

## Unit — `UnitTests/Application/<Aggregate>/<UseCase>/`

`<UseCase>TestFixture.cs`

```csharp
using Codeflix.Catalog.Application.UseCases.<Aggregate>.<Verb>;
using Codeflix.Catalog.UnitTests.Application.<Aggregate>.Common;

namespace Codeflix.Catalog.UnitTests.Application.<Aggregate>.<UseCase>;

[CollectionDefinition(nameof(<UseCase>TestFixture))]
public class <UseCase>TestFixtureCollection : ICollectionFixture<<UseCase>TestFixture>;

public class <UseCase>TestFixture : <Aggregate>UseCaseBaseFixture
{
    public <UseCase>Input GetValidInput()
    {
        return new <UseCase>Input(GetValidName(), GetRandomBoolean());
    }
}
```

`<UseCase>Test.cs`

```csharp
using Codeflix.Catalog.Application.UseCases.<Aggregate>.<Verb>;
using FluentAssertions;
using Moq;

namespace Codeflix.Catalog.UnitTests.Application.<Aggregate>.<UseCase>;

[Collection(nameof(<UseCase>TestFixture))]
public class <UseCase>Test(<UseCase>TestFixture fixture)
{
    [Fact(DisplayName = nameof(<MethodName>))]
    [Trait("Application", "<UseCase> - Use Cases")]
    public async Task <MethodName>()
    {
        var unitOfWork = fixture.GetUnitOfWork();
        var repository = fixture.Get<Aggregate>Repository();
        var useCase = new Catalog.Application.UseCases.<Aggregate>.<Verb>.<UseCase>(
            repository.Object,
            unitOfWork.Object
        );
        var input = fixture.GetValidInput();

        var output = await useCase.Handle(input, CancellationToken.None);

        repository.Verify(
            repo => repo.Insert(
                It.IsAny<Catalog.Domain.Entity.<Aggregate>>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
        unitOfWork.Verify(uow => uow.Commit(It.IsAny<CancellationToken>()), Times.Once());
        output.Id.Should().NotBeEmpty();
        output.Name.Should().Be(input.Name);
    }
}
```

The `Domain.Entity.<Aggregate>` name collides with the test namespace segment —
existing tests disambiguate with a fully-qualified `Catalog.Domain.Entity.X`, or
a `using DomainEntity = Codeflix.Catalog.Domain.Entity;` alias. Either is fine;
match the file you are next to.

## Integration — `IntegrationTests/Application/UseCases/<Aggregate>/<UseCase>/`

Same fixture shape, inheriting `<Aggregate>UseCaseTestFixture`. Test class:

```csharp
using Codeflix.Catalog.Infra.Data.EF;
using Codeflix.Catalog.Infra.Data.EF.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UseCase = Codeflix.Catalog.Application.UseCases.<Aggregate>.<Verb>;

namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.<Aggregate>.<UseCase>;

[Collection(nameof(<UseCase>TestFixture))]
public class <UseCase>Test(<UseCase>TestFixture fixture)
{
    [Fact(DisplayName = nameof(<MethodName>))]
    [Trait("Integration/Application", "<UseCase> - Use Cases")]
    public async Task <MethodName>()
    {
        var input = fixture.GetValidInput();
        var dbContext = fixture.CreateDbContext();
        var unitOfWork = new UnitOfWork(dbContext);
        var repository = new <Aggregate>Repository(dbContext);
        var useCase = new UseCase.<UseCase>(repository, unitOfWork);

        var output = await useCase.Handle(input, CancellationToken.None);

        // second context — fresh change tracker over the same store
        var assertDbContext = fixture.CreateDbContext(true);
        var fromDb = await assertDbContext.<Aggregate>s.FindAsync(output.Id);

        fromDb.Should().NotBeNull();
        fromDb!.Name.Should().Be(input.Name);
    }
}
```

Seeding: add entities to `dbContext` and `SaveChangesAsync` **before**
constructing the use case.

## End-to-end — `EndToEndTests/Api/<Aggregate>/<UseCase>/`

`<UseCase>ApiTestFixture.cs` is usually empty — all helpers live in the
aggregate base fixture:

```csharp
using Codeflix.Catalog.EndToEndTests.Api.<Aggregate>.Common;

namespace Codeflix.Catalog.EndToEndTests.Api.<Aggregate>.<UseCase>;

[CollectionDefinition(nameof(<UseCase>ApiTestFixture))]
public class <UseCase>ApiTestFixtureCollection : ICollectionFixture<<UseCase>ApiTestFixture>;

public class <UseCase>ApiTestFixture : <Aggregate>BaseFixture;
```

`<UseCase>ApiTest.cs`

```csharp
using System.Net;
using Codeflix.Catalog.Api.ApiModels;
using Codeflix.Catalog.Application.UseCases.<Aggregate>.<Verb>;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace Codeflix.Catalog.EndToEndTests.Api.<Aggregate>.<UseCase>;

[Collection(nameof(<UseCase>ApiTestFixture))]
public class <UseCase>ApiTest(<UseCase>ApiTestFixture fixture)
{
    [Fact(DisplayName = nameof(<MethodName>))]
    [Trait("EndToEnd/Api", "<UseCase> - Endpoints")]
    public async Task <MethodName>()
    {
        var items = fixture.GetValid<Aggregate>s(10);
        await fixture.Persistence.InsertList(items, CancellationToken.None);
        var target = items[5];

        var (response, output) = await fixture.ApiClient.Get<ApiOutput<<UseCase>Output>>(
            $"/<route>/{target.Id}",
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Data.Id.Should().Be(target.Id);
    }
}
```

`ApiClient` exposes `Get`/`Post`/`Put`/`Delete`, each returning
`(HttpResponseMessage, TOutput?)` and handling snake_case both ways. `Get` takes
an optional `query` object for query-string params.

Error-path test:

```csharp
var (response, output) = await fixture.ApiClient.Get<ProblemDetails>(
    $"/<route>/{Guid.NewGuid()}",
    CancellationToken.None
);

response.StatusCode.Should().Be(HttpStatusCode.NotFound);
output!.Status.Should().Be((int)HttpStatusCode.NotFound);
output.Title.Should().Be("Not found");
output.Type.Should().Be("NotFound");
```

Status mapping comes from `GlobalExceptionFilter`:
`EntityValidationException` → 422 `UnprocessableEntity`,
`NotFoundException` → 404 `NotFound`, anything else → 500 `InternalServerError`.
