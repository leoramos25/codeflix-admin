using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using Codeflix.Catalog.IntegrationTests.Application.UseCases.Genre.Common;
using DomainEntity = Codeflix.Catalog.Domain.Entity;

namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.Genre.ListGenres;

[CollectionDefinition(nameof(ListGenresTestFixture))]
public class ListGenresTestFixtureCollection : ICollectionFixture<ListGenresTestFixture>;

public class ListGenresTestFixture : GenreUseCaseTestFixture
{
    public List<DomainEntity.Genre> GetOrderedGenres(
        List<DomainEntity.Genre> genres,
        string orderBy,
        SearchOrder order
    )
    {
        var listClone = new List<DomainEntity.Genre>(genres);
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

    public List<DomainEntity.Genre> GetValidGenres(int size = 10)
    {
        return [.. Enumerable.Range(1, size).Select(_ => GetValidGenre())];
    }
};
