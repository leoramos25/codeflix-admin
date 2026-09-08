using Codeflix.Catalog.Application.Interfaces;
using Codeflix.Catalog.Domain.Repository;
using Codeflix.Catalog.UnitTests.Common;
using Moq;

namespace Codeflix.Catalog.UnitTests.Application.Genre.Common;

public class GenreUseCaseBaseFixture : BaseFixture
{
    protected const int CategoryNameMinLength = 2;
    protected const int CategoryNameMaxLength = 255;
    protected const int CategoryDescriptionMaxLength = 10_000;

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
        var categoryIds = Enumerable.Range(1, categoriesSize).Select(_ => Guid.NewGuid()).ToList();
        return GetValidGenreWithCategories(categoryIds);
    }

    public Catalog.Domain.Entity.Genre GetValidGenreWithCategories(List<Guid> categoryIds)
    {
        var genre = GetValidGenre();
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

    public List<Catalog.Domain.Entity.Category> GetValidCategories(int size = 5)
    {
        return Enumerable.Range(1, size).Select(_ => GetValidCategory()).ToList();
    }

    public Catalog.Domain.Entity.Category GetValidCategory()
    {
        return new Catalog.Domain.Entity.Category(
            GetValidCategoryName(),
            GetValidCategoryDescription(),
            GetRandomBoolean()
        );
    }

    public string GetValidCategoryName()
    {
        var categoryName = string.Empty;
        while (categoryName.Length < CategoryNameMinLength)
            categoryName = Faker.Commerce.Categories(1)[0];
        if (categoryName.Length > CategoryNameMaxLength)
            categoryName = categoryName[..CategoryNameMaxLength];
        return categoryName;
    }

    public string GetValidCategoryDescription()
    {
        var categoryDescription = Faker.Commerce.ProductDescription();
        if (categoryDescription.Length > CategoryDescriptionMaxLength)
            categoryDescription = categoryDescription[..CategoryDescriptionMaxLength];
        return categoryDescription;
    }
}
