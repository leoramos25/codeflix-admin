using Codeflix.Catalog.UnitTests.Application.CastMember.Common;

namespace Codeflix.Catalog.UnitTests.Application.CastMember.GetCastMember;

[CollectionDefinition(nameof(GetCastMemberTestFixture))]
public class GetCastMemberTestFixtureCollection : ICollectionFixture<GetCastMemberTestFixture>;

public class GetCastMemberTestFixture : CastMemberUseCaseBaseFixture;
