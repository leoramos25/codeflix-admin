using System.Net;
using Codeflix.Catalog.Application.UseCases.Category.List;
using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using FluentAssertions;

namespace Codeflix.Catalog.EndToEndTests.Api.Category.ListCategories;

[Collection(nameof(ListCategoriesApiTestFixture))]
public class ListCategoriesApiTest(ListCategoriesApiTestFixture fixture) : IDisposable
{
    [Fact(DisplayName = nameof(ListCategoriesAndTotalWithDefault))]
    [Trait("EndToEnd/Api", "Category/List - Endpoints")]
    public async Task ListCategoriesAndTotalWithDefault()
    {
        const int defaultPerPage = 15;
        var categories = fixture.GetValidCategories(20);
        await fixture.Persistence.InsertList(categories, CancellationToken.None);

        var (response, output) = await fixture.ApiClient.Get<ListCategoriesOutput>(
            $"/categories",
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output.Items.Should().HaveCount(defaultPerPage);
        output.Total.Should().Be(categories.Count);
        output.Page.Should().Be(1);
        output.PerPage.Should().Be(defaultPerPage);
        foreach (var item in output.Items)
        {
            var expectedItem = categories.FirstOrDefault(x => x.Id == item.Id);
            expectedItem.Should().NotBeNull();
            item.Should().NotBeNull();
            item.Name.Should().Be(expectedItem.Name);
            item.Description.Should().Be(expectedItem.Description);
            item.IsActive.Should().Be(expectedItem.IsActive);
            item.CreatedAt.Should().BeSameDateAs(expectedItem.CreatedAt);
        }
    }

    [Fact(DisplayName = nameof(ListCategoriesWhenPersistenceEmpty))]
    [Trait("EndToEnd/Api", "Category/List - Endpoints")]
    public async Task ListCategoriesWhenPersistenceEmpty()
    {
        var (response, output) = await fixture.ApiClient.Get<ListCategoriesOutput>(
            $"/categories",
            CancellationToken.None
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output.Items.Should().HaveCount(0);
        output.Total.Should().Be(0);
    }

    [Fact(DisplayName = nameof(ListCategoriesAndTotal))]
    [Trait("EndToEnd/Api", "Category/List - Endpoints")]
    public async Task ListCategoriesAndTotal()
    {
        var input = new ListCategoriesInput(page: 1, perPage: 10);
        var categories = fixture.GetValidCategories(20);
        await fixture.Persistence.InsertList(categories, CancellationToken.None);

        var (response, output) = await fixture.ApiClient.Get<ListCategoriesOutput>(
            $"/categories",
            CancellationToken.None,
            input
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output.Items.Should().HaveCount(input.PerPage);
        output.Total.Should().Be(categories.Count);
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        foreach (var item in output.Items)
        {
            var expectedItem = categories.FirstOrDefault(x => x.Id == item.Id);
            expectedItem.Should().NotBeNull();
            item.Should().NotBeNull();
            item.Name.Should().Be(expectedItem.Name);
            item.Description.Should().Be(expectedItem.Description);
            item.IsActive.Should().Be(expectedItem.IsActive);
            item.CreatedAt.Should().BeSameDateAs(expectedItem.CreatedAt);
        }
    }

    [Theory(DisplayName = nameof(ListPaginated))]
    [Trait("EndToEnd/Api", "Category/List - Endpoints")]
    [InlineData(10, 1, 5, 5)]
    [InlineData(10, 2, 5, 5)]
    [InlineData(7, 2, 5, 2)]
    [InlineData(7, 3, 5, 0)]
    public async Task ListPaginated(
        int quantityCategoriesToGenerate,
        int page,
        int perPage,
        int expectedQuantityItem
    )
    {
        var input = new ListCategoriesInput(page: page, perPage: perPage);
        var categories = fixture.GetValidCategories(quantityCategoriesToGenerate);
        await fixture.Persistence.InsertList(categories, CancellationToken.None);

        var (response, output) = await fixture.ApiClient.Get<ListCategoriesOutput>(
            $"/categories",
            CancellationToken.None,
            input
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output.Items.Should().HaveCount(expectedQuantityItem);
        output.Total.Should().Be(categories.Count);
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        foreach (var item in output.Items)
        {
            var expectedItem = categories.FirstOrDefault(x => x.Id == item.Id);
            expectedItem.Should().NotBeNull();
            item.Should().NotBeNull();
            item.Name.Should().Be(expectedItem.Name);
            item.Description.Should().Be(expectedItem.Description);
            item.IsActive.Should().Be(expectedItem.IsActive);
            item.CreatedAt.Should().BeSameDateAs(expectedItem.CreatedAt);
        }
    }

    [Theory(DisplayName = nameof(ListByText))]
    [Trait("EndToEnd/Api", "Category/List - Endpoints")]
    [InlineData("Action", 1, 5, 1, 1)]
    [InlineData("Horror", 1, 5, 3, 3)]
    [InlineData("Horror", 2, 5, 0, 3)]
    [InlineData("Sci-Fi", 1, 5, 4, 4)]
    [InlineData("Sci-Fi", 1, 2, 2, 4)]
    [InlineData("Sci-Fi", 2, 2, 2, 4)]
    [InlineData("Sci-Fi", 2, 3, 1, 4)]
    [InlineData("Other", 1, 3, 0, 0)]
    [InlineData("Robot", 1, 5, 2, 2)]
    public async Task ListByText(
        string search,
        int page,
        int perPage,
        int expectedQuantityReturn,
        int expectTotalItems
    )
    {
        var input = new ListCategoriesInput(page: page, perPage: perPage, search: search);
        var categories = fixture.GetValidCategoriesWithNames(
            [
                "Action",
                "Horror",
                "Horror - Robots",
                "Horror - Based On Real Facts",
                "Drama",
                "Sci-Fi IA",
                "Sci-Fi Robots",
                "Sci-Fi Space",
                "Sci-Fi Future",
            ]
        );
        await fixture.Persistence.InsertList(categories, CancellationToken.None);

        var (response, output) = await fixture.ApiClient.Get<ListCategoriesOutput>(
            $"/categories",
            CancellationToken.None,
            input
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output.Items.Should().HaveCount(expectedQuantityReturn);
        output.Total.Should().Be(expectTotalItems);
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        foreach (var item in output.Items)
        {
            var expectedItem = categories.FirstOrDefault(x => x.Id == item.Id);
            expectedItem.Should().NotBeNull();
            item.Should().NotBeNull();
            item.Name.Should().Be(expectedItem.Name);
            item.Description.Should().Be(expectedItem.Description);
            item.IsActive.Should().Be(expectedItem.IsActive);
            item.CreatedAt.Should().BeSameDateAs(expectedItem.CreatedAt);
        }
    }

    [Theory(DisplayName = nameof(ListOrdered))]
    [Trait("EndToEnd/Api", "Category/List - Endpoints")]
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
        var input = new ListCategoriesInput(
            page: 1,
            perPage: 20,
            search: "",
            sort: orderBy,
            dir: searchOrder
        );
        var categories = fixture.GetValidCategories();
        await fixture.Persistence.InsertList(categories, CancellationToken.None);

        var (response, output) = await fixture.ApiClient.Get<ListCategoriesOutput>(
            $"/categories",
            CancellationToken.None,
            input
        );

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output.Total.Should().Be(categories.Count);
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        var expectedList = fixture.GetOrderedCategories(categories, input.Sort, input.Dir);
        for (var index = 0; index < expectedList.Count; index++)
        {
            var expectedItem = expectedList[index];
            var outputItem = output.Items[index];
            expectedItem.Should().NotBeNull();
            outputItem.Should().NotBeNull();
            outputItem.Id.Should().Be(expectedItem.Id);
            outputItem.Name.Should().Be(expectedItem.Name);
            outputItem.Description.Should().Be(expectedItem.Description);
            outputItem.IsActive.Should().Be(expectedItem.IsActive);
            outputItem.CreatedAt.Should().BeSameDateAs(expectedItem.CreatedAt);
        }
    }

    public void Dispose() => fixture.CreateDbContext();
}
