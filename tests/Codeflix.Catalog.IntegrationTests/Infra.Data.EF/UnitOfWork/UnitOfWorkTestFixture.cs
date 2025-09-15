using Codeflix.Catalog.Domain.Entity;
using Codeflix.Catalog.IntegrationTests.Common;

namespace Codeflix.Catalog.IntegrationTests.Infra.Data.EF.UnitOfWork;

[CollectionDefinition(nameof(UnitOfWorkTestFixture))]
public class UnitOfWorkTestFixtureCollection : ICollectionFixture<UnitOfWorkTestFixture>;

public class UnitOfWorkTestFixture : BaseFixture
{
    private const int CategoryNameMinLength = 2;
    private const int CategoryNameMaxLength = 255;
    private const int CategoryDescriptionMaxLength = 10_000;

    public List<Category> GetValidCategories(int size = 10) =>
        Enumerable.Range(0, size).Select(_ => GetValidCategory()).ToList();

    public Category GetValidCategory() =>
        new(GetValidCategoryName(), GetValidCategoryDescription(), GetRandomBoolean());

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

    public bool GetRandomBoolean() => new Random().NextDouble() < 0.5;
}
