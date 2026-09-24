using Codeflix.Catalog.Domain.Enum;

namespace Codeflix.Catalog.Application.UseCases.CastMember.List;

public record ListCastMembersItemOutput(
    Guid Id,
    string Name,
    CastMemberType Type,
    DateTime CreatedAt
)
{
    public static ListCastMembersItemOutput FromCastMember(Domain.Entity.CastMember castMember) =>
        new(castMember.Id, castMember.Name, castMember.Type, castMember.CreatedAt);
}
