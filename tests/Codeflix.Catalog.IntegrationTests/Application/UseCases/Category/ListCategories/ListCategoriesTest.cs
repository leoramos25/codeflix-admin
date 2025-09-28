using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using Codeflix.Catalog.Infra.Data.EF.Repositories;
using FluentAssertions;
using UseCase = Codeflix.Catalog.Application.UseCases.Category.List;

namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.Category.ListCategories;

[Collection(nameof(ListCategoriesTestFixture))]
public class ListCategoriesTest(ListCategoriesTestFixture fixture)
{
    [Fact(DisplayName = nameof(Search))]
    [Trait("Integration/Application", "ListCategories - Use Cases")]
    public async Task Search()
    {
        var dbContext = fixture.CreateDbContext();
        var categories = fixture.GetValidCategories();
        await dbContext.Categories.AddRangeAsync(categories, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var categoryRepository = new CategoryRepository(fixture.CreateDbContext(true));
        var input = new UseCase.ListCategoriesInput(1, 20);
        var useCase = new UseCase.ListCategories(categoryRepository);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.Page.Should().Be(input.Page);
        output.Total.Should().Be(categories.Count);
        foreach (var item in output.Items)
        {
            var category = categories.Find(c => c.Id == item.Id);
            category.Should().NotBeNull();
            item.Name.Should().Be(category.Name);
            item.Description.Should().Be(category.Description);
            item.IsActive.Should().Be(category.IsActive);
            item.CreatedAt.Should().BeSameDateAs(category.CreatedAt);
        }
    }

    [Fact(DisplayName = nameof(SearchReturnsEmpty))]
    [Trait("Integration/Application", "ListCategories - Use Cases")]
    public async Task SearchReturnsEmpty()
    {
        var categoryRepository = new CategoryRepository(fixture.CreateDbContext());
        var input = new UseCase.ListCategoriesInput(1, 20);
        var useCase = new UseCase.ListCategories(categoryRepository);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.Items.Should().BeEmpty();
        output.Total.Should().Be(0);
        output.Page.Should().Be(input.Page);
    }

    [Theory(DisplayName = nameof(SearchReturnsPaginated))]
    [Trait("Integration/Application", "ListCategories - Use Cases")]
    [InlineData(10, 1, 5, 5)]
    [InlineData(10, 2, 5, 5)]
    [InlineData(7, 2, 5, 2)]
    [InlineData(7, 3, 5, 0)]
    public async Task SearchReturnsPaginated(
        int categoriesQuantity,
        int page,
        int perPage,
        int expectedItems
    )
    {
        var dbContext = fixture.CreateDbContext();
        var categories = fixture.GetValidCategories(categoriesQuantity);
        await dbContext.Categories.AddRangeAsync(categories, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var categoryRepository = new CategoryRepository(fixture.CreateDbContext(true));
        var input = new UseCase.ListCategoriesInput(page, perPage);
        var useCase = new UseCase.ListCategories(categoryRepository);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.Page.Should().Be(page);
        output.Total.Should().Be(categoriesQuantity);
        output.Items.Should().HaveCount(expectedItems);
        foreach (var item in output.Items)
        {
            var category = categories.Find(c => c.Id == item.Id);
            category.Should().NotBeNull();
            item.Name.Should().Be(category.Name);
            item.Description.Should().Be(category.Description);
            item.IsActive.Should().Be(category.IsActive);
            item.CreatedAt.Should().BeSameDateAs(category.CreatedAt);
        }
    }

    [Theory(DisplayName = nameof(SearchByText))]
    [Trait("Integration/Application", "ListCategories - Use Cases")]
    [InlineData("Action", 1, 5, 1, 1)]
    [InlineData("Horror", 1, 5, 3, 3)]
    [InlineData("Horror", 2, 5, 0, 3)]
    [InlineData("Sci-Fi", 1, 5, 4, 4)]
    [InlineData("Sci-Fi", 1, 2, 2, 4)]
    [InlineData("Sci-Fi", 2, 2, 2, 4)]
    [InlineData("Sci-Fi", 2, 3, 1, 4)]
    [InlineData("Other", 1, 3, 0, 0)]
    [InlineData("Robot", 1, 5, 2, 2)]
    public async Task SearchByText(
        string search,
        int page,
        int perPage,
        int expectedQuantityReturn,
        int expectTotalItems
    )
    {
        var dbContext = fixture.CreateDbContext();
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
        await dbContext.Categories.AddRangeAsync(categories, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var categoryRepository = new CategoryRepository(fixture.CreateDbContext(true));
        var input = new UseCase.ListCategoriesInput(page, perPage, search);
        var useCase = new UseCase.ListCategories(categoryRepository);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.Page.Should().Be(page);
        output.Total.Should().Be(expectTotalItems);
        output.Items.Should().HaveCount(expectedQuantityReturn);
        foreach (var item in output.Items)
        {
            var category = categories.Find(c => c.Id == item.Id);
            category.Should().NotBeNull();
            item.Name.Should().Be(category.Name);
            item.Description.Should().Be(category.Description);
            item.IsActive.Should().Be(category.IsActive);
            item.CreatedAt.Should().BeSameDateAs(category.CreatedAt);
        }
    }

    [Theory(DisplayName = nameof(SearchOrdered))]
    [Trait("Integration/Application", "ListCategories - Use Cases")]
    [InlineData("name", "asc")]
    [InlineData("name", "desc")]
    [InlineData("id", "asc")]
    [InlineData("id", "desc")]
    [InlineData("createdAt", "asc")]
    [InlineData("createdAt", "desc")]
    public async Task SearchOrdered(string orderBy, string order)
    {
        var dbContext = fixture.CreateDbContext();
        var categories = fixture.GetValidCategories();
        await dbContext.Categories.AddRangeAsync(categories, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var categoryRepository = new CategoryRepository(fixture.CreateDbContext(true));
        var searchOrder = order.Equals("asc", StringComparison.CurrentCultureIgnoreCase)
            ? SearchOrder.Asc
            : SearchOrder.Desc;
        var searchInput = new UseCase.ListCategoriesInput(1, 20, "", orderBy, searchOrder);
        var useCase = new UseCase.ListCategories(categoryRepository);

        var output = await useCase.Handle(searchInput, CancellationToken.None);

        var expectedList = fixture.GetOrderedCategories(categories, orderBy, searchOrder);
        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.Page.Should().Be(searchInput.Page);
        output.Total.Should().Be(categories.Count);
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
}
