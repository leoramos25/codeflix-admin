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
        var genre = fixture.GetValidGenreWithCategories(3);
        genreRepository
            .Setup(repo => repo.Get(It.Is<Guid>(x => x == genre.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(genre);
        var useCase = new Catalog.Application.UseCases.Genre.Get.GetGenre(genreRepository.Object);
        var input = new GetGenreInput(genre.Id);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().Be(input.Id);
        output.Name.Should().Be(genre.Name);
        output.IsActive.Should().Be(genre.IsActive);
        output.Categories.Should().HaveCount(genre.Categories.Count);
        output.Categories.Should().BeEquivalentTo(genre.Categories);
        output.CreatedAt.Should().BeSameDateAs(genre.CreatedAt);
        genreRepository.Verify(
            repo => repo.Get(It.Is<Guid>(x => x == genre.Id), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact(DisplayName = nameof(GetShouldThrowExceptionWhenGenreNotFound))]
    [Trait("Application", "Get Genre - Use Case")]
    public async Task GetShouldThrowExceptionWhenGenreNotFound()
    {
        var invalidId = Guid.NewGuid();
        var genreRepository = fixture.GetGenreRepository();
        genreRepository
            .Setup(repo =>
                repo.Get(It.Is<Guid>(x => x == invalidId), It.IsAny<CancellationToken>())
            )
            .Throws(new NotFoundException("Genre not found"));
        var useCase = new Catalog.Application.UseCases.Genre.Get.GetGenre(genreRepository.Object);
        var input = new GetGenreInput(invalidId);

        var action = () => useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>().WithMessage("Genre not found");
        genreRepository.Verify(
            repo => repo.Get(It.Is<Guid>(x => x == invalidId), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
