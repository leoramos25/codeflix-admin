using MediatR;

namespace Codeflix.Catalog.Application.UseCases.CastMember.List;

public interface IListCastMembers : IRequestHandler<ListCastMembersInput, ListCastMembersOutput>;
