using FluentValidation;

namespace Codeflix.Catalog.Application.UseCases.Category.Update;

public class UpdateCategoryInputValidator : AbstractValidator<UpdateCategoryInput>
{
    public UpdateCategoryInputValidator() => RuleFor(x => x.Id).NotNull().NotEmpty();
}
