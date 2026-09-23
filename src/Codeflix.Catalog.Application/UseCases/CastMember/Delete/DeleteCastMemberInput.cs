using MediatR;

namespace Codeflix.Catalog.Application.UseCases.CastMember.Delete;

public record DeleteCastMemberInput(Guid Id) : IRequest;
