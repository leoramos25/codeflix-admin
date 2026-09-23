using Codeflix.Catalog.UnitTests.Application.CastMember.Common;

namespace Codeflix.Catalog.UnitTests.Application.CastMember.UpdateCastMember;

[CollectionDefinition(nameof(UpdateCastMemberTestFixture))]
public class UpdateCastMemberTestFixtureCollection
    : ICollectionFixture<UpdateCastMemberTestFixture>;

public class UpdateCastMemberTestFixture : CastMemberUseCaseBaseFixture;
