using Codeflix.Catalog.EndToEndTests.Common;

namespace Codeflix.Catalog.EndToEndTests.Api.Category.Common;

public class CategoryBaseFixture : BaseFixture
{
    protected const int CategoryNameMinLength = 2;
    protected const int CategoryNameMaxLength = 255;
    protected const int CategoryDescriptionMaxLength = 10_000;

    public CategoryBaseFixture()
    {
        Persistence = new CategoryPersistence(CreateDbContext());
    }

    public CategoryPersistence Persistence { get; }

    public Domain.Entity.Category GetValidCategory()
    {
        return new Domain.Entity.Category(GetValidCategoryName(), GetValidCategoryDescription(), GetRandomBoolean());
    }

    public string GetInvalidInputDescriptionLong()
    {
        var invalidInputDescriptionLong = Faker.Commerce.ProductDescription();
        while (invalidInputDescriptionLong.Length <= CategoryDescriptionMaxLength)
            invalidInputDescriptionLong += Faker.Commerce.ProductDescription();
        return invalidInputDescriptionLong;
    }

    public string GetInvalidInputLongName()
    {
        var invalidInputLongName = Faker.Commerce.ProductName();
        while (invalidInputLongName.Length <= CategoryNameMaxLength)
            invalidInputLongName += Faker.Commerce.ProductDescription();
        return invalidInputLongName;
    }

    public string GetInvalidInputShortName()
    {
        var invalidInputShortName = Faker.Commerce.ProductName();
        invalidInputShortName = invalidInputShortName[..CategoryNameMinLength];
        return invalidInputShortName;
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

    public bool GetRandomBoolean()
    {
        return new Random().NextDouble() < 0.5;
    }
}