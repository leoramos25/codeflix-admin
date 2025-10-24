using MediatR;

namespace Codeflix.Catalog.Application.UseCases.Category.Get;

public class GetCategoryInput : IRequest<GetCategoryOutput>
{
    public GetCategoryInput(Guid categoryId)
    {
        CategoryId = categoryId;
    }

    public Guid CategoryId { get; set; }
}