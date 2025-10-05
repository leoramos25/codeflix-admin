using System.Net;
using Codeflix.Catalog.Api.ApiModels;
using Codeflix.Catalog.Application.UseCases.Category.Create;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Codeflix.Catalog.EndToEndTests.Api.Category.CreateCategory;

[Collection(nameof(CreateCategoryApiTestFixture))]
public class CreateCategoryApiTest(CreateCategoryApiTestFixture fixture) : IDisposable
{
    [Fact(DisplayName = nameof(CreateCategory))]
    [Trait("EndToEnd/Api", "CreateCategory - Endpoints")]
    public async Task CreateCategory()
    {
        var input = fixture.GetValidInput();

        var (response, output) = await fixture.ApiClient.Post<ApiOutput<CreateCategoryOutput>>(
            "/categories",
            input,
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        output.Should().NotBeNull();
        output.Data.Name.Should().Be(input.Name);
        output.Data.Description.Should().Be(input.Description);
        output.Data.IsActive.Should().Be(input.IsActive);
        output.Data.Id.Should().NotBeEmpty();
        output.Data.CreatedAt.Should().NotBeSameDateAs(default);

        var createdCategory = await fixture.Persistence.GetById(output.Data.Id);

        createdCategory.Should().NotBeNull();
        createdCategory.Name.Should().Be(input.Name);
        createdCategory.Description.Should().Be(input.Description);
        createdCategory.IsActive.Should().Be(input.IsActive);
        createdCategory.Id.Should().NotBeEmpty();
        createdCategory.CreatedAt.Should().NotBeSameDateAs(default);
    }

    [Theory(DisplayName = nameof(ThrowWhenCantInstantiateCategory))]
    [Trait("EndToEnd/Api", "CreateCategory - Endpoints")]
    [MemberData(
        nameof(CreateCategoryApiTestDataGenerator.GetInvalidInputs),
        MemberType = typeof(CreateCategoryApiTestDataGenerator)
    )]
    public async Task ThrowWhenCantInstantiateCategory(
        CreateCategoryInput input,
        string errorMessage
    )
    {
        var (response, output) = await fixture.ApiClient.Post<ProblemDetails>(
            "/categories",
            input,
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        output.Should().NotBeNull();
        output.Title.Should().Be("One or more validation errors occurred");
        output.Type.Should().Be("UnprocessableEntity");
        output.Detail.Should().Be(errorMessage);
        output.Status.Should().Be(StatusCodes.Status422UnprocessableEntity);
    }

    public void Dispose() => fixture.CleanPersistence();
}
