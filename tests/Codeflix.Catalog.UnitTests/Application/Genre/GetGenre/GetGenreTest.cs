using Codeflix.Catalog.Application.Exceptions;
using Codeflix.Catalog.Application.UseCases.Genre.Get;
using FluentAssertions;
using Moq;

namespace Codeflix.Catalog.UnitTests.Application.Genre.GetGenre;

[Collection(nameof(GetGenreTestFixture))]
public class GetGenreTest(GetGenreTestFixture fixture)
{
    [Fact(DisplayName = nameof(GetGenre))]
    [Trait("Application", "Get Genre - Use Case")]
    public async Task GetGenre()
    {
        var genreRepository = fixture.GetGenreRepository();
        var categoryRepository = fixture.GetCategoryRepository();
        var categories = fixture.GetValidCategories(3);
        var genre = fixture.GetValidGenreWithCategories(
            [.. categories.Select(category => category.Id)]
        );
        genreRepository
            .Setup(repo => repo.Get(It.Is<Guid>(x => x == genre.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(genre);
        categoryRepository
            .Setup(repo => repo.ListByIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);
        var useCase = new Catalog.Application.UseCases.Genre.Get.GetGenre(
            genreRepository.Object,
            categoryRepository.Object
        );
        var input = new GetGenreInput(genre.Id);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().Be(input.Id);
        output.Name.Should().Be(genre.Name);
        output.IsActive.Should().Be(genre.IsActive);
        output.Categories.Should().HaveCount(genre.Categories.Count);
        output.Categories.Select(category => category.Id).Should().BeEquivalentTo(genre.Categories);
        output
            .Categories.ToList()
            .ForEach(categoryOutput =>
            {
                var category = categories.Find(category => category.Id == categoryOutput.Id);
                category.Should().NotBeNull();
                categoryOutput.Name.Should().Be(category.Name);
            });
        output.CreatedAt.Should().BeSameDateAs(genre.CreatedAt);
        genreRepository.Verify(
            repo => repo.Get(It.Is<Guid>(x => x == genre.Id), It.IsAny<CancellationToken>()),
            Times.Once
        );
        categoryRepository.Verify(
            repo =>
                repo.ListByIds(
                    It.Is<List<Guid>>(parameterIds =>
                        genre.Categories.All(parameterIds.Contains)
                        && genre.Categories.Count == parameterIds.Count
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact(DisplayName = nameof(GetGenreWithoutCategoriesShouldNotSearchCategories))]
    [Trait("Application", "Get Genre - Use Case")]
    public async Task GetGenreWithoutCategoriesShouldNotSearchCategories()
    {
        var genreRepository = fixture.GetGenreRepository();
        var categoryRepository = fixture.GetCategoryRepository();
        var genre = fixture.GetValidGenre();
        genreRepository
            .Setup(repo => repo.Get(It.Is<Guid>(x => x == genre.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(genre);
        var useCase = new Catalog.Application.UseCases.Genre.Get.GetGenre(
            genreRepository.Object,
            categoryRepository.Object
        );
        var input = new GetGenreInput(genre.Id);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Categories.Should().BeEmpty();
        categoryRepository.Verify(
            repo => repo.ListByIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact(DisplayName = nameof(GetShouldThrowExceptionWhenGenreNotFound))]
    [Trait("Application", "Get Genre - Use Case")]
    public async Task GetShouldThrowExceptionWhenGenreNotFound()
    {
        var invalidId = Guid.NewGuid();
        var genreRepository = fixture.GetGenreRepository();
        var categoryRepository = fixture.GetCategoryRepository();
        genreRepository
            .Setup(repo =>
                repo.Get(It.Is<Guid>(x => x == invalidId), It.IsAny<CancellationToken>())
            )
            .Throws(new NotFoundException("Genre not found"));
        var useCase = new Catalog.Application.UseCases.Genre.Get.GetGenre(
            genreRepository.Object,
            categoryRepository.Object
        );
        var input = new GetGenreInput(invalidId);

        var action = () => useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>().WithMessage("Genre not found");
        genreRepository.Verify(
            repo => repo.Get(It.Is<Guid>(x => x == invalidId), It.IsAny<CancellationToken>()),
            Times.Once
        );
        categoryRepository.Verify(
            repo => repo.ListByIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
