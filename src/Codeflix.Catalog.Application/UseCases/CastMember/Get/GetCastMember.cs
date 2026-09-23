using Codeflix.Catalog.Domain.Repository;

namespace Codeflix.Catalog.Application.UseCases.CastMember.Get;

public class GetCastMember(ICastMemberRepository castMemberRepository) : IGetCastMember
{
    public async Task<GetCastMemberOutput> Handle(
        GetCastMemberInput request,
        CancellationToken cancellationToken
    )
    {
        var castMember = await castMemberRepository.Get(request.Id, cancellationToken);
        return GetCastMemberOutput.FromCastMember(castMember);
    }
}
