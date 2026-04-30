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
            random.Next(1, 10),
            random.Next(10, 100),
            Faker.Commerce.ProductName(),
            Faker.Commerce.ProductName(),
            Faker.PickRandom(SearchOrder.Asc, SearchOrder.Desc)
        );
    }
}
