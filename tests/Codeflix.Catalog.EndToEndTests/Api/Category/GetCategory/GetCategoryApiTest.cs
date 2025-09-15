using System.Net;
using Codeflix.Catalog.Application.UseCases.Category.Get;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace Codeflix.Catalog.EndToEndTests.Api.Category.GetCategory;

[Collection(nameof(GetCategoryApiTestFixture))]
public class GetCategoryApiTest(GetCategoryApiTestFixture fixture)
{
    [Fact(DisplayName = nameof(Get))]
    [Trait("EndToEnd/Api", "GetCategory - Endpoints")]
    public async Task Get()
    {
        var categories = fixture.GetCategories();
        var category = categories[5];
        await fixture.Persistence.InsertList(categories, CancellationToken.None);

        var (response, output) = await fixture.ApiClient.Get<GetCategoryOutput>(
            $"/categories/{category.Id}",
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output.Id.Should().Be(category.Id);
        output.Name.Should().Be(category.Name);
        output.Description.Should().Be(category.Description);
        output.IsActive.Should().Be(category.IsActive);
        output.CreatedAt.Should().Be(category.CreatedAt);
    }

    [Fact(DisplayName = nameof(ThrowWhenCategoryNotFound))]
    [Trait("EndToEnd/Api", "GetCategory - Endpoints")]
    public async Task ThrowWhenCategoryNotFound()
    {
        var invalidId = Guid.NewGuid();
        var (response, output) = await fixture.ApiClient.Get<ProblemDetails>(
            $"/categories/{invalidId}",
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
}
