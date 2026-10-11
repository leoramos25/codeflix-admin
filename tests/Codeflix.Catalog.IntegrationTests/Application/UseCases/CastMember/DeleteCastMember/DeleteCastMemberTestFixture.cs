using Codeflix.Catalog.IntegrationTests.Application.UseCases.CastMember.Common;

namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.CastMember.DeleteCastMember;

[CollectionDefinition(nameof(DeleteCastMemberTestFixture))]
public class DeleteCastMemberTestFixtureCollection
    : ICollectionFixture<DeleteCastMemberTestFixture>;

public class DeleteCastMemberTestFixture : CastMemberUseCaseBaseFixture;
