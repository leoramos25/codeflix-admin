using Codeflix.Catalog.Domain.Entity;
using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using Codeflix.Catalog.IntegrationTests.Common;

namespace Codeflix.Catalog.IntegrationTests.Infra.Data.EF.Repositories.CategoryRepository;

[CollectionDefinition(nameof(CategoryRepositoryTestFixture))]
public class CategoryRepositoryTestFixtureCollection
    : ICollectionFixture<CategoryRepositoryTestFixture>;

public class CategoryRepositoryTestFixture : BaseFixture
{
    private const int CategoryNameMinLength = 2;
    private const int CategoryNameMaxLength = 255;
    private const int CategoryDescriptionMaxLength = 10_000;

    public List<Category> GetValidCategoriesWithNames(List<string> categoryNames) =>
        categoryNames
            .Select(name =>
            {
                var category = GetValidCategory();
                category.Update(name);
                return category;
            })
            .ToList();

    public List<Category> GetOrderedCategories(
        List<Category> categories,
        string orderBy,
        SearchOrder order
    )
    {
        var listClone = new List<Category>(categories);
        var orderedEnumerable = (orderBy.ToLower(), order) switch
        {
            ("name", SearchOrder.Asc) => listClone.OrderBy(x => x.Name),
            ("name", SearchOrder.Desc) => listClone.OrderByDescending(x => x.Name),
            ("id", SearchOrder.Asc) => listClone.OrderBy(x => x.Id),
            ("id", SearchOrder.Desc) => listClone.OrderByDescending(x => x.Id),
            ("createdat", SearchOrder.Asc) => listClone.OrderBy(x => x.CreatedAt),
            ("createdat", SearchOrder.Desc) => listClone.OrderByDescending(x => x.CreatedAt),
            _ => listClone.OrderBy(x => x.Name),
        };
        return orderedEnumerable.ToList();
    }

    public List<Category> GetValidCategories(int size = 10) =>
        Enumerable.Range(0, size).Select(_ => GetValidCategory()).ToList();

    public Category GetValidCategory() =>
        new(GetValidCategoryName(), GetValidCategoryDescription(), GetRandomBoolean());

    public string GetValidCategoryName()
    {
        var categoryName = string.Empty;
        while (categoryName.Length < CategoryNameMinLength)
            categoryName = Faker.Commerce.Categories(1)[0];
        if (categoryName.Length > CategoryNameMaxLength)
            categoryName = categoryName[..CategoryNameMaxLength];
        return categoryName;
    }

    public string GetValidCategoryDescription()
    {
        var categoryDescription = Faker.Commerce.ProductDescription();
        if (categoryDescription.Length > CategoryDescriptionMaxLength)
            categoryDescription = categoryDescription[..CategoryDescriptionMaxLength];
        return categoryDescription;
    }

    public bool GetRandomBoolean() => new Random().NextDouble() < 0.5;
}
