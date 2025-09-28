using Codeflix.Catalog.Application.UseCases.Category.List;
using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using Codeflix.Catalog.IntegrationTests.Application.UseCases.Category.Common;

namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.Category.ListCategories;

[CollectionDefinition(nameof(ListCategoriesTestFixture))]
public class ListCategoriesTestFixtureCollection : ICollectionFixture<ListCategoriesTestFixture>;

public class ListCategoriesTestFixture : CategoryUseCaseBaseFixture
{
    public List<Domain.Entity.Category> GetOrderedCategories(
        List<Domain.Entity.Category> categories,
        string orderBy,
        SearchOrder order
    )
    {
        var listClone = new List<Domain.Entity.Category>(categories);
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
