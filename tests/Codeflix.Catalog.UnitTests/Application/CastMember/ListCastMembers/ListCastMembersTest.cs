using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using FluentAssertions;
using Moq;
using DomainEntity = Codeflix.Catalog.Domain.Entity;
using UseCases = Codeflix.Catalog.Application.UseCases.CastMember.List;

namespace Codeflix.Catalog.UnitTests.Application.CastMember.ListCastMembers;

[Collection(nameof(ListCastMembersTestFixture))]
public class ListCastMembersTest(ListCastMembersTestFixture fixture)
{
    [Fact(DisplayName = nameof(List))]
    [Trait("Application", "List Cast Members - Use Cases")]
    public async Task List()
    {
        var castMembers = fixture.GetValidCastMembers(20);
        var castMemberRepository = fixture.GetCastMemberRepositoryMock();
        var input = fixture.GetValidInput();
        var searchOutput = new SearchOutput<DomainEntity.CastMember>(
            input.Page,
            input.PerPage,
            castMembers.Count,
            castMembers
        );
        var useCase = new UseCases.ListCastMembers(castMemberRepository.Object);
        castMemberRepository
            .Setup(repo => repo.Search(It.IsAny<SearchInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchOutput);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(searchOutput.CurrentPage);
        output.PerPage.Should().Be(searchOutput.PerPage);
        output.Total.Should().Be(searchOutput.Total);
        output.Items.Should().HaveCount(searchOutput.Items.Count);
        output
            .Items.ToList()
            .ForEach(item =>
            {
                var castMember = castMembers.Find(x => x.Id == item.Id);
                castMember.Should().NotBeNull();
                item.Name.Should().Be(castMember.Name);
                item.Type.Should().Be(castMember.Type);
                item.CreatedAt.Should().BeSameDateAs(castMember.CreatedAt);
            });
        castMemberRepository.Verify(
            repo =>
                repo.Search(
                    It.Is<SearchInput>(searchInput =>
                        searchInput.Page == input.Page
                        && searchInput.PerPage == input.PerPage
                        && searchInput.Search == input.Search
                        && searchInput.OrderBy == input.Sort
                        && searchInput.Order == input.Dir
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact(DisplayName = nameof(EmptyList))]
    [Trait("Application", "List Cast Members - Use Cases")]
    public async Task EmptyList()
    {
        var castMemberRepository = fixture.GetCastMemberRepositoryMock();
        var input = fixture.GetValidInput();
        var searchOutput = new SearchOutput<DomainEntity.CastMember>(
            input.Page,
            input.PerPage,
            0,
            []
        );
        var useCase = new UseCases.ListCastMembers(castMemberRepository.Object);
        castMemberRepository
            .Setup(repo => repo.Search(It.IsAny<SearchInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchOutput);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(searchOutput.CurrentPage);
        output.PerPage.Should().Be(searchOutput.PerPage);
        output.Total.Should().Be(searchOutput.Total);
        output.Items.Should().BeEmpty();
        castMemberRepository.Verify(
            repo =>
                repo.Search(
                    It.Is<SearchInput>(searchInput =>
                        searchInput.Page == input.Page
                        && searchInput.PerPage == input.PerPage
                        && searchInput.Search == input.Search
                        && searchInput.OrderBy == input.Sort
                        && searchInput.Order == input.Dir
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact(DisplayName = nameof(ListWihDefaultValues))]
    [Trait("Application", "List Cast Members - Use Cases")]
    public async Task ListWihDefaultValues()
    {
        var castMembers = fixture.GetValidCastMembers(20);
        var castMemberRepository = fixture.GetCastMemberRepositoryMock();
        var input = new UseCases.ListCastMembersInput();
        var searchOutput = new SearchOutput<DomainEntity.CastMember>(
            input.Page,
            input.PerPage,
            castMembers.Count,
            castMembers.Take(input.PerPage).ToList()
        );
        var useCase = new UseCases.ListCastMembers(castMemberRepository.Object);
        castMemberRepository
            .Setup(repo => repo.Search(It.IsAny<SearchInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchOutput);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(searchOutput.CurrentPage);
        output.PerPage.Should().Be(searchOutput.PerPage);
        output.Total.Should().Be(searchOutput.Total);
        output.Items.Should().HaveCount(searchOutput.Items.Count);
        output
            .Items.ToList()
            .ForEach(item =>
            {
                var castMember = castMembers.Find(x => x.Id == item.Id);
                castMember.Should().NotBeNull();
                item.Name.Should().Be(castMember.Name);
                item.Type.Should().Be(castMember.Type);
                item.CreatedAt.Should().BeSameDateAs(castMember.CreatedAt);
            });
        castMemberRepository.Verify(
            repo =>
                repo.Search(
                    It.Is<SearchInput>(searchInput =>
                        searchInput.Page == input.Page
                        && searchInput.PerPage == input.PerPage
                        && searchInput.Search == input.Search
                        && searchInput.OrderBy == input.Sort
                        && searchInput.Order == input.Dir
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
