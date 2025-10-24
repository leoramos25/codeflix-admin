using Codeflix.Catalog.Domain.Entity;
using Codeflix.Catalog.IntegrationTests.Common;

namespace Codeflix.Catalog.IntegrationTests.Infra.Data.EF.Repositories.GenreRepository;

[CollectionDefinition(nameof(GenreRepositoryTestFixture))]
public class GenreRepositoryTestFixtureCollection : ICollectionFixture<GenreRepositoryTestFixture>;

public class GenreRepositoryTestFixture : BaseFixture
{
    public List<Genre> GetValidGenres(int size = 10)
    {
        return Enumerable
            .Range(1, size)
            .Select(_ => GetValidGenreWithCategories(new Random().Next(1, 4)))
            .ToList();
    }

    public Genre GetValidGenreWithCategories(int categoriesSize)
    {
        var genre = GetValidGenre();
        var categoryIds = Enumerable.Range(1, categoriesSize).Select(_ => Guid.NewGuid()).ToList();
        categoryIds.ForEach(genre.AddCategory);
        return genre;
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