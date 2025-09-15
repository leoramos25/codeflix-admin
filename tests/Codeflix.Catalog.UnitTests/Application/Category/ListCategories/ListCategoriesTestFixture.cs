using Codeflix.Catalog.Application.UseCases.Category.List;
using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using Codeflix.Catalog.UnitTests.Application.Category.Common;

namespace Codeflix.Catalog.UnitTests.Application.Category.ListCategories;

[CollectionDefinition(nameof(ListCategoriesTestFixture))]
public class ListCategoriesTestFixtureCollection : ICollectionFixture<ListCategoriesTestFixture>;

public class ListCategoriesTestFixture : CategoryUseCaseBaseFixture
{
    public ListCategoriesInput GetValidInput()
    {
        var random = new Random();
        return new ListCategoriesInput(
            page: random.Next(1, 10),
            perPage: random.Next(10, 100),
            search: Faker.Commerce.ProductName(),
            sort: Faker.Commerce.ProductName(),
            dir: Faker.PickRandom(SearchOrder.Asc, SearchOrder.Desc)
        );
    }

    public List<Catalog.Domain.Entity.Category> GetCategories(int size = 10)
    {
        List<Catalog.Domain.Entity.Category> categories = [];
        for (var i = 0; i < size; i++)
        {
            categories.Add(GetValidCategory());
        }

        return categories;
    }
}
