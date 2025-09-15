using MediatR;

namespace Codeflix.Catalog.Application.UseCases.Category.Delete;

public class DeleteCategoryInput : IRequest
{
    public Guid CategoryId { get; set; }

    public DeleteCategoryInput(Guid categoryId)
    {
        CategoryId = categoryId;
    }
}
