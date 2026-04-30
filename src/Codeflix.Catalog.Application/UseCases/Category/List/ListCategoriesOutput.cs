using Codeflix.Catalog.Application.Common;

namespace Codeflix.Catalog.Application.UseCases.Category.List;

public class ListCategoriesOutput : PaginatedListOutput<ListCategoriesItemOutput>
{
    public ListCategoriesOutput(
        int page,
        int perPage,
        int total,
        IReadOnlyList<ListCategoriesItemOutput> items
    )
        : base(page, perPage, total, items) { }
}
