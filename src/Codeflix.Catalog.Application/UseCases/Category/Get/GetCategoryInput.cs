using MediatR;

namespace Codeflix.Catalog.Application.UseCases.Category.Get;

public class GetCategoryInput : IRequest<GetCategoryOutput>
{
    public Guid CategoryId { get; set; }

    public GetCategoryInput(Guid categoryId)
    {
        CategoryId = categoryId;
    }
}
