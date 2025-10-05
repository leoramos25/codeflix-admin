using System.Net;
using Codeflix.Catalog.Api.ApiModels;
using Codeflix.Catalog.Api.ApiModels.Category;
using Codeflix.Catalog.Application.UseCases.Category.Update;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace Codeflix.Catalog.EndToEndTests.Api.Category.UpdateCategory;

[Collection(nameof(UpdateCategoryApiTestFixture))]
public class UpdateCategoryApiTest(UpdateCategoryApiTestFixture fixture) : IDisposable
{
    [Fact(DisplayName = nameof(Update))]
    [Trait("EndToEnd/Api", "UpdateCategory - Endpoints")]
    public async Task Update()
    {
        var category = fixture.GetValidCategory();
        await fixture.Persistence.InsertList([category], CancellationToken.None);
        var input = fixture.GetValidInput();

        var (response, output) = await fixture.ApiClient.Put<ApiOutput<UpdateCategoryOutput>>(
            $"/categories/{category.Id}",
            input,
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output.Data.Name.Should().Be(input.Name);
        output.Data.Description.Should().Be(input.Description);
        output.Data.Id.Should().Be(category.Id);
        output.Data.IsActive.Should().Be(input.IsActive!.Value);
        output.Data.CreatedAt.Should().BeSameDateAs(category.CreatedAt);

        var dbContext = fixture.CreateDbContext();
        var dbCategory = await dbContext.Categories.FindAsync(category.Id, CancellationToken.None);

        dbCategory.Should().NotBeNull();
        dbCategory.Name.Should().Be(input.Name);
        dbCategory.Description.Should().Be(input.Description);
        dbCategory.Id.Should().NotBeEmpty();
        dbCategory.CreatedAt.Should().BeSameDateAs(category.CreatedAt);
    }

    [Fact(DisplayName = nameof(UpdateCategoryOnlyName))]
    [Trait("EndToEnd/Api", "UpdateCategory - Endpoints")]
    public async Task UpdateCategoryOnlyName()
    {
        var category = fixture.GetValidCategory();
        await fixture.Persistence.InsertList([category], CancellationToken.None);
        var input = new UpdateCategoryInput(category.Id, fixture.GetValidCategoryName());

        var (response, output) = await fixture.ApiClient.Put<UpdateCategoryOutput>(
            $"/categories/{category.Id}",
            input,
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output.Name.Should().Be(input.Name);
        output.Description.Should().Be(category.Description);
        output.Id.Should().Be(category.Id);
        output.IsActive.Should().Be(category.IsActive);
        output.CreatedAt.Should().BeSameDateAs(category.CreatedAt);

        var dbContext = fixture.CreateDbContext();
        var dbCategory = await dbContext.Categories.FindAsync(category.Id, CancellationToken.None);

        dbCategory.Should().NotBeNull();
        dbCategory.Name.Should().Be(input.Name);
        dbCategory.Description.Should().Be(category.Description);
        dbCategory.Id.Should().NotBeEmpty();
        dbCategory.IsActive.Should().Be(category.IsActive);
        dbCategory.CreatedAt.Should().BeSameDateAs(category.CreatedAt);
    }

    [Fact(DisplayName = nameof(UpdateCategoryNameAndDescription))]
    [Trait("EndToEnd/Api", "UpdateCategory - Endpoints")]
    public async Task UpdateCategoryNameAndDescription()
    {
        var category = fixture.GetValidCategory();
        await fixture.Persistence.InsertList([category], CancellationToken.None);
        var input = new UpdateCategoryApiInput(
            fixture.GetValidCategoryName(),
            fixture.GetValidCategoryDescription()
        );

        var (response, output) = await fixture.ApiClient.Put<UpdateCategoryOutput>(
            $"/categories/{category.Id}",
            input,
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output.Name.Should().Be(input.Name);
        output.Description.Should().Be(input.Description);
        output.Id.Should().Be(category.Id);
        output.IsActive.Should().Be(category.IsActive);
        output.CreatedAt.Should().BeSameDateAs(category.CreatedAt);

        var dbContext = fixture.CreateDbContext();
        var dbCategory = await dbContext.Categories.FindAsync(category.Id, CancellationToken.None);

        dbCategory.Should().NotBeNull();
        dbCategory.Name.Should().Be(input.Name);
        dbCategory.Description.Should().Be(input.Description);
        dbCategory.Id.Should().NotBeEmpty();
        dbCategory.IsActive.Should().Be(category.IsActive);
        dbCategory.CreatedAt.Should().BeSameDateAs(category.CreatedAt);
    }

    [Fact(DisplayName = nameof(ThrowWhenCategoryNotFound))]
    [Trait("EndToEnd/Api", "UpdateCategory - Endpoints")]
    public async Task ThrowWhenCategoryNotFound()
    {
        var invalidId = Guid.NewGuid();
        var input = fixture.GetValidInput();

        var (response, output) = await fixture.ApiClient.Put<ProblemDetails>(
            $"/categories/{invalidId}",
            input,
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        output.Should().NotBeNull();
        output.Status.Should().Be((int)HttpStatusCode.NotFound);
        output.Title.Should().Be("Not found");
        output.Detail.Should().Be($"Category '{invalidId}' not found.");
        output.Type.Should().Be("NotFound");
    }

    [Theory(DisplayName = nameof(ThrowErrorWhenCantInstantiateCategory))]
    [Trait("EndToEnd/Api", "UpdateCategory - Endpoints")]
    [MemberData(
        nameof(UpdateCategoryApiTestDataGenerator.GetInvalidData),
        parameters: 3,
        MemberType = typeof(UpdateCategoryApiTestDataGenerator)
    )]
    public async Task ThrowErrorWhenCantInstantiateCategory(
        UpdateCategoryApiInput input,
        string errorMessage
    )
    {
        var category = fixture.GetValidCategory();
        await fixture.Persistence.InsertList([category], CancellationToken.None);

        var (response, output) = await fixture.ApiClient.Put<ProblemDetails>(
            $"/categories/{category.Id}",
            input,
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        output.Should().NotBeNull();
        output.Status.Should().Be((int)HttpStatusCode.UnprocessableEntity);
        output.Title.Should().Be("One or more validation errors occurred");
        output.Type.Should().Be("UnprocessableEntity");
        output.Detail.Should().Be(errorMessage);
    }

    public void Dispose() => fixture.CleanPersistence();
}
