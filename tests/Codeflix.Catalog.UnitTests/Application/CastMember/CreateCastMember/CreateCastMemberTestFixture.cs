using Codeflix.Catalog.UnitTests.Application.CastMember.Common;

namespace Codeflix.Catalog.UnitTests.Application.CastMember.CreateCastMember;

[CollectionDefinition(nameof(CreateCastMemberTestFixture))]
public class CreateCastMemberTestFixtureCollection
    : ICollectionFixture<CreateCastMemberTestFixture>;

public class CreateCastMemberTestFixture : CastMemberUseCaseBaseFixture;
