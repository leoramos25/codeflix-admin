using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using Codeflix.Catalog.EndToEndTests.Api.Genre.Common;
using DomainEntity = Codeflix.Catalog.Domain.Entity;

namespace Codeflix.Catalog.EndToEndTests.Api.Genre.ListGenres;

[CollectionDefinition(nameof(ListGenresApiTestFixture))]
public class ListGenresApiTestFixtureCollection : ICollectionFixture<ListGenresApiTestFixture>;

public class ListGenresApiTestFixture : GenreBaseFixture
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
}
