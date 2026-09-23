using Codeflix.Catalog.Application.Interfaces;
using Codeflix.Catalog.Domain.Repository;

namespace Codeflix.Catalog.Application.UseCases.CastMember.Delete;

public class DeleteCastMember(ICastMemberRepository castMemberRepository, IUnitOfWork unitOfWork)
    : IDeleteCastMember
{
    public async Task Handle(DeleteCastMemberInput request, CancellationToken cancellationToken)
    {
        var castMember = await castMemberRepository.Get(request.Id, cancellationToken);
        await castMemberRepository.Delete(castMember, cancellationToken);
        await unitOfWork.Commit(cancellationToken);
    }
}
