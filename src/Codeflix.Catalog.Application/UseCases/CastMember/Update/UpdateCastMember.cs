using Codeflix.Catalog.Application.Interfaces;
using Codeflix.Catalog.Domain.Repository;

namespace Codeflix.Catalog.Application.UseCases.CastMember.Update;

public class UpdateCastMember(ICastMemberRepository castMemberRepository, IUnitOfWork unitOfWork)
    : IUpdateCastMember
{
    public async Task<UpdateCastMemberOutput> Handle(
        UpdateCastMemberInput request,
        CancellationToken cancellationToken
    )
    {
        var castMember = await castMemberRepository.Get(request.Id, cancellationToken);
        castMember.Update(request.Name, request.Type);
        await castMemberRepository.Update(castMember, cancellationToken);
        await unitOfWork.Commit(cancellationToken);
        return UpdateCastMemberOutput.FromCastMember(castMember);
    }
}
