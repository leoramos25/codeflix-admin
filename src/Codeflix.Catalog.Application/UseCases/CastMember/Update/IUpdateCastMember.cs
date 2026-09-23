using MediatR;

namespace Codeflix.Catalog.Application.UseCases.CastMember.Update;

public interface IUpdateCastMember : IRequestHandler<UpdateCastMemberInput, UpdateCastMemberOutput>;
