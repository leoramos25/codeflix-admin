using Codeflix.Catalog.Domain.Enum;
using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using Codeflix.Catalog.EndToEndTests.Common;
using DomainEntity = Codeflix.Catalog.Domain.Entity;

namespace Codeflix.Catalog.EndToEndTests.Api.CastMember.Common;

public class CastMemberBaseFixture : BaseFixture
{
    public CastMemberPersistence Persistence { get; }

    public CastMemberBaseFixture()
    {
        Persistence = new CastMemberPersistence(CreateDbContext());
    }

    public DomainEntity.CastMember GetValidCastMember() =>
        new(GetValidName(), GetRandomCastMemberType());

    public List<DomainEntity.CastMember> GetValidCastMembers(int size = 10) =>
        [.. Enumerable.Range(1, size).Select(_ => GetValidCastMember())];

    public string GetValidName() => Faker.Name.FullName();

    public CastMemberType GetRandomCastMemberType() => Faker.PickRandom<CastMemberType>();

    public List<DomainEntity.CastMember> GetValidCastMembersWithNames(List<string> names) =>
        [.. names.Select(name => new DomainEntity.CastMember(name, GetRandomCastMemberType()))];

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
        // MySQL stores the Guid as char(36), so the tie-breaker is a string comparison.
        return [.. orderedEnumerable.ThenBy(x => x.Id.ToString(), StringComparer.Ordinal)];
    }
}
