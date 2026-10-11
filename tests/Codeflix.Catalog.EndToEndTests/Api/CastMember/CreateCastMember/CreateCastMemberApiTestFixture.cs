using Codeflix.Catalog.Api.ApiModels.CastMember;
using Codeflix.Catalog.EndToEndTests.Api.CastMember.Common;

namespace Codeflix.Catalog.EndToEndTests.Api.CastMember.CreateCastMember;

[CollectionDefinition(nameof(CreateCastMemberApiTestFixture))]
public class CreateCastMemberApiTestFixtureCollection
    : ICollectionFixture<CreateCastMemberApiTestFixture>;

public class CreateCastMemberApiTestFixture : CastMemberBaseFixture
{
    public CreateCastMemberApiInput GetValidInput() =>
        new(GetValidName(), GetRandomCastMemberType());
}
