using Codeflix.Catalog.Application.Interfaces;
using Codeflix.Catalog.Domain.Repository;

namespace Codeflix.Catalog.Application.UseCases.Category.Delete;

public class DeleteCategory(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    : IDeleteCategory
{
    public async Task Handle(DeleteCategoryInput request, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.Get(request.CategoryId, cancellationToken);
        await categoryRepository.Delete(category, cancellationToken);
        await unitOfWork.Commit(cancellationToken);
    }
}
