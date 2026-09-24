using Codeflix.Catalog.Application.Common;
using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using MediatR;

namespace Codeflix.Catalog.Application.UseCases.CastMember.List;

public class ListCastMembersInput : PaginatedListInput, IRequest<ListCastMembersOutput>
{
    public ListCastMembersInput()
        : base(1, 15, "", "", SearchOrder.Asc) { }

    public ListCastMembersInput(
        int page = 1,
        int perPage = 15,
        string search = "",
        string sort = "",
        SearchOrder dir = SearchOrder.Asc
    )
        : base(page, perPage, search, sort, dir) { }
}
