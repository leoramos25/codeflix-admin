using Codeflix.Catalog.Domain.Enum;
using Codeflix.Catalog.UnitTests.Common;
using DomainEntity = Codeflix.Catalog.Domain.Entity;

namespace Codeflix.Catalog.UnitTests.Domain.Entity.CastMember;

[CollectionDefinition(nameof(CastMemberTestFixture))]
public class CastMemberTestFixtureCollection : ICollectionFixture<CastMemberTestFixture>;

public class CastMemberTestFixture : BaseFixture
{
    public DomainEntity.CastMember GetValidCastMember() =>
        new(GetValidName(), GetRandomCastMemberType());

    public string GetValidName() => Faker.Name.FullName();

    public CastMemberType GetRandomCastMemberType()
    {
        var random = new Random();
        return (CastMemberType)random.Next(1, 2);
    }
};
