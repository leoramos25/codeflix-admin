using System.Net;
using Codeflix.Catalog.Api.ApiModels;
using Codeflix.Catalog.Api.ApiModels.Genre;
using Codeflix.Catalog.Application.UseCases.Genre.Update;
using Codeflix.Catalog.Infra.Data.EF.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Codeflix.Catalog.EndToEndTests.Api.Genre.UpdateGenre;

[Collection(nameof(UpdateGenreApiTestFixture))]
public class UpdateGenreApiTest(UpdateGenreApiTestFixture fixture) : IDisposable
{
    public void Dispose()
    {
        fixture.CleanPersistence();
    }

    [Fact(DisplayName = nameof(UpdateGenre))]
    [Trait("EndToEnd/Api", "UpdateGenre - Endpoints")]
    public async Task UpdateGenre()
    {
        var genres = fixture.GetValidGenres(10);
        var genre = genres[5];
        await fixture.Persistence.InsertList(genres, CancellationToken.None);
        var input = fixture.GetValidInput();

        var (response, output) = await fixture.ApiClient.Put<ApiOutput<UpdateGenreOutput>>(
            $"/genres/{genre.Id}",
            input,
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Data.Id.Should().Be(genre.Id);
        output.Data.Name.Should().Be(input.Name);
        output.Data.IsActive.Should().Be(input.IsActive!.Value);
        output.Data.CreatedAt.Should().BeSameDateAs(genre.CreatedAt);

        var dbContext = fixture.CreateDbContext();
        var dbGenre = await dbContext
            .Genres.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == genre.Id, CancellationToken.None);
        dbGenre.Should().NotBeNull();
        dbGenre!.Name.Should().Be(input.Name);
        dbGenre.IsActive.Should().Be(input.IsActive!.Value);
    }

    [Fact(DisplayName = nameof(UpdateGenreWithCategories))]
    [Trait("EndToEnd/Api", "UpdateGenre - Endpoints")]
    public async Task UpdateGenreWithCategories()
    {
        var genres = fixture.GetValidGenres(10);
        var genre = genres[5];
        var categories = fixture.GetValidCategories(10);
        var seededCategories = categories.GetRange(0, 5);
        seededCategories.ForEach(category => genre.AddCategory(category.Id));
        await fixture.Persistence.InsertCategoriesList(categories, CancellationToken.None);
        await fixture.Persistence.InsertList(genres, CancellationToken.None);
        await fixture.Persistence.InsertGenresCategoriesRelationsList(
            [.. seededCategories.Select(category => new GenresCategories(genre.Id, category.Id))],
            CancellationToken.None
        );
        var newCategoryIds = categories.GetRange(5, 5).Select(category => category.Id).ToList();
        var input = fixture.GetValidInput(newCategoryIds);

        var (response, output) = await fixture.ApiClient.Put<ApiOutput<UpdateGenreOutput>>(
            $"/genres/{genre.Id}",
            input,
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Data.Id.Should().Be(genre.Id);
        output.Data.Name.Should().Be(input.Name);
        output.Data.Categories.Should().BeEquivalentTo(newCategoryIds);

        var dbContext = fixture.CreateDbContext();
        var relations = await dbContext
            .GenresCategories.AsNoTracking()
            .Where(relation => relation.GenreId == genre.Id)
            .ToListAsync(CancellationToken.None);
        relations.Select(relation => relation.CategoryId).Should().BeEquivalentTo(newCategoryIds);
    }

    [Fact(DisplayName = nameof(ThrowWhenGenreNotFound))]
    [Trait("EndToEnd/Api", "UpdateGenre - Endpoints")]
    public async Task ThrowWhenGenreNotFound()
    {
        await fixture.Persistence.InsertList(fixture.GetValidGenres(10), CancellationToken.None);
        var invalidId = Guid.NewGuid();
        var input = fixture.GetValidInput();

        var (response, output) = await fixture.ApiClient.Put<ProblemDetails>(
            $"/genres/{invalidId}",
            input,
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        output.Should().NotBeNull();
        output!.Status.Should().Be(StatusCodes.Status404NotFound);
        output.Title.Should().Be("Not found");
        output.Detail.Should().Be($"Genre '{invalidId}' not found.");
        output.Type.Should().Be("NotFound");
    }

    [Fact(DisplayName = nameof(ThrowWhenRelatedCategoryNotFound))]
    [Trait("EndToEnd/Api", "UpdateGenre - Endpoints")]
    public async Task ThrowWhenRelatedCategoryNotFound()
    {
        var genres = fixture.GetValidGenres(10);
        var genre = genres[5];
        var categories = fixture.GetValidCategories(5);
        await fixture.Persistence.InsertCategoriesList(categories, CancellationToken.None);
        await fixture.Persistence.InsertList(genres, CancellationToken.None);
        var categoryIds = categories.Select(category => category.Id).ToList();
        var invalidCategoryId = Guid.NewGuid();
        categoryIds.Add(invalidCategoryId);
        var input = fixture.GetValidInput(categoryIds);

        var (response, output) = await fixture.ApiClient.Put<ProblemDetails>(
            $"/genres/{genre.Id}",
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
    [Trait("EndToEnd/Api", "UpdateGenre - Endpoints")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ThrowWhenNameIsInvalid(string invalidName)
    {
        var genres = fixture.GetValidGenres(10);
        var genre = genres[5];
        await fixture.Persistence.InsertList(genres, CancellationToken.None);
        var input = new UpdateGenreApiInput(invalidName);

        var (response, output) = await fixture.ApiClient.Put<ProblemDetails>(
            $"/genres/{genre.Id}",
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

    [Fact(DisplayName = nameof(KeepCategoriesWhenCategoriesIsNull))]
    [Trait("EndToEnd/Api", "UpdateGenre - Endpoints")]
    public async Task KeepCategoriesWhenCategoriesIsNull()
    {
        var genres = fixture.GetValidGenres(10);
        var genre = genres[5];
        var categories = fixture.GetValidCategories(5);
        categories.ForEach(category => genre.AddCategory(category.Id));
        await fixture.Persistence.InsertCategoriesList(categories, CancellationToken.None);
        await fixture.Persistence.InsertList(genres, CancellationToken.None);
        await fixture.Persistence.InsertGenresCategoriesRelationsList(
            [.. categories.Select(category => new GenresCategories(genre.Id, category.Id))],
            CancellationToken.None
        );
        var categoryIds = categories.Select(category => category.Id).ToList();
        var input = fixture.GetValidInput();

        var (response, output) = await fixture.ApiClient.Put<ApiOutput<UpdateGenreOutput>>(
            $"/genres/{genre.Id}",
            input,
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Data.Id.Should().Be(genre.Id);
        output.Data.Name.Should().Be(input.Name);
        output.Data.Categories.Should().BeEquivalentTo(categoryIds);

        var dbContext = fixture.CreateDbContext();
        var relations = await dbContext
            .GenresCategories.AsNoTracking()
            .Where(relation => relation.GenreId == genre.Id)
            .ToListAsync(CancellationToken.None);
        relations.Select(relation => relation.CategoryId).Should().BeEquivalentTo(categoryIds);
    }
}
