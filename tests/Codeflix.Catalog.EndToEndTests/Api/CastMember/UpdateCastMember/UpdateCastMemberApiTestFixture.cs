using Codeflix.Catalog.Api.ApiModels.CastMember;
using Codeflix.Catalog.EndToEndTests.Api.CastMember.Common;

namespace Codeflix.Catalog.EndToEndTests.Api.CastMember.UpdateCastMember;

[CollectionDefinition(nameof(UpdateCastMemberApiTestFixture))]
public class UpdateCastMemberApiTestFixtureCollection
    : ICollectionFixture<UpdateCastMemberApiTestFixture>;

public class UpdateCastMemberApiTestFixture : CastMemberBaseFixture
{
    public UpdateCastMemberApiInput GetValidInput() =>
        new(GetValidName(), GetRandomCastMemberType());
}
