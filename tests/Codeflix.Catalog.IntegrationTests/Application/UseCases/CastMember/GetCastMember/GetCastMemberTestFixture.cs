using Codeflix.Catalog.IntegrationTests.Application.UseCases.CastMember.Common;

namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.CastMember.GetCastMember;

[CollectionDefinition(nameof(GetCastMemberTestFixture))]
public class GetCastMemberTestFixtureCollection : ICollectionFixture<GetCastMemberTestFixture>;

public class GetCastMemberTestFixture : CastMemberUseCaseBaseFixture;
