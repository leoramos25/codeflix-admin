using Codeflix.Catalog.UnitTests.Common;

namespace Codeflix.Catalog.UnitTests.Domain.Entity.Genre;

[CollectionDefinition(nameof(GenreTestFixture))]
public class GenreTestFixtureCollection : ICollectionFixture<GenreTestFixture>;

public class GenreTestFixture : BaseFixture
{
    public string GetValidName()
    {
        return Faker.Music.Genre();
    }

    public Catalog.Domain.Entity.Genre GetValidGenre(List<Guid>? categoryIds = null)
    {
        var genre = new Catalog.Domain.Entity.Genre(GetValidName(), GetRandomBoolean());
        categoryIds?.ForEach(categoryId => genre.AddCategory(categoryId));
        return genre;
    }

    public bool GetRandomBoolean() => new Random().NextDouble() < 0.5;
}
