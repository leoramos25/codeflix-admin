using Codeflix.Catalog.Domain.Enum;
using DomainEntity = Codeflix.Catalog.Domain.Entity;

namespace Codeflix.Catalog.Application.UseCases.CastMember.Get;

public record GetCastMemberOutput(Guid Id, string Name, CastMemberType Type, DateTime CreatedAt)
{
    public static GetCastMemberOutput FromCastMember(DomainEntity.CastMember castMember) =>
        new(castMember.Id, castMember.Name, castMember.Type, castMember.CreatedAt);
}
