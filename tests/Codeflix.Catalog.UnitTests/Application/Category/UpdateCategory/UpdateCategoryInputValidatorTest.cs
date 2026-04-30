using Codeflix.Catalog.Application.UseCases.Category.Update;
using FluentAssertions;

namespace Codeflix.Catalog.UnitTests.Application.Category.UpdateCategory;

[Collection(nameof(UpdateCategoryTestFixture))]
public class UpdateCategoryInputValidatorTest(UpdateCategoryTestFixture fixture)
{
    [Fact(DisplayName = nameof(ValidationOk))]
    [Trait("Application", "UpdateCategoryInputValidator - Validation")]
    public void ValidationOk()
    {
        var input = fixture.GetValidInput();
        var validator = new UpdateCategoryInputValidator();

        var validationResult = validator.Validate(input);

        validationResult.Should().NotBeNull();
        validationResult.IsValid.Should().BeTrue();
        validationResult.Errors.Should().BeEmpty();
    }

    [Fact(DisplayName = nameof(InvalidWhenIdIsEmpty))]
    [Trait("Application", "UpdateCategoryInputValidator - Validation")]
    public void InvalidWhenIdIsEmpty()
    {
        var input = fixture.GetValidInput(Guid.Empty);
        var validator = new UpdateCategoryInputValidator();

        var validationResult = validator.Validate(input);

        validationResult.Should().NotBeNull();
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().HaveCount(1);
        validationResult.Errors[0].ErrorMessage.Should().Be("'Id' must not be empty.");
    }
}
