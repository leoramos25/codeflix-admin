using FluentValidation;

namespace Codeflix.Catalog.Application.UseCases.Category.Delete;

public class DeleteCategoryInputValidator : AbstractValidator<DeleteCategoryInput>
{
    public DeleteCategoryInputValidator()
    {
        RuleFor(x => x.CategoryId).NotNull().NotEmpty();
    }
}
