using Codeflix.Catalog.Domain.Entity;
using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using Codeflix.Catalog.IntegrationTests.Common;

namespace Codeflix.Catalog.IntegrationTests.Infra.Data.EF.Repositories.CategoryRepository;

[CollectionDefinition(nameof(CategoryRepositoryTestFixture))]
public class CategoryRepositoryTestFixtureCollection
    : ICollectionFixture<CategoryRepositoryTestFixture>;

public class CategoryRepositoryTestFixture : BaseFixture
{
    public List<Category> GetValidCategoriesWithNames(List<string> categoryNames)
    {
        return categoryNames
            .Select(name =>
            {
                var category = GetValidCategory();
                category.Update(name);
                return category;
            })
            .ToList();
    }

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
            _ => listClone.OrderBy(x => x.Name)
        };
        return orderedEnumerable.ThenBy(x => x.Id).ToList();
    }
}