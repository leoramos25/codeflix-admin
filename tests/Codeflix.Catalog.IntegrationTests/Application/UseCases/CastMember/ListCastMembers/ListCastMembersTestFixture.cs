using Codeflix.Catalog.IntegrationTests.Application.UseCases.CastMember.Common;

namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.CastMember.ListCastMembers;

[CollectionDefinition(nameof(ListCastMembersTestFixture))]
public class ListCastMembersTestFixtureCollection : ICollectionFixture<ListCastMembersTestFixture>;

public class ListCastMembersTestFixture : CastMemberUseCaseBaseFixture;
