using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using Codeflix.Catalog.EndToEndTests.Api.Category.Common;

namespace Codeflix.Catalog.EndToEndTests.Api.Category.ListCategories;

[CollectionDefinition(nameof(ListCategoriesApiTestFixture))]
public class ListCategoriesApiTestFixtureCollection
    : ICollectionFixture<ListCategoriesApiTestFixture>;

public class ListCategoriesApiTestFixture : CategoryBaseFixture
{
    public List<Domain.Entity.Category> GetValidCategories(int size = 10) =>
        Enumerable.Range(0, size).Select(_ => GetValidCategory()).ToList();

    public List<Domain.Entity.Category> GetOrderedCategories(
        List<Domain.Entity.Category> categories,
        string orderBy,
        SearchOrder order
    )
    {
        var listClone = new List<Domain.Entity.Category>(categories);
        var orderedEnumerable = (orderBy.ToLower(), order) switch
        {
            ("name", SearchOrder.Asc) => listClone.OrderBy(
                x => x.Name,
                StringComparer.OrdinalIgnoreCase
            ),
            ("name", SearchOrder.Desc) => listClone.OrderByDescending(
                x => x.Name,
                StringComparer.OrdinalIgnoreCase
            ),
            ("id", SearchOrder.Asc) => listClone.OrderBy(x => x.Id),
            ("id", SearchOrder.Desc) => listClone.OrderByDescending(x => x.Id),
            ("createdat", SearchOrder.Asc) => listClone.OrderBy(x => x.CreatedAt),
            ("createdat", SearchOrder.Desc) => listClone.OrderByDescending(x => x.CreatedAt),
            _ => listClone.OrderBy(x => x.Name),
        };
        return orderedEnumerable.ThenBy(x => x.Id).ToList();
    }

    public List<Domain.Entity.Category> GetValidCategoriesWithNames(List<string> categoryNames) =>
        categoryNames
            .Select(name =>
            {
                var category = GetValidCategory();
                category.Update(name);
                return category;
            })
            .ToList();
}
