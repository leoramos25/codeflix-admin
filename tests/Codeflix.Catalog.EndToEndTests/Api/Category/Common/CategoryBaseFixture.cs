using Codeflix.Catalog.EndToEndTests.Common;

namespace Codeflix.Catalog.EndToEndTests.Api.Category.Common;

public class CategoryBaseFixture : BaseFixture
{
    public CategoryBaseFixture()
    {
        Persistence = new CategoryPersistence(CreateDbContext());
    }

    public CategoryPersistence Persistence { get; }

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
        invalidInputShortName = invalidInputShortName[..(CategoryNameMinLength - 1)];
        return invalidInputShortName;
    }
}
