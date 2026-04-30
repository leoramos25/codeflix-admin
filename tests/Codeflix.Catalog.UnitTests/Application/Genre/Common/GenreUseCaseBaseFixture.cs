using Codeflix.Catalog.Application.Interfaces;
using Codeflix.Catalog.Domain.Repository;
using Codeflix.Catalog.UnitTests.Common;
using Moq;

namespace Codeflix.Catalog.UnitTests.Application.Genre.Common;

public class GenreUseCaseBaseFixture : BaseFixture
{
    public Mock<IUnitOfWork> GetUnitOfWork()
    {
        return new Mock<IUnitOfWork>();
    }

    public Mock<IGenreRepository> GetGenreRepository()
    {
        return new Mock<IGenreRepository>();
    }

    public Mock<ICategoryRepository> GetCategoryRepository()
    {
        return new Mock<ICategoryRepository>();
    }

    public List<Catalog.Domain.Entity.Genre> GetValidGenres(int size = 10)
    {
        return Enumerable
            .Range(1, size)
            .Select(_ => GetValidGenreWithCategories(new Random().Next(1, 4)))
            .ToList();
    }

    public Catalog.Domain.Entity.Genre GetValidGenreWithCategories(int categoriesSize)
    {
        var genre = GetValidGenre();
        var categoryIds = Enumerable.Range(1, categoriesSize).Select(_ => Guid.NewGuid()).ToList();
        categoryIds.ForEach(genre.AddCategory);
        return genre;
    }

    public Catalog.Domain.Entity.Genre GetValidGenre()
    {
        return new Catalog.Domain.Entity.Genre(GetValidName(), GetRandomBoolean());
    }

    public string GetValidName()
    {
        return Faker.Music.Genre();
    }
}
