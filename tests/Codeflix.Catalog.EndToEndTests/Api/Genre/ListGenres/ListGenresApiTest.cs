using System.Net;
using Codeflix.Catalog.Application.UseCases.Genre.List;
using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using Codeflix.Catalog.EndToEndTests.Models;
using Codeflix.Catalog.Infra.Data.EF.Models;
using FluentAssertions;

namespace Codeflix.Catalog.EndToEndTests.Api.Genre.ListGenres;

[Collection(nameof(ListGenresApiTestFixture))]
public class ListGenresApiTest(ListGenresApiTestFixture fixture) : IDisposable
{
    public void Dispose()
    {
        fixture.CleanPersistence();
    }

    [Fact(DisplayName = nameof(ListGenresAndTotalWithDefault))]
    [Trait("EndToEnd/Api", "ListGenres - Endpoints")]
    public async Task ListGenresAndTotalWithDefault()
    {
        const int defaultPerPage = 15;
        var genres = fixture.GetValidGenres(20);
        await fixture.Persistence.InsertList(genres, CancellationToken.None);

        var (response, output) = await fixture.ApiClient.Get<
            TestApiResponseList<ListGenresItemOutput>
        >("/genres", CancellationToken.None);

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Data.Should().HaveCount(defaultPerPage);
        output.Meta!.Total.Should().Be(genres.Count);
        output.Meta.CurrentPage.Should().Be(1);
        output.Meta.PerPage.Should().Be(defaultPerPage);
        foreach (var item in output.Data!)
        {
            var expectedItem = genres.Find(genre => genre.Id == item.Id);
            expectedItem.Should().NotBeNull();
            item.Name.Should().Be(expectedItem!.Name);
            item.IsActive.Should().Be(expectedItem.IsActive);
            item.CreatedAt.Should().BeSameDateAs(expectedItem.CreatedAt);
        }
    }

    [Theory(DisplayName = nameof(ListPaginated))]
    [Trait("EndToEnd/Api", "ListGenres - Endpoints")]
    [InlineData(10, 1, 5, 5)]
    [InlineData(10, 2, 5, 5)]
    [InlineData(7, 2, 5, 2)]
    [InlineData(7, 3, 5, 0)]
    public async Task ListPaginated(
        int quantityGenresToGenerate,
        int page,
        int perPage,
        int expectedQuantityItem
    )
    {
        var input = new ListGenresInput(page, perPage, "", "", SearchOrder.Asc);
        var genres = fixture.GetValidGenres(quantityGenresToGenerate);
        await fixture.Persistence.InsertList(genres, CancellationToken.None);

        var (response, output) = await fixture.ApiClient.Get<
            TestApiResponseList<ListGenresItemOutput>
        >("/genres", CancellationToken.None, input);

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Data.Should().HaveCount(expectedQuantityItem);
        output.Meta!.Total.Should().Be(genres.Count);
        output.Meta.CurrentPage.Should().Be(input.Page);
        output.Meta.PerPage.Should().Be(input.PerPage);
    }

    [Fact(DisplayName = nameof(ListGenresWhenPersistenceEmpty))]
    [Trait("EndToEnd/Api", "ListGenres - Endpoints")]
    public async Task ListGenresWhenPersistenceEmpty()
    {
        var (response, output) = await fixture.ApiClient.Get<
            TestApiResponseList<ListGenresItemOutput>
        >("/genres", CancellationToken.None);

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Data.Should().BeEmpty();
        output.Meta!.Total.Should().Be(0);
    }

    [Fact(DisplayName = nameof(ListGenresWithRelations))]
    [Trait("EndToEnd/Api", "ListGenres - Endpoints")]
    public async Task ListGenresWithRelations()
    {
        var genres = fixture.GetValidGenres(10);
        var categories = fixture.GetValidCategories(10);
        var random = new Random();
        genres.ForEach(genre =>
        {
            var relationsCount = random.Next(2, categories.Count - 1);
            for (var index = 0; index < relationsCount; index++)
            {
                var category = categories[random.Next(0, categories.Count - 1)];
                if (!genre.Categories.Contains(category.Id))
                    genre.AddCategory(category.Id);
            }
        });
        var relations = genres
            .SelectMany(genre =>
                genre.Categories.Select(categoryId => new GenresCategories(genre.Id, categoryId))
            )
            .ToList();
        await fixture.CategoryPersistence.InsertList(categories, CancellationToken.None);
        await fixture.Persistence.InsertList(genres, CancellationToken.None);
        await fixture.Persistence.InsertGenresCategoriesRelationsList(
            relations,
            CancellationToken.None
        );

        var (response, output) = await fixture.ApiClient.Get<
            TestApiResponseList<ListGenresItemOutput>
        >("/genres", CancellationToken.None);

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Data.Should().HaveCount(genres.Count);
        output.Meta!.Total.Should().Be(genres.Count);
        foreach (var item in output.Data!)
        {
            var expectedItem = genres.Find(genre => genre.Id == item.Id);
            expectedItem.Should().NotBeNull();
            item.Name.Should().Be(expectedItem!.Name);
            item.IsActive.Should().Be(expectedItem.IsActive);
            item.CreatedAt.Should().BeSameDateAs(expectedItem.CreatedAt);
            item.Categories.Select(category => category.Id)
                .Should()
                .BeEquivalentTo(expectedItem.Categories);
            foreach (var categoryOutput in item.Categories)
            {
                var expectedCategory = categories.Find(category =>
                    category.Id == categoryOutput.Id
                );
                expectedCategory.Should().NotBeNull();
                categoryOutput.Name.Should().Be(expectedCategory!.Name);
            }
        }
    }

    [Theory(DisplayName = nameof(ListByText))]
    [Trait("EndToEnd/Api", "ListGenres - Endpoints")]
    [InlineData("Action", 1, 5, 1, 1)]
    [InlineData("Horror", 1, 5, 3, 3)]
    [InlineData("Horror", 2, 5, 0, 3)]
    [InlineData("Sci-Fi", 1, 5, 4, 4)]
    [InlineData("Sci-Fi", 1, 2, 2, 4)]
    [InlineData("Sci-Fi", 2, 3, 1, 4)]
    [InlineData("Other", 1, 3, 0, 0)]
    [InlineData("Robot", 1, 5, 2, 2)]
    public async Task ListByText(
        string search,
        int page,
        int perPage,
        int expectedQuantityReturn,
        int expectedTotalItems
    )
    {
        var input = new ListGenresInput(page, perPage, search, "", SearchOrder.Asc);
        var genres = fixture.GetValidGenresWithNames([
            "Action",
            "Horror",
            "Horror - Robots",
            "Horror - Based On Real Facts",
            "Drama",
            "Sci-Fi IA",
            "Sci-Fi Robots",
            "Sci-Fi Space",
            "Sci-Fi Future",
        ]);
        await fixture.Persistence.InsertList(genres, CancellationToken.None);

        var (response, output) = await fixture.ApiClient.Get<
            TestApiResponseList<ListGenresItemOutput>
        >("/genres", CancellationToken.None, input);

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Data.Should().HaveCount(expectedQuantityReturn);
        output.Meta!.Total.Should().Be(expectedTotalItems);
        output.Meta.CurrentPage.Should().Be(input.Page);
        output.Meta.PerPage.Should().Be(input.PerPage);
        foreach (var item in output.Data!)
        {
            var expectedItem = genres.Find(genre => genre.Id == item.Id);
            expectedItem.Should().NotBeNull();
            item.Name.Should().Be(expectedItem!.Name);
            item.IsActive.Should().Be(expectedItem.IsActive);
            item.CreatedAt.Should().BeSameDateAs(expectedItem.CreatedAt);
        }
    }

    [Theory(DisplayName = nameof(ListOrdered))]
    [Trait("EndToEnd/Api", "ListGenres - Endpoints")]
    [InlineData("name", "asc")]
    [InlineData("name", "desc")]
    [InlineData("id", "asc")]
    [InlineData("id", "desc")]
    [InlineData("createdAt", "asc")]
    [InlineData("createdAt", "desc")]
    public async Task ListOrdered(string orderBy, string order)
    {
        var searchOrder = order.Equals("asc", StringComparison.CurrentCultureIgnoreCase)
            ? SearchOrder.Asc
            : SearchOrder.Desc;
        var input = new ListGenresInput(1, 20, "", orderBy, searchOrder);
        var genres = fixture.GetValidGenresWithNames([
            "Action",
            "Horror",
            "Drama",
            "Comedy",
            "Sci-Fi",
            "Romance",
            "Thriller",
            "Western",
            "Fantasy",
            "Documentary",
        ]);
        await fixture.Persistence.InsertList(genres, CancellationToken.None);

        var (response, output) = await fixture.ApiClient.Get<
            TestApiResponseList<ListGenresItemOutput>
        >("/genres", CancellationToken.None, input);

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Meta!.Total.Should().Be(genres.Count);
        output.Meta.CurrentPage.Should().Be(input.Page);
        output.Meta.PerPage.Should().Be(input.PerPage);
        var expectedList = fixture.GetOrderedGenres(genres, input.Sort, input.Dir);
        for (var index = 0; index < expectedList.Count; index++)
        {
            var expectedItem = expectedList[index];
            var outputItem = output.Data![index];
            outputItem.Should().NotBeNull();
            outputItem.Id.Should().Be(expectedItem.Id);
            outputItem.Name.Should().Be(expectedItem.Name);
            outputItem.IsActive.Should().Be(expectedItem.IsActive);
            outputItem.CreatedAt.Should().BeSameDateAs(expectedItem.CreatedAt);
        }
    }
}
