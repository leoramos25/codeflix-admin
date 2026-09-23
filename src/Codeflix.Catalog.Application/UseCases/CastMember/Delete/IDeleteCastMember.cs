using MediatR;

namespace Codeflix.Catalog.Application.UseCases.CastMember.Delete;

public interface IDeleteCastMember : IRequestHandler<DeleteCastMemberInput>;
