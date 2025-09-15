using Codeflix.Catalog.Application.UseCases.Category.Delete;
using FluentAssertions;

namespace Codeflix.Catalog.UnitTests.Application.Category.DeleteCategory;

public class DeleteValidationValidatorTest
{
    [Fact(DisplayName = nameof(ValidationOk))]
    [Trait("Application", "DeleteCategoryInputValidation - Use Cases")]
    public void ValidationOk()
    {
        var validInput = new DeleteCategoryInput(Guid.NewGuid());
        var validator = new DeleteCategoryInputValidator();
        var validationResult = validator.Validate(validInput);

        validationResult.Should().NotBeNull();
        validationResult.IsValid.Should().BeTrue();
        validationResult.Errors.Should().BeEmpty();
    }

    [Fact(DisplayName = nameof(InvalidWhenGuidIsEmpty))]
    [Trait("Application", "DeleteCategoryInputValidation - Use Cases")]
    public void InvalidWhenGuidIsEmpty()
    {
        var invalidInput = new DeleteCategoryInput(Guid.Empty);
        var validator = new DeleteCategoryInputValidator();
        var validationResult = validator.Validate(invalidInput);

        validationResult.Should().NotBeNull();
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().NotBeEmpty();
        validationResult.Errors.Should().HaveCount(1);
        validationResult.Errors[0].ErrorMessage.Should().Be("'Category Id' must not be empty.");
    }
}
