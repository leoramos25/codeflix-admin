using Codeflix.Catalog.Application.UseCases.Category.Get;
using FluentAssertions;

namespace Codeflix.Catalog.UnitTests.Application.Category.GetCategory;

[Collection(nameof(GetCategoryTestFixture))]
public class GetCategoryInputValidatorTest(GetCategoryTestFixture fixture)
{
    [Fact(DisplayName = nameof(ValidationOk))]
    [Trait("Application", "GetCategoryInputValidation - Use Cases")]
    public void ValidationOk()
    {
        var validInput = new GetCategoryInput(Guid.NewGuid());
        var validator = new GetCategoryInputValidator();
        var validationResult = validator.Validate(validInput);

        validationResult.Should().NotBeNull();
        validationResult.IsValid.Should().BeTrue();
        validationResult.Errors.Should().BeEmpty();
    }

    [Fact(DisplayName = nameof(InvalidWhenGuidIsEmpty))]
    [Trait("Application", "GetCategoryInputValidation - Use Cases")]
    public void InvalidWhenGuidIsEmpty()
    {
        var validInput = new GetCategoryInput(Guid.Empty);
        var validator = new GetCategoryInputValidator();
        var validationResult = validator.Validate(validInput);

        validationResult.Should().NotBeNull();
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().NotBeEmpty();
        validationResult.Errors.Should().HaveCount(1);
        validationResult.Errors[0].ErrorMessage.Should().Be("'Category Id' must not be empty.");
    }
}
