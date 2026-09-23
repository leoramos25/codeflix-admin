using Codeflix.Catalog.Domain.Enum;
using MediatR;

namespace Codeflix.Catalog.Application.UseCases.CastMember.Update;

public record UpdateCastMemberInput(Guid Id, string Name, CastMemberType Type)
    : IRequest<UpdateCastMemberOutput>;
