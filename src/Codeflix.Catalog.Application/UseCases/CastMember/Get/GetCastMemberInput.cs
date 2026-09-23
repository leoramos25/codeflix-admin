using MediatR;

namespace Codeflix.Catalog.Application.UseCases.CastMember.Get;

public record GetCastMemberInput(Guid Id) : IRequest<GetCastMemberOutput>;
