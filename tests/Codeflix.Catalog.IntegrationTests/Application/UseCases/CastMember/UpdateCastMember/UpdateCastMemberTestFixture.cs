using Codeflix.Catalog.IntegrationTests.Application.UseCases.CastMember.Common;

namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.CastMember.UpdateCastMember;

[CollectionDefinition(nameof(UpdateCastMemberTestFixture))]
public class UpdateCastMemberTestFixtureCollection
    : ICollectionFixture<UpdateCastMemberTestFixture>;

public class UpdateCastMemberTestFixture : CastMemberUseCaseBaseFixture;
