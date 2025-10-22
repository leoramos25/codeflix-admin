using Codeflix.Catalog.Application.Exceptions;
using Codeflix.Catalog.Application.UseCases.Genre.Delete;
using FluentAssertions;
using Moq;

namespace Codeflix.Catalog.UnitTests.Application.Genre.DeleteGenre;

[Collection(nameof(DeleteGenreTestFixture))]
public class DeleteGenreTest(DeleteGenreTestFixture fixture)
{
    [Fact(DisplayName = nameof(DeleteGenre))]
    [Trait("Application", "Delete Genre - Use Case")]
    public async Task DeleteGenre()
    {
        var genre = fixture.GetValidGenreWithCategories(3);
        var genreRepository = fixture.GetGenreRepository();
        genreRepository
            .Setup(repo => repo.Get(It.Is<Guid>(x => x == genre.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(genre);
        var unitOfWork = fixture.GetUnitOfWork();
        var useCase = new Catalog.Application.UseCases.Genre.Delete.DeleteGenre(
            genreRepository.Object,
            unitOfWork.Object
        );
        var input = new DeleteGenreInput(genre.Id);

        await useCase.Handle(input, CancellationToken.None);

        genreRepository.Verify(
            repo => repo.Get(It.Is<Guid>(x => x == genre.Id), It.IsAny<CancellationToken>()),
            Times.Once
        );
        genreRepository.Verify(
            repo => repo.Delete(genre, It.IsAny<CancellationToken>()),
            Times.Once
        );
        unitOfWork.Verify(uow => uow.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = nameof(DeleteGenreShouldThrowExceptionWhenNotFound))]
    [Trait("Application", "Delete Genre - Use Case")]
    public async Task DeleteGenreShouldThrowExceptionWhenNotFound()
    {
        var invalidId = Guid.NewGuid();
        var genreRepository = fixture.GetGenreRepository();
        genreRepository
            .Setup(repo =>
                repo.Get(It.Is<Guid>(x => x == invalidId), It.IsAny<CancellationToken>())
            )
            .ThrowsAsync(new NotFoundException("Genre not found"));
        var unitOfWork = fixture.GetUnitOfWork();
        var input = new DeleteGenreInput(invalidId);
        var useCase = new Catalog.Application.UseCases.Genre.Delete.DeleteGenre(
            genreRepository.Object,
            unitOfWork.Object
        );

        var action = () => useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>().WithMessage("Genre not found");
        genreRepository.Verify(
            repo =>
                repo.Delete(It.IsAny<Catalog.Domain.Entity.Genre>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        unitOfWork.Verify(uow => uow.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }
}
