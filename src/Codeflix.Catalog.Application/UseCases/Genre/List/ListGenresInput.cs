using Codeflix.Catalog.Application.Common;
using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using MediatR;

namespace Codeflix.Catalog.Application.UseCases.Genre.List;

public class ListGenresInput : PaginatedListInput, IRequest<ListGenresOutput>
{
    public ListGenresInput()
        : base(1, 15, "", "", SearchOrder.Asc) { }

    public ListGenresInput(int page, int perPage, string search, string sort, SearchOrder dir)
        : base(page, perPage, search, sort, dir) { }
}
