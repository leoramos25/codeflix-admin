using Codeflix.Catalog.EndToEndTests.Api.CastMember.Common;

namespace Codeflix.Catalog.EndToEndTests.Api.CastMember.ListCastMembers;

[CollectionDefinition(nameof(ListCastMembersApiTestFixture))]
public class ListCastMembersApiTestFixtureCollection
    : ICollectionFixture<ListCastMembersApiTestFixture>;

public class ListCastMembersApiTestFixture : CastMemberBaseFixture;
