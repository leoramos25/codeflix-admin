using Codeflix.Catalog.Domain.Repository;

namespace Codeflix.Catalog.Application.UseCases.CastMember.List;

public class ListCastMembers(ICastMemberRepository castMemberRepository) : IListCastMembers
{
    public async Task<ListCastMembersOutput> Handle(
        ListCastMembersInput request,
        CancellationToken cancellationToken
    )
    {
        var searchOutput = await castMemberRepository.Search(
            request.ToSearchInput(),
            cancellationToken
        );
        return new ListCastMembersOutput(
            searchOutput.CurrentPage,
            searchOutput.PerPage,
            searchOutput.Total,
            searchOutput.Items.Select(ListCastMembersItemOutput.FromCastMember).ToList()
        );
    }
}
