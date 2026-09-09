using System.Net;
using Codeflix.Catalog.Api.ApiModels;
using Codeflix.Catalog.Application.UseCases.Genre.Create;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Codeflix.Catalog.EndToEndTests.Api.Genre.CreateGenre;

[Collection(nameof(CreateGenreApiTestFixture))]
public class CreateGenreApiTest(CreateGenreApiTestFixture fixture)
{
    [Fact(DisplayName = nameof(CreateGenre))]
    [Trait("EndToEnd/Api", "CreateGenre - Endpoints")]
    public async Task CreateGenre()
    {
        var input = fixture.GetValidInput();

        var (response, output) = await fixture.ApiClient.Post<ApiOutput<CreateGenreOutput>>(
            "/genres",
            input,
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        output.Should().NotBeNull();
        output!.Data.Id.Should().NotBeEmpty();
        output.Data.Name.Should().Be(input.Name);
        output.Data.IsActive.Should().Be(input.IsActive);
        output.Data.CreatedAt.Should().NotBeSameDateAs(default);
        output.Data.Categories.Should().BeEmpty();

        var dbContext = fixture.CreateDbContext();
        var dbGenre = await dbContext
            .Genres.AsNoTracking()
            .FirstOrDefaultAsync(genre => genre.Id == output.Data.Id, CancellationToken.None);
        dbGenre.Should().NotBeNull();
        dbGenre!.Name.Should().Be(input.Name);
        dbGenre.IsActive.Should().Be(input.IsActive);
    }

    [Fact(DisplayName = nameof(CreateGenreWithCategories))]
    [Trait("EndToEnd/Api", "CreateGenre - Endpoints")]
    public async Task CreateGenreWithCategories()
    {
        var categories = fixture.GetValidCategories(5);
        await fixture.Persistence.InsertCategoriesList(categories, CancellationToken.None);
        var categoryIds = categories.Select(category => category.Id).ToList();
        var input = fixture.GetValidInput(categoryIds);

        var (response, output) = await fixture.ApiClient.Post<ApiOutput<CreateGenreOutput>>(
            "/genres",
            input,
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        output.Should().NotBeNull();
        output!.Data.Id.Should().NotBeEmpty();
        output.Data.Name.Should().Be(input.Name);
        output.Data.IsActive.Should().Be(input.IsActive);
        output.Data.Categories.Should().BeEquivalentTo(categoryIds);

        var dbContext = fixture.CreateDbContext();
        var dbGenre = await dbContext
            .Genres.AsNoTracking()
            .FirstOrDefaultAsync(genre => genre.Id == output.Data.Id, CancellationToken.None);
        dbGenre.Should().NotBeNull();
        var relations = await dbContext
            .GenresCategories.AsNoTracking()
            .Where(relation => relation.GenreId == output.Data.Id)
            .ToListAsync(CancellationToken.None);
        relations.Select(relation => relation.CategoryId).Should().BeEquivalentTo(categoryIds);
    }

    [Fact(DisplayName = nameof(ThrowWhenRelatedCategoryNotFound))]
    [Trait("EndToEnd/Api", "CreateGenre - Endpoints")]
    public async Task ThrowWhenRelatedCategoryNotFound()
    {
        var categories = fixture.GetValidCategories(5);
        await fixture.Persistence.InsertCategoriesList(categories, CancellationToken.None);
        var categoryIds = categories.Select(category => category.Id).ToList();
        var invalidCategoryId = Guid.NewGuid();
        categoryIds.Add(invalidCategoryId);
        var input = fixture.GetValidInput(categoryIds);

        var (response, output) = await fixture.ApiClient.Post<ProblemDetails>(
            "/genres",
            input,
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        output.Should().NotBeNull();
        output!.Status.Should().Be(StatusCodes.Status422UnprocessableEntity);
        output.Title.Should().Be("Invalid related aggregate");
        output.Type.Should().Be("RelatedAggregate");
        output.Detail.Should().Be($"Related category ids not found {invalidCategoryId}");
    }

    [Theory(DisplayName = nameof(ThrowWhenNameIsInvalid))]
    [Trait("EndToEnd/Api", "CreateGenre - Endpoints")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ThrowWhenNameIsInvalid(string invalidName)
    {
        var input = new CreateGenreInput(invalidName);

        var (response, output) = await fixture.ApiClient.Post<ProblemDetails>(
            "/genres",
            input,
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        output.Should().NotBeNull();
        output!.Status.Should().Be(StatusCodes.Status422UnprocessableEntity);
        output.Title.Should().Be("One or more validation errors occurred");
        output.Type.Should().Be("UnprocessableEntity");
        output.Detail.Should().Be("Name should not be empty or null");
    }
}
