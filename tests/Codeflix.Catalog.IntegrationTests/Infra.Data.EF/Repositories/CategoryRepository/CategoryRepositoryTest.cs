using Codeflix.Catalog.Application.Exceptions;
using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using FluentAssertions;
using Context = Codeflix.Catalog.Infra.Data.EF.Repositories;

namespace Codeflix.Catalog.IntegrationTests.Infra.Data.EF.Repositories.CategoryRepository;

[Collection(nameof(CategoryRepositoryTestFixture))]
public class CategoryRepositoryTest(CategoryRepositoryTestFixture fixture)
{
    [Fact(DisplayName = nameof(Insert))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
    public async Task Insert()
    {
        var dbContext = fixture.CreateDbContext();
        var category = fixture.GetValidCategory();
        var categoryRepository = new Context.CategoryRepository(dbContext);

        await categoryRepository.Insert(category, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var persistedCategory = await fixture
            .CreateDbContext(true)
            .Categories.FindAsync(category.Id);

        persistedCategory.Should().NotBeNull();
        persistedCategory.Id.Should().Be(category.Id);
        persistedCategory.Name.Should().Be(category.Name);
        persistedCategory.Description.Should().Be(category.Description);
        persistedCategory.IsActive.Should().Be(category.IsActive);
        persistedCategory.CreatedAt.Should().BeSameDateAs(category.CreatedAt);
    }

    [Fact(DisplayName = nameof(Get))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
    public async Task Get()
    {
        var dbContext = fixture.CreateDbContext();
        var category = fixture.GetValidCategory();
        var categories = fixture.GetValidCategories();
        categories.Add(category);
        await dbContext.Categories.AddRangeAsync(categories, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var categoryRepository = new Context.CategoryRepository(fixture.CreateDbContext(true));

        var persistedCategory = await categoryRepository.Get(category.Id, CancellationToken.None);

        persistedCategory.Should().NotBeNull();
        persistedCategory.Id.Should().Be(category.Id);
        persistedCategory.Name.Should().Be(category.Name);
        persistedCategory.Description.Should().Be(category.Description);
        persistedCategory.IsActive.Should().Be(category.IsActive);
        persistedCategory.CreatedAt.Should().BeSameDateAs(category.CreatedAt);
    }

    [Fact(DisplayName = nameof(GetThrowWhenIfNotFound))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
    public async Task GetThrowWhenIfNotFound()
    {
        var dbContext = fixture.CreateDbContext();
        var invalidId = Guid.NewGuid();
        var categoryRepository = new Context.CategoryRepository(dbContext);

        var action = () => categoryRepository.Get(invalidId, CancellationToken.None);

        await action
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"Category '{invalidId}' not found.");
    }

    [Fact(DisplayName = nameof(Update))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
    public async Task Update()
    {
        var dbContext = fixture.CreateDbContext();
        var category = fixture.GetValidCategory();
        var categoryRepository = new Context.CategoryRepository(dbContext);
        await dbContext.AddAsync(category, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var updateCategoryData = new
        {
            Name = fixture.GetValidCategoryName(),
            Description = fixture.GetValidCategoryDescription(),
            IsActive = fixture.GetRandomBoolean(),
        };

        category.Update(updateCategoryData.Name, updateCategoryData.Description);

        if (updateCategoryData.IsActive)
            category.Activate();
        else
            category.Deactivate();

        await categoryRepository.Update(category, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var updatedCategory = await fixture
            .CreateDbContext(true)
            .Categories.FindAsync(category.Id, CancellationToken.None);

        updatedCategory.Should().NotBeNull();
        updatedCategory.Id.Should().Be(category.Id);
        updatedCategory.Name.Should().Be(updateCategoryData.Name);
        updatedCategory.Description.Should().Be(updateCategoryData.Description);
        updatedCategory.IsActive.Should().Be(updateCategoryData.IsActive);
        updatedCategory.CreatedAt.Should().BeSameDateAs(category.CreatedAt);
    }

    [Fact(DisplayName = nameof(Delete))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
    public async Task Delete()
    {
        var dbContext = fixture.CreateDbContext();
        var category = fixture.GetValidCategory();
        var categoryRepository = new Context.CategoryRepository(dbContext);
        await dbContext.AddAsync(category, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        await categoryRepository.Delete(category, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var deletedCategory = await fixture
            .CreateDbContext(true)
            .Categories.FindAsync(category.Id, CancellationToken.None);

        deletedCategory.Should().BeNull();
    }

    [Fact(DisplayName = nameof(Search))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
    public async Task Search()
    {
        var dbContext = fixture.CreateDbContext();
        var categories = fixture.GetValidCategories();
        await dbContext.Categories.AddRangeAsync(categories, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var categoryRepository = new Context.CategoryRepository(fixture.CreateDbContext(true));
        var searchInput = new SearchInput(1, 20, "", "", SearchOrder.Asc);

        var output = await categoryRepository.Search(searchInput, CancellationToken.None);

        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.CurrentPage.Should().Be(searchInput.Page);
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
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
    public async Task SearchReturnsEmpty()
    {
        var categoryRepository = new Context.CategoryRepository(fixture.CreateDbContext());
        var searchInput = new SearchInput(1, 20, "", "", SearchOrder.Asc);

        var output = await categoryRepository.Search(searchInput, CancellationToken.None);

        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.Items.Should().BeEmpty();
        output.Total.Should().Be(0);
        output.CurrentPage.Should().Be(searchInput.Page);
    }

    [Theory(DisplayName = nameof(SearchReturnsPaginated))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
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
        var categoryRepository = new Context.CategoryRepository(fixture.CreateDbContext(true));
        var searchInput = new SearchInput(page, perPage, "", "", SearchOrder.Asc);

        var output = await categoryRepository.Search(searchInput, CancellationToken.None);

        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.CurrentPage.Should().Be(page);
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
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
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
        var categoryRepository = new Context.CategoryRepository(fixture.CreateDbContext(true));
        var searchInput = new SearchInput(page, perPage, search, "", SearchOrder.Asc);

        var output = await categoryRepository.Search(searchInput, CancellationToken.None);

        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.CurrentPage.Should().Be(page);
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

    [Theory(DisplayName = nameof(SearchByText))]
    [Trait("Integration/Infra.Data", "CategoryRepository - Repositories")]
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
        var categoryRepository = new Context.CategoryRepository(fixture.CreateDbContext(true));
        var searchOrder = order.Equals("asc", StringComparison.CurrentCultureIgnoreCase)
            ? SearchOrder.Asc
            : SearchOrder.Desc;
        var searchInput = new SearchInput(1, 20, "", orderBy, searchOrder);

        var output = await categoryRepository.Search(searchInput, CancellationToken.None);

        var expectedList = fixture.GetOrderedCategories(categories, orderBy, searchOrder);
        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.CurrentPage.Should().Be(searchInput.Page);
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
