using Codeflix.Catalog.EndToEndTests.Common;
using DomainEntity = Codeflix.Catalog.Domain.Entity;

namespace Codeflix.Catalog.EndToEndTests.Api.Genre.Common;

public class GenreBaseFixture : BaseFixture
{
    public GenrePersistence Persistence { get; }

    public GenreBaseFixture()
    {
        Persistence = new GenrePersistence(CreateDbContext());
    }

    public List<DomainEntity.Genre> GetValidGenresWithNames(List<string> genreNames)
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

    public List<DomainEntity.Genre> GetValidGenres(int size = 10)
    {
        return [.. Enumerable.Range(1, size).Select(_ => GetValidGenre())];
    }

    public DomainEntity.Genre GetValidGenre(bool? isActive = null)
    {
        return new DomainEntity.Genre(GetValidName(), isActive ?? GetRandomBoolean());
    }

    public string GetValidName()
    {
        return Faker.Music.Genre();
    }
}
