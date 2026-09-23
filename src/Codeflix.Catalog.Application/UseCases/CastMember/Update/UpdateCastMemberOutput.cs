using Codeflix.Catalog.Domain.Enum;

namespace Codeflix.Catalog.Application.UseCases.CastMember.Update;

public record UpdateCastMemberOutput(Guid Id, string Name, CastMemberType Type, DateTime CreatedAt)
{
    public static UpdateCastMemberOutput FromCastMember(Domain.Entity.CastMember castMember) =>
        new(castMember.Id, castMember.Name, castMember.Type, castMember.CreatedAt);
}
