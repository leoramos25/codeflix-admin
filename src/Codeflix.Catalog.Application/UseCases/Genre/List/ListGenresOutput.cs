using Codeflix.Catalog.Application.Common;

namespace Codeflix.Catalog.Application.UseCases.Genre.List;

public class ListGenresOutput : PaginatedListOutput<ListGenresItemOutput>
{
    public ListGenresOutput(
        int page,
        int perPage,
        int total,
        IReadOnlyList<ListGenresItemOutput> items
    )
        : base(page, perPage, total, items) { }
}
