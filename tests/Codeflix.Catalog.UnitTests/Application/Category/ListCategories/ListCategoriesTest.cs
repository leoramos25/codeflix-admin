using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using FluentAssertions;
using Moq;
using UseCases = Codeflix.Catalog.Application.UseCases.Category.List;

namespace Codeflix.Catalog.UnitTests.Application.Category.ListCategories;

[Collection(nameof(ListCategoriesTestFixture))]
public class ListCategoriesTest(ListCategoriesTestFixture fixture)
{
    [Fact(DisplayName = nameof(List))]
    [Trait("Application", "ListCategories - Use Cases")]
    public async Task List()
    {
        var repositoryMock = fixture.GetRepositoryMock();
        var input = fixture.GetValidInput();
        var categories = fixture.GetCategories();
        var outputSearch = new SearchOutput<Catalog.Domain.Entity.Category>(
            currentPage: input.Page,
            perPage: input.PerPage,
            items: categories,
            total: new Random().Next(50, 200)
        );
        repositoryMock
            .Setup(repo =>
                repo.Search(
                    It.Is<SearchInput>(searchInput =>
                        searchInput.PerPage == input.PerPage
                        && searchInput.Page == input.Page
                        && searchInput.Search == input.Search
                        && searchInput.OrderBy == input.Sort
                        && searchInput.Order == input.Dir
                    ),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(outputSearch);
        var useCase = new UseCases.ListCategories(repositoryMock.Object);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(outputSearch.CurrentPage);
        output.PerPage.Should().Be(outputSearch.PerPage);
        output.Total.Should().Be(outputSearch.Total);
        output.Items.Should().HaveCount(outputSearch.Items.Count);
        ((List<UseCases.ListCategoriesItemOutput>)output.Items).ForEach(item =>
        {
            var category = categories.Find(x => x.Id == item.Id);
            category.Should().NotBeNull();
            item.Name.Should().Be(category.Name);
            item.Description.Should().Be(category.Description);
            item.IsActive.Should().Be(category.IsActive);
            item.CreatedAt.Should().BeSameDateAs(category.CreatedAt);
        });
        repositoryMock.Verify(
            repo =>
                repo.Search(
                    It.Is<SearchInput>(searchInput =>
                        searchInput.PerPage == input.PerPage
                        && searchInput.Page == input.Page
                        && searchInput.Search == input.Search
                        && searchInput.OrderBy == input.Sort
                        && searchInput.Order == input.Dir
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Theory(DisplayName = nameof(ListInputWithoutAllParameters))]
    [Trait("Application", "ListCategories - Use Cases")]
    [MemberData(
        nameof(ListCategoriesTestDataGenerator.GetValidInputs),
        parameters: 15,
        MemberType = typeof(ListCategoriesTestDataGenerator)
    )]
    public async Task ListInputWithoutAllParameters(UseCases.ListCategoriesInput input)
    {
        var repositoryMock = fixture.GetRepositoryMock();
        var categories = fixture.GetCategories();
        var outputSearch = new SearchOutput<Catalog.Domain.Entity.Category>(
            currentPage: input.Page,
            perPage: input.PerPage,
            items: categories,
            total: new Random().Next(50, 200)
        );
        repositoryMock
            .Setup(repo =>
                repo.Search(
                    It.Is<SearchInput>(searchInput =>
                        searchInput.PerPage == input.PerPage
                        && searchInput.Page == input.Page
                        && searchInput.Search == input.Search
                        && searchInput.OrderBy == input.Sort
                        && searchInput.Order == input.Dir
                    ),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(outputSearch);
        var useCase = new UseCases.ListCategories(repositoryMock.Object);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(outputSearch.CurrentPage);
        output.PerPage.Should().Be(outputSearch.PerPage);
        output.Total.Should().Be(outputSearch.Total);
        output.Items.Should().HaveCount(outputSearch.Items.Count);
        ((List<UseCases.ListCategoriesItemOutput>)output.Items).ForEach(item =>
        {
            var category = categories.Find(x => x.Id == item.Id);
            category.Should().NotBeNull();
            item.Name.Should().Be(category.Name);
            item.Description.Should().Be(category.Description);
            item.IsActive.Should().Be(category.IsActive);
            item.CreatedAt.Should().BeSameDateAs(category.CreatedAt);
        });
        repositoryMock.Verify(
            repo =>
                repo.Search(
                    It.Is<SearchInput>(searchInput =>
                        searchInput.PerPage == input.PerPage
                        && searchInput.Page == input.Page
                        && searchInput.Search == input.Search
                        && searchInput.OrderBy == input.Sort
                        && searchInput.Order == input.Dir
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact(DisplayName = nameof(ListOkWhenEmpty))]
    [Trait("Application", "ListCategories - Use Cases")]
    public async Task ListOkWhenEmpty()
    {
        var repositoryMock = fixture.GetRepositoryMock();
        var input = fixture.GetValidInput();
        var outputSearch = new SearchOutput<Catalog.Domain.Entity.Category>(
            currentPage: input.Page,
            perPage: input.PerPage,
            items: new List<Catalog.Domain.Entity.Category>().AsReadOnly(),
            total: 0
        );
        repositoryMock
            .Setup(repo =>
                repo.Search(
                    It.Is<SearchInput>(searchInput =>
                        searchInput.PerPage == input.PerPage
                        && searchInput.Page == input.Page
                        && searchInput.Search == input.Search
                        && searchInput.OrderBy == input.Sort
                        && searchInput.Order == input.Dir
                    ),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(outputSearch);
        var useCase = new UseCases.ListCategories(repositoryMock.Object);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(outputSearch.CurrentPage);
        output.PerPage.Should().Be(outputSearch.PerPage);
        output.Total.Should().Be(0);
        output.Items.Should().HaveCount(0);
        repositoryMock.Verify(
            repo =>
                repo.Search(
                    It.Is<SearchInput>(searchInput =>
                        searchInput.PerPage == input.PerPage
                        && searchInput.Page == input.Page
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
