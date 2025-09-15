using FluentValidation;

namespace Codeflix.Catalog.Application.UseCases.Category.Get;

public class GetCategoryInputValidator : AbstractValidator<GetCategoryInput>
{
    public GetCategoryInputValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty().NotNull();
    }
}
