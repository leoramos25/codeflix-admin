using Codeflix.Catalog.Application.Interfaces;
using Codeflix.Catalog.Domain.Repository;

namespace Codeflix.Catalog.Application.UseCases.Category.Create;

public class CreateCategory(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    : ICreateCategory
{
    public async Task<CreateCategoryOutput> Handle(
        CreateCategoryInput request,
        CancellationToken cancellationToken
    )
    {
        var category = new Domain.Entity.Category(
            request.Name,
            request.Description,
            request.IsActive
        );

        await categoryRepository.Insert(category, cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        return CreateCategoryOutput.FromCategory(category);
    }
}
