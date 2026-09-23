using MediatR;

namespace Codeflix.Catalog.Application.UseCases.CastMember.Create;

public interface ICreateCastMember : IRequestHandler<CreateCastMemberInput, CreateCastMemberOutput>;