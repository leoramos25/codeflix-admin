using Codeflix.Catalog.Domain.Entity;
using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using Codeflix.Catalog.IntegrationTests.Common;

namespace Codeflix.Catalog.IntegrationTests.Infra.Data.EF.Repositories.GenreRepository;

[CollectionDefinition(nameof(GenreRepositoryTestFixture))]
public class GenreRepositoryTestFixtureCollection : ICollectionFixture<GenreRepositoryTestFixture>;

public class GenreRepositoryTestFixture : BaseFixture
{
    public List<Genre> GetValidGenres(int size = 10)
    {
        return Enumerable.Range(1, size).Select(_ => GetValidGenre()).ToList();
    }

    public List<Genre> GetValidGenresWithNames(List<string> genreNames)
    {
        return genreNames
            .Select(name =>
            {
                var category = GetValidGenre();
                category.Update(name);
                return category;
            })
            .ToList();
    }

    public List<Genre> GetOrderedGenres(List<Genre> genres, string orderBy, SearchOrder order)
    {
        var listClone = new List<Genre>(genres);
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

    public Genre GetValidGenre()
    {
        return new Genre(GetValidName(), GetRandomBoolean());
    }

    public string GetValidName()
    {
        return Faker.Music.Genre();
    }
}
