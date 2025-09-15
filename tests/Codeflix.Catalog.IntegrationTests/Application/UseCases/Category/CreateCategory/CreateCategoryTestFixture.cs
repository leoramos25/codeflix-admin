using Codeflix.Catalog.Application.UseCases.Category.Create;
using Codeflix.Catalog.IntegrationTests.Application.UseCases.Category.Common;

namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.Category.CreateCategory;

[CollectionDefinition(nameof(CreateCategoryTestFixture))]
public class CreateCategoryTestFixtureCollection : ICollectionFixture<CreateCategoryTestFixture>;

public class CreateCategoryTestFixture : CategoryUseCaseBaseFixture
{
    public CreateCategoryInput GetInvalidInputNameNull()
    {
        var invalidNullNameInput = GetValidInput();
        invalidNullNameInput.Name = null!;
        return invalidNullNameInput;
    }

    public CreateCategoryInput GetInvalidInputDescriptionNull()
    {
        var invalidNullDescriptionInput = GetValidInput();
        invalidNullDescriptionInput.Description = null!;
        return invalidNullDescriptionInput;
    }

    public CreateCategoryInput GetInvalidInputDescriptionLong()
    {
        var invalidInputDescriptionLong = GetValidInput();
        while (invalidInputDescriptionLong.Description.Length <= CategoryDescriptionMaxLength)
            invalidInputDescriptionLong.Description += Faker.Commerce.ProductDescription();
        return invalidInputDescriptionLong;
    }

    public CreateCategoryInput GetInvalidInputLongName()
    {
        var invalidInputLongName = GetValidInput();
        while (invalidInputLongName.Name.Length <= CategoryNameMaxLength)
            invalidInputLongName.Name += Faker.Commerce.ProductDescription();
        return invalidInputLongName;
    }

    public CreateCategoryInput GetInvalidInputShortName()
    {
        var invalidInputShortName = GetValidInput();
        invalidInputShortName.Name = invalidInputShortName.Name[..CategoryNameMinLength];
        return invalidInputShortName;
    }

    public CreateCategoryInput GetValidInput() =>
        new(GetValidCategoryName(), GetValidCategoryDescription(), GetRandomBoolean());
}
