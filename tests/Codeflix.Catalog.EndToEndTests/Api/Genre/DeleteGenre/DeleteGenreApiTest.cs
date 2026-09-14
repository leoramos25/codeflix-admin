using System.Net;
using Codeflix.Catalog.Infra.Data.EF.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Codeflix.Catalog.EndToEndTests.Api.Genre.DeleteGenre;

[Collection(nameof(DeleteGenreApiTestFixture))]
public class DeleteGenreApiTest(DeleteGenreApiTestFixture fixture)
{
    [Fact(DisplayName = nameof(DeleteGenre))]
    [Trait("EndToEnd/Api", "DeleteGenre - Endpoints")]
    public async Task DeleteGenre()
    {
        var genres = fixture.GetValidGenres(10);
        var genre = genres[5];
        await fixture.Persistence.InsertList(genres, CancellationToken.None);

        var (response, output) = await fixture.ApiClient.Delete<object>(
            $"/genres/{genre.Id}",
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        output.Should().BeNull();

        var dbContext = fixture.CreateDbContext();
        var dbGenre = await dbContext.Genres.FindAsync([genre.Id], CancellationToken.None);
        dbGenre.Should().BeNull();
    }

    [Fact(DisplayName = nameof(DeleteGenreWithRelations))]
    [Trait("EndToEnd/Api", "DeleteGenre - Endpoints")]
    public async Task DeleteGenreWithRelations()
    {
        var genres = fixture.GetValidGenres(10);
        var genre = genres[5];
        var categories = fixture.GetValidCategories(5);
        categories.ForEach(category => genre.AddCategory(category.Id));
        await fixture.CategoryPersistence.InsertList(categories, CancellationToken.None);
        await fixture.Persistence.InsertList(genres, CancellationToken.None);
        await fixture.Persistence.InsertGenresCategoriesRelationsList(
            [.. categories.Select(category => new GenresCategories(genre.Id, category.Id))],
            CancellationToken.None
        );

        var (response, output) = await fixture.ApiClient.Delete<object>(
            $"/genres/{genre.Id}",
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        output.Should().BeNull();

        var dbContext = fixture.CreateDbContext();
        var dbGenre = await dbContext.Genres.FindAsync([genre.Id], CancellationToken.None);
        dbGenre.Should().BeNull();
        var relations = await dbContext
            .GenresCategories.AsNoTracking()
            .Where(relation => relation.GenreId == genre.Id)
            .ToListAsync(CancellationToken.None);
        relations.Should().BeEmpty();
    }

    [Fact(DisplayName = nameof(ThrowWhenGenreNotFound))]
    [Trait("EndToEnd/Api", "DeleteGenre - Endpoints")]
    public async Task ThrowWhenGenreNotFound()
    {
        var genres = fixture.GetValidGenres(10);
        await fixture.Persistence.InsertList(genres, CancellationToken.None);
        var invalidId = Guid.NewGuid();

        var (response, output) = await fixture.ApiClient.Delete<ProblemDetails>(
            $"/genres/{invalidId}",
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        output.Should().NotBeNull();
        output!.Status.Should().Be((int)HttpStatusCode.NotFound);
        output.Title.Should().Be("Not found");
        output.Detail.Should().Be($"Genre '{invalidId}' not found.");
        output.Type.Should().Be("NotFound");
    }
}
