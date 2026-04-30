using MediatR;

namespace Codeflix.Catalog.Application.UseCases.Category.Delete;

public class DeleteCategoryInput : IRequest
{
    public DeleteCategoryInput(Guid categoryId)
    {
        CategoryId = categoryId;
    }

    public Guid CategoryId { get; set; }
}
