using Codeflix.Catalog.Application.Interfaces;
using Codeflix.Catalog.Domain.Repository;
using Codeflix.Catalog.UnitTests.Common;
using Moq;

namespace Codeflix.Catalog.UnitTests.Application.Genre.Common;

public class GenreUseCaseBaseFixture : BaseFixture
{
    public Mock<IUnitOfWork> GetUnitOfWork() => new();

    public Mock<IGenreRepository> GetGenreRepository() => new();

    public Mock<ICategoryRepository> GetCategoryRepository() => new();

    public Catalog.Domain.Entity.Genre GetValidGenreWithCategories(int categoriesSize)
    {
        var genre = GetValidGenre();
        var categoryIds = Enumerable.Range(1, categoriesSize).Select(_ => Guid.NewGuid()).ToList();
        categoryIds.ForEach(genre.AddCategory);
        return genre;
    }

    public Catalog.Domain.Entity.Genre GetValidGenre() => new(GetValidName(), GetRandomBoolean());

    public string GetValidName() => Faker.Music.Genre();
}
