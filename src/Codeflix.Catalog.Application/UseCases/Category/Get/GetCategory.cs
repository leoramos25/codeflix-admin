using Codeflix.Catalog.Domain.Repository;

namespace Codeflix.Catalog.Application.UseCases.Category.Get;

public class GetCategory(ICategoryRepository categoryRepository) : IGetCategory
{
    public async Task<GetCategoryOutput> Handle(
        GetCategoryInput request,
        CancellationToken cancellationToken
    )
    {
        var category = await categoryRepository.Get(request.CategoryId, cancellationToken);
        return GetCategoryOutput.FromCategory(category);
    }
}
