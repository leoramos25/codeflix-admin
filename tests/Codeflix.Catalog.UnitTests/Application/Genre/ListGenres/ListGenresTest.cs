using Codeflix.Catalog.Application.UseCases.Genre.List;
using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using FluentAssertions;
using Moq;

namespace Codeflix.Catalog.UnitTests.Application.Genre.ListGenres;

[Collection(nameof(ListGenresTestFixture))]
public class ListGenresTest(ListGenresTestFixture fixture)
{
    [Fact(DisplayName = nameof(ListGenres))]
    [Trait("Application", "List Genres - Use Case")]
    public async Task ListGenres()
    {
        var genreRepository = fixture.GetGenreRepository();
        var genres = fixture.GetValidGenres();
        var input = fixture.GetValidInput();
        var searchOutput = new SearchOutput<Catalog.Domain.Entity.Genre>(
            currentPage: input.Page,
            perPage: input.PerPage,
            items: genres,
            total: new Random().Next(50, 200)
        );
        genreRepository
            .Setup(repo => repo.Search(It.IsAny<SearchInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchOutput);
        var useCase = new Catalog.Application.UseCases.Genre.List.ListGenres(
            genreRepository.Object
        );

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(searchOutput.CurrentPage);
        output.PerPage.Should().Be(searchOutput.PerPage);
        output.Total.Should().Be(searchOutput.Total);
        output.Items.Should().HaveCount(searchOutput.Items.Count);
        ((List<ListGenresItemOutput>)output.Items).ForEach(item =>
        {
            var genre = genres.Find(x => x.Id == item.Id);
            genre.Should().NotBeNull();
            item.Name.Should().Be(genre.Name);
            item.IsActive.Should().Be(genre.IsActive);
            item.CreatedAt.Should().BeSameDateAs(genre.CreatedAt);
        });
        genreRepository.Verify(
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

    [Fact(DisplayName = nameof(ListEmpty))]
    [Trait("Application", "List Genres - Use Case")]
    public async Task ListEmpty()
    {
        var genreRepository = fixture.GetGenreRepository();
        var input = fixture.GetValidInput();
        var searchOutput = new SearchOutput<Catalog.Domain.Entity.Genre>(
            currentPage: input.Page,
            perPage: input.PerPage,
            items: [],
            total: 0
        );
        genreRepository
            .Setup(repo => repo.Search(It.IsAny<SearchInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchOutput);
        var useCase = new Catalog.Application.UseCases.Genre.List.ListGenres(
            genreRepository.Object
        );

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Page.Should().Be(searchOutput.CurrentPage);
        output.PerPage.Should().Be(searchOutput.PerPage);
        output.Total.Should().Be(searchOutput.Total);
        output.Items.Should().HaveCount(searchOutput.Items.Count);
        output.Items.Should().BeEmpty();
        genreRepository.Verify(
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

    [Fact(DisplayName = nameof(ListUsingInputDefaultValues))]
    [Trait("Application", "List Genres - Use Case")]
    public async Task ListUsingInputDefaultValues()
    {
        var genreRepository = fixture.GetGenreRepository();
        var input = new ListGenresInput();
        var genres = fixture.GetValidGenres(input.PerPage);
        var searchOutput = new SearchOutput<Catalog.Domain.Entity.Genre>(
            currentPage: input.Page,
            perPage: input.PerPage,
            items: genres,
            total: new Random().Next(50, 200)
        );
        genreRepository
            .Setup(repo => repo.Search(It.IsAny<SearchInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchOutput);
        var useCase = new Catalog.Application.UseCases.Genre.List.ListGenres(
            genreRepository.Object
        );

        await useCase.Handle(input, CancellationToken.None);

        genreRepository.Verify(
            repo =>
                repo.Search(
                    It.Is<SearchInput>(searchInput =>
                        searchInput.PerPage == 15
                        && searchInput.Page == 1
                        && searchInput.Search == ""
                        && searchInput.OrderBy == ""
                        && searchInput.Order == SearchOrder.Asc
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
