using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace Codeflix.Catalog.EndToEndTests.Api.Category.DeleteCategory;

[Collection(nameof(DeleteCategoryApiTestFixture))]
public class DeleteCategoryApiTest(DeleteCategoryApiTestFixture fixture)
{
    [Fact(DisplayName = nameof(Delete))]
    [Trait("EndToEnd/Api", "DeleteCategory - Endpoints")]
    public async Task Delete()
    {
        var category = fixture.GetValidCategory();
        await fixture.Persistence.InsertList([category], CancellationToken.None);

        var (response, output) = await fixture.ApiClient.Delete<object>(
            $"/categories/{category.Id}",
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        output.Should().BeNull();

        var dbContext = fixture.CreateDbContext();
        var dbCategory = await dbContext.Categories.FindAsync(category.Id, CancellationToken.None);

        dbCategory.Should().BeNull();
    }

    [Fact(DisplayName = nameof(ThrowWhenCategoryNotFound))]
    [Trait("EndToEnd/Api", "DeleteCategory - Endpoints")]
    public async Task ThrowWhenCategoryNotFound()
    {
        var invalidId = Guid.NewGuid();

        var (response, output) = await fixture.ApiClient.Delete<ProblemDetails>(
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
