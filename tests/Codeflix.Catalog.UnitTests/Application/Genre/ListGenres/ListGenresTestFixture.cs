using Codeflix.Catalog.Application.UseCases.Genre.List;
using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using Codeflix.Catalog.UnitTests.Application.Genre.Common;

namespace Codeflix.Catalog.UnitTests.Application.Genre.ListGenres;

[CollectionDefinition(nameof(ListGenresTestFixture))]
public class ListGenresTestFixtureCollection : ICollectionFixture<ListGenresTestFixture>;

public class ListGenresTestFixture : GenreUseCaseBaseFixture
{
    public ListGenresInput GetValidInput()
    {
        var random = new Random();
        return new ListGenresInput(
            page: random.Next(1, 10),
            perPage: random.Next(10, 100),
            search: Faker.Commerce.ProductName(),
            sort: Faker.Commerce.ProductName(),
            dir: Faker.PickRandom(SearchOrder.Asc, SearchOrder.Desc)
        );
    }
}
