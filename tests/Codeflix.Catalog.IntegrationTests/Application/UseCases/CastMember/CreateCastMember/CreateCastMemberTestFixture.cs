using Codeflix.Catalog.Application.UseCases.CastMember.Create;
using Codeflix.Catalog.IntegrationTests.Application.UseCases.CastMember.Common;

namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.CastMember.CreateCastMember;

[CollectionDefinition(nameof(CreateCastMemberTestFixture))]
public class CreateCastMemberTestFixtureCollection
    : ICollectionFixture<CreateCastMemberTestFixture>;

public class CreateCastMemberTestFixture : CastMemberUseCaseBaseFixture
{
    public CreateCastMemberInput GetValidInput() => new(GetValidName(), GetRandomCastMemberType());
}
