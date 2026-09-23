using Codeflix.Catalog.Application.Interfaces;
using Codeflix.Catalog.Domain.Repository;
using DomainEntity = Codeflix.Catalog.Domain.Entity;

namespace Codeflix.Catalog.Application.UseCases.CastMember.Create;

public class CreateCastMember(ICastMemberRepository castMemberRepository, IUnitOfWork unitOfWork)
    : ICreateCastMember
{
    public async Task<CreateCastMemberOutput> Handle(
        CreateCastMemberInput request,
        CancellationToken cancellationToken
    )
    {
        var castMember = new DomainEntity.CastMember(request.Name, request.Type);
        await castMemberRepository.Insert(castMember, cancellationToken);
        await unitOfWork.Commit(cancellationToken);
        return CreateCastMemberOutput.FromCastMember(castMember);
    }
}
