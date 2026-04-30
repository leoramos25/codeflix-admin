using Codeflix.Catalog.Application.Interfaces;
using Codeflix.Catalog.Domain.Repository;

namespace Codeflix.Catalog.Application.UseCases.Category.Update;

public class UpdateCategory(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    : IUpdateCategory
{
    public async Task<UpdateCategoryOutput> Handle(
        UpdateCategoryInput request,
        CancellationToken cancellationToken
    )
    {
        var category = await categoryRepository.Get(request.Id, cancellationToken);
        category.Update(request.Name, request.Description);
        if (request.IsActive.HasValue && request.IsActive != category.IsActive)
            if (request.IsActive.Value)
                category.Activate();
            else
                category.Deactivate();
        await categoryRepository.Update(category, cancellationToken);
        await unitOfWork.Commit(cancellationToken);
        return UpdateCategoryOutput.FromCategory(category);
    }
}
