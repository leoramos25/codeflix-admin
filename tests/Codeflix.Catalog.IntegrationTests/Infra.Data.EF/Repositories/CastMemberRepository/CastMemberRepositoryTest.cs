using Codeflix.Catalog.Application.Exceptions;
using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Context = Codeflix.Catalog.Infra.Data.EF.Repositories;

namespace Codeflix.Catalog.IntegrationTests.Infra.Data.EF.Repositories.CastMemberRepository;

[Collection(nameof(CastMemberRepositoryTestFixture))]
public class CastMemberRepositoryTest(CastMemberRepositoryTestFixture fixture)
{
    [Fact(DisplayName = nameof(Insert))]
    [Trait("Integration/Infra.Data", "CastMemberRepository - Repositories")]
    public async Task Insert()
    {
        var castMember = fixture.GetValidCastMember();
        var dbContext = fixture.CreateDbContext();
        var repository = new Context.CastMemberRepository(dbContext);

        await repository.Insert(castMember, CancellationToken.None);
        await dbContext.SaveChangesAsync();

        var assertDbContext = fixture.CreateDbContext(true);
        Domain.Entity.CastMember? createdCastMember = await assertDbContext
            .CastMembers.AsNoTracking()
            .FirstOrDefaultAsync(cm => cm.Id == castMember.Id);
        createdCastMember.Should().NotBeNull();
        createdCastMember.Name.Should().Be(castMember.Name);
        createdCastMember.Type.Should().Be(castMember.Type);
        createdCastMember.CreatedAt.Should().BeSameDateAs(castMember.CreatedAt);
    }

    [Fact(DisplayName = nameof(Get))]
    [Trait("Integration/Infra.Data", "CastMemberRepository - Repositories")]
    public async Task Get()
    {
        var castMember = fixture.GetValidCastMember();
        var dbContext = fixture.CreateDbContext();
        await dbContext.CastMembers.AddAsync(castMember, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var repository = new Context.CastMemberRepository(dbContext);

        var persistedCastMember = await repository.Get(castMember.Id, CancellationToken.None);

        persistedCastMember.Should().NotBeNull();
        persistedCastMember.Name.Should().Be(castMember.Name);
        persistedCastMember.Type.Should().Be(castMember.Type);
        persistedCastMember.CreatedAt.Should().BeSameDateAs(castMember.CreatedAt);
    }

    [Fact(DisplayName = nameof(GetThrowIfNull))]
    [Trait("Integration/Infra.Data", "CastMemberRepository - Repositories")]
    public async Task GetThrowIfNull()
    {
        var dbContext = fixture.CreateDbContext();
        var repository = new Context.CastMemberRepository(dbContext);
        var castMemberInvalidId = Guid.NewGuid();

        var action = () => repository.Get(castMemberInvalidId, CancellationToken.None);

        await action
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"CastMember {castMemberInvalidId} not found.");
    }

    [Fact(DisplayName = nameof(Delete))]
    [Trait("Integration/Infra.Data", "CastMemberRepository - Repositories")]
    public async Task Delete()
    {
        var castMember = fixture.GetValidCastMember();
        var dbContext = fixture.CreateDbContext();
        await dbContext.CastMembers.AddAsync(castMember, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var repository = new Context.CastMemberRepository(dbContext);

        await repository.Delete(castMember, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var assertDbContext = fixture.CreateDbContext(true);
        var deletedCastMember = await assertDbContext.CastMembers.FindAsync(
            castMember.Id,
            CancellationToken.None
        );
        deletedCastMember.Should().BeNull();
    }

    [Fact(DisplayName = nameof(Update))]
    [Trait("Integration/Infra.Data", "CastMemberRepository - Repositories")]
    public async Task Update()
    {
        var castMember = fixture.GetValidCastMember();
        var data = new { Name = fixture.GetValidName(), Type = fixture.GetRandomCastMemberType() };
        var dbContext = fixture.CreateDbContext();
        await dbContext.CastMembers.AddAsync(castMember, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var repository = new Context.CastMemberRepository(dbContext);
        castMember.Update(data.Name, data.Type);

        await repository.Update(castMember, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var updatedCastMember = await fixture
            .CreateDbContext(true)
            .CastMembers.FindAsync(castMember.Id, CancellationToken.None);
        updatedCastMember.Should().NotBeNull();
        updatedCastMember.Name.Should().Be(data.Name);
        updatedCastMember.Type.Should().Be(data.Type);
        updatedCastMember.CreatedAt.Should().BeSameDateAs(castMember.CreatedAt);
    }

    [Fact(DisplayName = nameof(Search))]
    [Trait("Integration/Infra.Data", "CastMemberRepository - Repositories")]
    public async Task Search()
    {
        var dbContext = fixture.CreateDbContext();
        var castMembers = fixture.GetValidCastMembers();
        await dbContext.CastMembers.AddRangeAsync(castMembers, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var repository = new Context.CastMemberRepository(dbContext);
        var searchInput = new SearchInput(1, 20, "", "", SearchOrder.Asc);

        var output = await repository.Search(searchInput, CancellationToken.None);

        output.Should().NotBeNull();
        output.Items.Should().NotBeNull();
        output.CurrentPage.Should().Be(searchInput.Page);
        output.Total.Should().Be(castMembers.Count);
        foreach (var item in output.Items)
        {
            var castMember = castMembers.Find(cm => cm.Id == item.Id);
            castMember.Should().NotBeNull();
            item.Name.Should().Be(castMember.Name);
            item.Type.Should().Be(castMember.Type);
            item.CreatedAt.Should().BeSameDateAs(castMember.CreatedAt);
        }
    }

    [Theory(DisplayName = nameof(SearchOrdered))]
    [Trait("Integration/Infra.Data", "CastMemberRepository - Repositories")]
    [InlineData("name", "asc")]
    [InlineData("name", "desc")]
    [InlineData("type", "asc")]
    [InlineData("type", "desc")]
    [InlineData("createdAt", "asc")]
    [InlineData("createdAt", "desc")]
    public async Task SearchOrdered(string orderBy, string order)
    {
        var dbContext = fixture.CreateDbContext();
        var castMembers = fixture.GetValidCastMembers();
        await dbContext.CastMembers.AddRangeAsync(castMembers, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var repository = new Context.CastMemberRepository(fixture.CreateDbContext(true));
        var searchOrder = order == "asc" ? SearchOrder.Asc : SearchOrder.Desc;
        var searchInput = new SearchInput(1, 20, "", orderBy, searchOrder);

        var output = await repository.Search(searchInput, CancellationToken.None);

        var expectedList = fixture.GetOrderedCastMembers(castMembers, orderBy, searchOrder);
        output.Should().NotBeNull();
        output.Total.Should().Be(castMembers.Count);
        output.Items.Should().HaveCount(expectedList.Count);
        for (var index = 0; index < expectedList.Count; index++)
        {
            var expectedItem = expectedList[index];
            var outputItem = output.Items[index];
            outputItem.Id.Should().Be(expectedItem.Id);
            outputItem.Name.Should().Be(expectedItem.Name);
            outputItem.Type.Should().Be(expectedItem.Type);
            outputItem.CreatedAt.Should().BeSameDateAs(expectedItem.CreatedAt);
        }
    }

    [Theory(DisplayName = nameof(SearchByText))]
    [Trait("Integration/Infra.Data", "CastMemberRepository - Repositories")]
    [InlineData("Tom", 1, 5, 3, 3)]
    [InlineData("Davis", 1, 5, 2, 2)]
    [InlineData("Hanks", 1, 5, 1, 1)]
    [InlineData("Tom", 2, 2, 1, 3)]
    [InlineData("Tom Cruise", 1, 5, 1, 1)]
    [InlineData("Nonexistent", 1, 5, 0, 0)]
    public async Task SearchByText(
        string search,
        int page,
        int perPage,
        int expectedQuantityItemsReturned,
        int expectedQuantityTotalItems
    )
    {
        var dbContext = fixture.CreateDbContext();
        var castMembers = fixture.GetValidCastMembersWithNames([
            "Tom Hanks",
            "Tom Cruise",
            "Tom Holland",
            "Viola Davis",
            "Geena Davis",
            "Meryl Streep",
            "Greta Gerwig",
        ]);
        await dbContext.CastMembers.AddRangeAsync(castMembers, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var repository = new Context.CastMemberRepository(fixture.CreateDbContext(true));
        var searchInput = new SearchInput(page, perPage, search, "", SearchOrder.Asc);

        var output = await repository.Search(searchInput, CancellationToken.None);

        output.Should().NotBeNull();
        output.CurrentPage.Should().Be(page);
        output.PerPage.Should().Be(perPage);
        output.Total.Should().Be(expectedQuantityTotalItems);
        output.Items.Should().HaveCount(expectedQuantityItemsReturned);
        output.Items.Should().OnlyContain(item => item.Name.Contains(search));
    }
}
