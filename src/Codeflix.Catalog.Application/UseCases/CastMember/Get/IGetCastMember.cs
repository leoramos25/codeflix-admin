using MediatR;

namespace Codeflix.Catalog.Application.UseCases.CastMember.Get;

public interface IGetCastMember : IRequestHandler<GetCastMemberInput, GetCastMemberOutput>;
