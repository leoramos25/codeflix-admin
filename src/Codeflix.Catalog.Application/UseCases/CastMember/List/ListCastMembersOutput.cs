using Codeflix.Catalog.Application.Common;

namespace Codeflix.Catalog.Application.UseCases.CastMember.List;

public class ListCastMembersOutput : PaginatedListOutput<ListCastMembersItemOutput>
{
    public ListCastMembersOutput(
        int page,
        int perPage,
        int total,
        IReadOnlyList<ListCastMembersItemOutput> items
    )
        : base(page, perPage, total, items) { }
}
