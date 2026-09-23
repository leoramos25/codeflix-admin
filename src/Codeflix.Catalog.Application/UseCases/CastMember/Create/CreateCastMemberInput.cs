using Codeflix.Catalog.Domain.Enum;
using MediatR;

namespace Codeflix.Catalog.Application.UseCases.CastMember.Create;

public record CreateCastMemberInput(string Name, CastMemberType Type) : IRequest<CreateCastMemberOutput>;