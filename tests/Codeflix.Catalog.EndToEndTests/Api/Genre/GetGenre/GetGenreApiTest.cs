using System.Net;
using Codeflix.Catalog.Api.ApiModels;
using Codeflix.Catalog.Application.UseCases.Genre.Get;
using Codeflix.Catalog.Infra.Data.EF.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace Codeflix.Catalog.EndToEndTests.Api.Genre.GetGenre;

[Collection(nameof(GetGenreApiTestFixture))]
public class GetGenreApiTest(GetGenreApiTestFixture fixture)
{
    [Fact(DisplayName = nameof(GetGenre))]
    [Trait("EndToEnd/Api", "GetGenre - Endpoints")]
    public async Task GetGenre()
    {
        var genres = fixture.GetValidGenres(10);
        var genre = genres[5];
        await fixture.Persistence.InsertList(genres, CancellationToken.None);

        var (response, output) = await fixture.ApiClient.Get<ApiOutput<GetGenreOutput>>(
            $"/genres/{genre.Id}",
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Data.Id.Should().Be(genre.Id);
        output.Data.Name.Should().Be(genre.Name);
        output.Data.IsActive.Should().Be(genre.IsActive);
        output.Data.CreatedAt.Should().BeSameDateAs(genre.CreatedAt);
        output.Data.Categories.Should().BeEmpty();
    }

    [Fact(DisplayName = nameof(GetGenreWithCategories))]
    [Trait("EndToEnd/Api", "GetGenre - Endpoints")]
    public async Task GetGenreWithCategories()
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

        var (response, output) = await fixture.ApiClient.Get<ApiOutput<GetGenreOutput>>(
            $"/genres/{genre.Id}",
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Data.Id.Should().Be(genre.Id);
        output.Data.Name.Should().Be(genre.Name);
        output.Data.IsActive.Should().Be(genre.IsActive);
        output.Data.CreatedAt.Should().BeSameDateAs(genre.CreatedAt);
        output.Data.Categories.Should().HaveCount(categories.Count);
        output
            .Data.Categories.Select(relation => relation.Id)
            .Should()
            .BeEquivalentTo(categories.Select(category => category.Id));
        output
            .Data.Categories.ToList()
            .ForEach(categoryOutput =>
            {
                var category = categories.Find(category => category.Id == categoryOutput.Id);
                category.Should().NotBeNull();
                categoryOutput.Name.Should().Be(category.Name);
            });
    }

    [Fact(DisplayName = nameof(ThrowWhenGenreNotFound))]
    [Trait("EndToEnd/Api", "GetGenre - Endpoints")]
    public async Task ThrowWhenGenreNotFound()
    {
        var genres = fixture.GetValidGenres(10);
        await fixture.Persistence.InsertList(genres, CancellationToken.None);
        var invalidId = Guid.NewGuid();

        var (response, output) = await fixture.ApiClient.Get<ProblemDetails>(
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
