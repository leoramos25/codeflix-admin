using Codeflix.Catalog.Domain.Repository;

namespace Codeflix.Catalog.Application.UseCases.Category.List;

public class ListCategories(ICategoryRepository categoryRepository) : IListCategories
{
    public async Task<ListCategoriesOutput> Handle(
        ListCategoriesInput request,
        CancellationToken cancellationToken
    )
    {
        var searchOutput = await categoryRepository.Search(
            request.ToSearchInput(),
            cancellationToken
        );
        return new ListCategoriesOutput(
            searchOutput.CurrentPage,
            searchOutput.PerPage,
            searchOutput.Total,
            searchOutput.Items.Select(ListCategoriesItemOutput.FromCategory).ToList()
        );
    }
}
