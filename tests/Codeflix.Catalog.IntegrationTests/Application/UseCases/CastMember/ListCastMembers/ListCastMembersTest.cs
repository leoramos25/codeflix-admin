using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using Codeflix.Catalog.Infra.Data.EF.Repositories;
using FluentAssertions;
using UseCase = Codeflix.Catalog.Application.UseCases.CastMember.List;

namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.CastMember.ListCastMembers;

[Collection(nameof(ListCastMembersTestFixture))]
public class ListCastMembersTest(ListCastMembersTestFixture fixture)
{
    [Fact(DisplayName = nameof(ListCastMembers))]
    [Trait("Integration/Application", "ListCastMembers - Use Cases")]
    public async Task ListCastMembers()
    {
        var castMembers = fixture.GetValidCastMembers();
        var arrangeDbContext = fixture.CreateDbContext();
        await arrangeDbContext.CastMembers.AddRangeAsync(castMembers, CancellationToken.None);
        await arrangeDbContext.SaveChangesAsync(CancellationToken.None);
        var useCase = new UseCase.ListCastMembers(
            new CastMemberRepository(fixture.CreateDbContext(true))
        );
        var input = new UseCase.ListCastMembersInput(1, 20);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(castMembers.Count);
        output.Items.Should().HaveCount(castMembers.Count);
        output
            .Items.ToList()
            .ForEach(item =>
            {
                var castMember = castMembers.Find(castMember => castMember.Id == item.Id);
                castMember.Should().NotBeNull();
                item.Name.Should().Be(castMember!.Name);
                item.Type.Should().Be(castMember.Type);
                item.CreatedAt.Should().Be(castMember.CreatedAt);
            });
    }

    [Fact(DisplayName = nameof(ListCastMembersReturnsEmptyWhenPersistenceIsEmpty))]
    [Trait("Integration/Application", "ListCastMembers - Use Cases")]
    public async Task ListCastMembersReturnsEmptyWhenPersistenceIsEmpty()
    {
        var useCase = new UseCase.ListCastMembers(
            new CastMemberRepository(fixture.CreateDbContext())
        );
        var input = new UseCase.ListCastMembersInput(1, 20);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(input.Page);
        output.PerPage.Should().Be(input.PerPage);
        output.Total.Should().Be(0);
        output.Items.Should().BeEmpty();
    }

    [Theory(DisplayName = nameof(ListCastMembersPaginated))]
    [Trait("Integration/Application", "ListCastMembers - Use Cases")]
    [InlineData(10, 1, 5, 5)]
    [InlineData(10, 2, 5, 5)]
    [InlineData(7, 2, 5, 2)]
    [InlineData(7, 3, 5, 0)]
    public async Task ListCastMembersPaginated(
        int quantityToGenerate,
        int page,
        int perPage,
        int expectedItems
    )
    {
        var castMembers = fixture.GetValidCastMembers(quantityToGenerate);
        var arrangeDbContext = fixture.CreateDbContext();
        await arrangeDbContext.CastMembers.AddRangeAsync(castMembers, CancellationToken.None);
        await arrangeDbContext.SaveChangesAsync(CancellationToken.None);
        var useCase = new UseCase.ListCastMembers(
            new CastMemberRepository(fixture.CreateDbContext(true))
        );
        var input = new UseCase.ListCastMembersInput(page, perPage);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(page);
        output.PerPage.Should().Be(perPage);
        output.Total.Should().Be(quantityToGenerate);
        output.Items.Should().HaveCount(expectedItems);
        output
            .Items.ToList()
            .ForEach(item =>
            {
                var castMember = castMembers.Find(castMember => castMember.Id == item.Id);
                castMember.Should().NotBeNull();
                item.Name.Should().Be(castMember!.Name);
                item.Type.Should().Be(castMember.Type);
            });
    }

    [Theory(DisplayName = nameof(ListCastMembersOrdered))]
    [Trait("Integration/Application", "ListCastMembers - Use Cases")]
    [InlineData("name", "asc")]
    [InlineData("name", "desc")]
    [InlineData("type", "asc")]
    [InlineData("type", "desc")]
    [InlineData("createdAt", "asc")]
    [InlineData("createdAt", "desc")]
    [InlineData("", "asc")]
    public async Task ListCastMembersOrdered(string orderBy, string order)
    {
        var castMembers = fixture.GetValidCastMembers();
        var arrangeDbContext = fixture.CreateDbContext();
        await arrangeDbContext.CastMembers.AddRangeAsync(castMembers, CancellationToken.None);
        await arrangeDbContext.SaveChangesAsync(CancellationToken.None);
        var useCase = new UseCase.ListCastMembers(
            new CastMemberRepository(fixture.CreateDbContext(true))
        );
        var searchOrder = order == "asc" ? SearchOrder.Asc : SearchOrder.Desc;
        var input = new UseCase.ListCastMembersInput(1, 20, "", orderBy, searchOrder);

        var output = await useCase.Handle(input, CancellationToken.None);

        var expectedOrder = fixture.GetOrderedCastMembers(castMembers, orderBy, searchOrder);
        output.Should().NotBeNull();
        output.Total.Should().Be(castMembers.Count);
        output
            .Items.Select(item => item.Id)
            .Should()
            .Equal(expectedOrder.Select(castMember => castMember.Id));
    }
}
