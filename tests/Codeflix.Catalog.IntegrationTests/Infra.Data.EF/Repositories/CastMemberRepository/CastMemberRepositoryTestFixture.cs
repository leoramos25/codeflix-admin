using Codeflix.Catalog.Domain.Enum;
using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using Codeflix.Catalog.IntegrationTests.Common;
using DomainEntity = Codeflix.Catalog.Domain.Entity;

namespace Codeflix.Catalog.IntegrationTests.Infra.Data.EF.Repositories.CastMemberRepository;

[CollectionDefinition(nameof(CastMemberRepositoryTestFixture))]
public class CastMemberRepositoryTestFixtureCollection
    : ICollectionFixture<CastMemberRepositoryTestFixture>;

public class CastMemberRepositoryTestFixture : BaseFixture
{
    public DomainEntity.CastMember GetValidCastMember() =>
        new(GetValidName(), GetRandomCastMemberType());

    public string GetValidName() => Faker.Name.FullName();

    public CastMemberType GetRandomCastMemberType() => Faker.PickRandom<CastMemberType>();

    internal List<DomainEntity.CastMember> GetValidCastMembers(int size = 10) =>
        Enumerable.Range(1, size).Select(_ => GetValidCastMember()).ToList();

    public List<DomainEntity.CastMember> GetOrderedCastMembers(
        List<DomainEntity.CastMember> castMembers,
        string orderBy,
        SearchOrder order
    )
    {
        var orderedEnumerable = (orderBy.ToLower(), order) switch
        {
            ("name", SearchOrder.Asc) => castMembers.OrderBy(x => x.Name),
            ("name", SearchOrder.Desc) => castMembers.OrderByDescending(x => x.Name),
            ("type", SearchOrder.Asc) => castMembers.OrderBy(x => x.Type),
            ("type", SearchOrder.Desc) => castMembers.OrderByDescending(x => x.Type),
            ("createdat", SearchOrder.Asc) => castMembers.OrderBy(x => x.CreatedAt),
            ("createdat", SearchOrder.Desc) => castMembers.OrderByDescending(x => x.CreatedAt),
            _ => castMembers.OrderBy(x => x.Name),
        };
        return orderedEnumerable.ThenBy(x => x.Id).ToList();
    }
};
