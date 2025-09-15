using Codeflix.Catalog.Application.UseCases.Category.Update;
using Codeflix.Catalog.IntegrationTests.Application.UseCases.Category.Common;

namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.Category.UpdateCategory;

[CollectionDefinition(nameof(UpdateCategoryTestFixture))]
public class UpdateCategoryTestFixtureCollection : ICollectionFixture<UpdateCategoryTestFixture>;

public class UpdateCategoryTestFixture : CategoryUseCaseBaseFixture
{
    public UpdateCategoryInput GetInvalidInputNameNull()
    {
        var invalidNullNameInput = GetValidInput();
        invalidNullNameInput.Name = null!;
        return invalidNullNameInput;
    }

    public UpdateCategoryInput GetInvalidInputDescriptionLong()
    {
        var invalidInputDescriptionLong = GetValidInput();
        while (invalidInputDescriptionLong.Description!.Length <= CategoryDescriptionMaxLength)
            invalidInputDescriptionLong.Description += Faker.Commerce.ProductDescription();
        return invalidInputDescriptionLong;
    }

    public UpdateCategoryInput GetInvalidInputLongName()
    {
        var invalidInputLongName = GetValidInput();
        while (invalidInputLongName.Name.Length <= CategoryNameMaxLength)
            invalidInputLongName.Name += Faker.Commerce.ProductDescription();
        return invalidInputLongName;
    }

    public UpdateCategoryInput GetInvalidInputShortName()
    {
        var invalidInputShortName = GetValidInput();
        invalidInputShortName.Name = invalidInputShortName.Name[..2];
        return invalidInputShortName;
    }

    public UpdateCategoryInput GetValidInput(Guid? id = null) =>
        new(
            id ?? Guid.NewGuid(),
            GetValidCategoryName(),
            GetValidCategoryDescription(),
            GetRandomBoolean()
        );
}
