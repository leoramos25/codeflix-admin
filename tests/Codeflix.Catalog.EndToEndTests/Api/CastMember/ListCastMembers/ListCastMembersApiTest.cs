using System.Net;
using Codeflix.Catalog.Application.UseCases.CastMember.List;
using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using Codeflix.Catalog.EndToEndTests.Models;
using FluentAssertions;

namespace Codeflix.Catalog.EndToEndTests.Api.CastMember.ListCastMembers;

[Collection(nameof(ListCastMembersApiTestFixture))]
public class ListCastMembersApiTest(ListCastMembersApiTestFixture fixture) : IDisposable
{
    public void Dispose()
    {
        fixture.CleanPersistence();
    }

    [Fact(DisplayName = nameof(ListCastMembersAndTotalWithDefault))]
    [Trait("EndToEnd/Api", "ListCastMembers - Endpoints")]
    public async Task ListCastMembersAndTotalWithDefault()
    {
        const int defaultPerPage = 15;
        var castMembers = fixture.GetValidCastMembers(20);
        await fixture.Persistence.InsertList(castMembers);

        var (response, output) = await fixture.ApiClient.Get<
            TestApiResponseList<ListCastMembersItemOutput>
        >("/castmembers", CancellationToken.None);

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Data.Should().HaveCount(defaultPerPage);
        output.Meta!.Total.Should().Be(castMembers.Count);
        output.Meta.CurrentPage.Should().Be(1);
        output.Meta.PerPage.Should().Be(defaultPerPage);
        foreach (var item in output.Data!)
        {
            var expectedItem = castMembers.Find(castMember => castMember.Id == item.Id);
            expectedItem.Should().NotBeNull();
            item.Name.Should().Be(expectedItem!.Name);
            item.Type.Should().Be(expectedItem.Type);
            item.CreatedAt.Should().BeSameDateAs(expectedItem.CreatedAt);
        }
    }

    [Theory(DisplayName = nameof(ListPaginated))]
    [Trait("EndToEnd/Api", "ListCastMembers - Endpoints")]
    [InlineData(10, 1, 5, 5)]
    [InlineData(10, 2, 5, 5)]
    [InlineData(7, 2, 5, 2)]
    [InlineData(7, 3, 5, 0)]
    public async Task ListPaginated(
        int quantityToGenerate,
        int page,
        int perPage,
        int expectedQuantityItems
    )
    {
        var castMembers = fixture.GetValidCastMembers(quantityToGenerate);
        await fixture.Persistence.InsertList(castMembers);
        var input = new ListCastMembersInput(page, perPage);

        var (response, output) = await fixture.ApiClient.Get<
            TestApiResponseList<ListCastMembersItemOutput>
        >("/castmembers", CancellationToken.None, input);

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Data.Should().HaveCount(expectedQuantityItems);
        output.Meta!.Total.Should().Be(quantityToGenerate);
        output.Meta.CurrentPage.Should().Be(page);
        output.Meta.PerPage.Should().Be(perPage);
        foreach (var item in output.Data!)
        {
            var expectedItem = castMembers.Find(castMember => castMember.Id == item.Id);
            expectedItem.Should().NotBeNull();
            item.Name.Should().Be(expectedItem!.Name);
            item.Type.Should().Be(expectedItem.Type);
        }
    }

    [Theory(DisplayName = nameof(ListOrdered))]
    [Trait("EndToEnd/Api", "ListCastMembers - Endpoints")]
    [InlineData("name", "asc")]
    [InlineData("name", "desc")]
    [InlineData("type", "asc")]
    [InlineData("type", "desc")]
    [InlineData("createdAt", "asc")]
    [InlineData("createdAt", "desc")]
    public async Task ListOrdered(string orderBy, string order)
    {
        var searchOrder = order == "asc" ? SearchOrder.Asc : SearchOrder.Desc;
        var input = new ListCastMembersInput(1, 20, "", orderBy, searchOrder);
        var castMembers = fixture.GetValidCastMembersWithNames([
            "Meryl Streep",
            "Denzel Washington",
            "Cate Blanchett",
            "Tom Hanks",
            "Viola Davis",
            "Christopher Nolan",
            "Greta Gerwig",
            "Anthony Hopkins",
            "Kathryn Bigelow",
            "Bong Joon-ho",
        ]);
        await fixture.Persistence.InsertList(castMembers);

        var (response, output) = await fixture.ApiClient.Get<
            TestApiResponseList<ListCastMembersItemOutput>
        >("/castmembers", CancellationToken.None, input);

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Meta!.Total.Should().Be(castMembers.Count);
        // MySQL keeps CreatedAt at microsecond precision (.NET uses 100ns ticks), so members
        // created within the same microsecond tie in the database and fall back to the Id
        // tie-breaker. Order what was actually persisted, not the in-memory instances.
        var persistedCastMembers = await fixture.Persistence.GetAll();
        var expectedList = fixture.GetOrderedCastMembers(
            persistedCastMembers,
            orderBy,
            searchOrder
        );
        output
            .Data!.Select(item => item.Id)
            .Should()
            .Equal(expectedList.Select(castMember => castMember.Id));
    }

    [Theory(DisplayName = nameof(SearchByText))]
    [Trait("EndToEnd/Api", "ListCastMembers - Endpoints")]
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
        var castMembers = fixture.GetValidCastMembersWithNames([
            "Tom Hanks",
            "Tom Cruise",
            "Tom Holland",
            "Viola Davis",
            "Geena Davis",
            "Meryl Streep",
            "Greta Gerwig",
        ]);
        await fixture.Persistence.InsertList(castMembers);
        var input = new ListCastMembersInput(page, perPage, search);

        var (response, output) = await fixture.ApiClient.Get<
            TestApiResponseList<ListCastMembersItemOutput>
        >("/castmembers", CancellationToken.None, input);

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        output.Should().NotBeNull();
        output!.Meta!.Total.Should().Be(expectedQuantityTotalItems);
        output.Meta.CurrentPage.Should().Be(page);
        output.Meta.PerPage.Should().Be(perPage);
        output.Data.Should().HaveCount(expectedQuantityItemsReturned);
        output.Data.Should().OnlyContain(item => item.Name.Contains(search));
    }
}
