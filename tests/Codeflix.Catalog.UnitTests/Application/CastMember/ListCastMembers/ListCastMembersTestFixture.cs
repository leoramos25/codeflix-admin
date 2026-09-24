using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using Codeflix.Catalog.UnitTests.Application.CastMember.Common;
using DomainEntity = Codeflix.Catalog.Domain.Entity;
using UseCases = Codeflix.Catalog.Application.UseCases.CastMember.List;

namespace Codeflix.Catalog.UnitTests.Application.CastMember.ListCastMembers;

[CollectionDefinition(nameof(ListCastMembersTestFixture))]
public class ListCastMembersTestFixtureCollection : ICollectionFixture<ListCastMembersTestFixture>;

public class ListCastMembersTestFixture : CastMemberUseCaseBaseFixture
{
    public List<DomainEntity.CastMember> GetValidCastMembers(int size = 10) =>
        Enumerable.Range(1, size).Select(_ => GetValidCastMember()).ToList();

    public UseCases.ListCastMembersInput GetValidInput()
    {
        var random = new Random();
        return new UseCases.ListCastMembersInput(
            page: random.Next(1, 100),
            perPage: random.Next(1, 50),
            search: Faker.Name.FirstName(),
            sort: nameof(Faker.Name),
            Faker.PickRandom(SearchOrder.Asc, SearchOrder.Desc)
        );
    }
};
