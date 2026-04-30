using Codeflix.Catalog.Application.Exceptions;
using Codeflix.Catalog.Application.UseCases.Genre.Update;
using Codeflix.Catalog.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace Codeflix.Catalog.UnitTests.Application.Genre.UpdateGenre;

[Collection(nameof(UpdateGenreTestFixture))]
public class UpdateGenreTest(UpdateGenreTestFixture fixture)
{
    [Fact(DisplayName = nameof(Update))]
    [Trait("Genre", "Update Genre - Use Case")]
    public async Task Update()
    {
        var genreRepository = fixture.GetGenreRepository();
        var unitOfWork = fixture.GetUnitOfWork();
        var categoryRepository = fixture.GetCategoryRepository();
        var genre = fixture.GetValidGenre();
        var input = fixture.GetValidInput(genre.Id, !genre.IsActive);
        genreRepository
            .Setup(repo => repo.Get(genre.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(genre);
        var useCase = new Catalog.Application.UseCases.Genre.Update.UpdateGenre(
            genreRepository.Object,
            unitOfWork.Object,
            categoryRepository.Object
        );

        var output = await useCase.Handle(input, CancellationToken.None);

        genreRepository.Verify(
            repo => repo.Get(genre.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        genreRepository.Verify(
            repo =>
                repo.Update(It.IsAny<Catalog.Domain.Entity.Genre>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        unitOfWork.Verify(uow => uow.Commit(It.IsAny<CancellationToken>()), Times.Once);
        categoryRepository.Verify(
            repo => repo.ListIdsByIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        output.Id.Should().Be(input.Id);
        output.Name.Should().Be(input.Name);
        output.IsActive.Should().Be(input.IsActive!.Value);
        output.CreatedAt.Should().NotBeSameDateAs(default);
    }

    [Fact(DisplayName = nameof(UpdateOnlyName))]
    [Trait("Genre", "Update Genre - Use Case")]
    public async Task UpdateOnlyName()
    {
        var genreRepository = fixture.GetGenreRepository();
        var unitOfWork = fixture.GetUnitOfWork();
        var categoryRepository = fixture.GetCategoryRepository();
        var genre = fixture.GetValidGenre();
        var input = fixture.GetValidInput(genre.Id);
        genreRepository
            .Setup(repo => repo.Get(genre.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(genre);
        var useCase = new Catalog.Application.UseCases.Genre.Update.UpdateGenre(
            genreRepository.Object,
            unitOfWork.Object,
            categoryRepository.Object
        );

        var output = await useCase.Handle(input, CancellationToken.None);

        genreRepository.Verify(
            repo => repo.Get(genre.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        genreRepository.Verify(
            repo =>
                repo.Update(It.IsAny<Catalog.Domain.Entity.Genre>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        categoryRepository.Verify(
            repo => repo.ListIdsByIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        unitOfWork.Verify(uow => uow.Commit(It.IsAny<CancellationToken>()), Times.Once);
        output.Id.Should().Be(input.Id);
        output.Name.Should().Be(input.Name);
        output.IsActive.Should().Be(genre.IsActive);
        output.CreatedAt.Should().NotBeSameDateAs(default);
    }

    [Theory(DisplayName = nameof(UpdateShouldThrowExceptionWhenNameIsInvalid))]
    [Trait("Genre", "Update Genre - Use Case")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task UpdateShouldThrowExceptionWhenNameIsInvalid(string invalidName)
    {
        var genre = fixture.GetValidGenre();
        var input = new UpdateGenreInput(genre.Id, invalidName);
        var genreRepository = fixture.GetGenreRepository();
        var unitOfWork = fixture.GetUnitOfWork();
        var categoryRepository = fixture.GetCategoryRepository();
        genreRepository
            .Setup(repo => repo.Get(genre.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(genre);
        var useCase = new Catalog.Application.UseCases.Genre.Update.UpdateGenre(
            genreRepository.Object,
            unitOfWork.Object,
            categoryRepository.Object
        );

        var action = () => useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<EntityValidationException>();

        genreRepository.Verify(
            repo => repo.Get(genre.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        genreRepository.Verify(
            repo =>
                repo.Update(It.IsAny<Catalog.Domain.Entity.Genre>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        unitOfWork.Verify(uow => uow.Commit(It.IsAny<CancellationToken>()), Times.Never);
        categoryRepository.Verify(
            repo => repo.ListIdsByIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact(DisplayName = nameof(UpdateShouldThrowWhenGenreNotFound))]
    [Trait("Genre", "Update Genre - Use Case")]
    public async Task UpdateShouldThrowWhenGenreNotFound()
    {
        var genre = fixture.GetValidGenre();
        var input = fixture.GetValidInput(genre.Id);
        var genreRepository = fixture.GetGenreRepository();
        var unitOfWork = fixture.GetUnitOfWork();
        var categoryRepository = fixture.GetCategoryRepository();
        genreRepository
            .Setup(repo => repo.Get(genre.Id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Genre '{genre.Id}' not found"));
        var useCase = new Catalog.Application.UseCases.Genre.Update.UpdateGenre(
            genreRepository.Object,
            unitOfWork.Object,
            categoryRepository.Object
        );

        var action = () => useCase.Handle(input, CancellationToken.None);

        await action
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"Genre '{genre.Id}' not found");

        genreRepository.Verify(
            repo => repo.Get(genre.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        genreRepository.Verify(
            repo =>
                repo.Update(It.IsAny<Catalog.Domain.Entity.Genre>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        unitOfWork.Verify(uow => uow.Commit(It.IsAny<CancellationToken>()), Times.Never);
        categoryRepository.Verify(
            repo => repo.ListIdsByIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact(DisplayName = nameof(UpdateAddingRelatedCategories))]
    [Trait("Genre", "Update Genre - Use Case")]
    public async Task UpdateAddingRelatedCategories()
    {
        var genre = fixture.GetValidGenre();
        var categoryIds = Enumerable.Range(1, 3).Select(_ => Guid.NewGuid()).ToList();
        var input = fixture.GetValidInput(genre.Id, categories: categoryIds);
        var genreRepository = fixture.GetGenreRepository();
        var unitOfWork = fixture.GetUnitOfWork();
        var categoryRepository = fixture.GetCategoryRepository();
        categoryRepository
            .Setup(repo => repo.ListIdsByIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(categoryIds.AsReadOnly());
        genreRepository
            .Setup(repo => repo.Get(genre.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(genre);
        var useCase = new Catalog.Application.UseCases.Genre.Update.UpdateGenre(
            genreRepository.Object,
            unitOfWork.Object,
            categoryRepository.Object
        );

        var output = await useCase.Handle(input, CancellationToken.None);

        genreRepository.Verify(
            repo => repo.Get(genre.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        genreRepository.Verify(
            repo =>
                repo.Update(It.IsAny<Catalog.Domain.Entity.Genre>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        unitOfWork.Verify(uow => uow.Commit(It.IsAny<CancellationToken>()), Times.Once);
        categoryRepository.Verify(
            repo => repo.ListIdsByIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        output.Id.Should().Be(input.Id);
        output.Name.Should().Be(input.Name);
        output.IsActive.Should().Be(genre.IsActive);
        output.Categories.Should().HaveCount(categoryIds.Count);
        output.Categories.Should().BeEquivalentTo(categoryIds);
        output.CreatedAt.Should().NotBeSameDateAs(default);
    }

    [Fact(DisplayName = nameof(UpdateReplacingRelatedCategories))]
    [Trait("Genre", "Update Genre - Use Case")]
    public async Task UpdateReplacingRelatedCategories()
    {
        var genre = fixture.GetValidGenre();
        var genreOldCategories = Enumerable.Range(1, 3).Select(_ => Guid.NewGuid()).ToList();
        genreOldCategories.ForEach(genre.AddCategory);
        var input = fixture.GetValidInputWithCategories(genre.Id);
        var genreRepository = fixture.GetGenreRepository();
        var unitOfWork = fixture.GetUnitOfWork();
        var categoryRepository = fixture.GetCategoryRepository();
        genreRepository
            .Setup(repo => repo.Get(genre.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(genre);
        categoryRepository
            .Setup(repo => repo.ListIdsByIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(input.Categories!.AsReadOnly());
        var useCase = new Catalog.Application.UseCases.Genre.Update.UpdateGenre(
            genreRepository.Object,
            unitOfWork.Object,
            categoryRepository.Object
        );

        var output = await useCase.Handle(input, CancellationToken.None);

        genreRepository.Verify(
            repo => repo.Get(genre.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        genreRepository.Verify(
            repo =>
                repo.Update(It.IsAny<Catalog.Domain.Entity.Genre>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        unitOfWork.Verify(uow => uow.Commit(It.IsAny<CancellationToken>()), Times.Once);
        categoryRepository.Verify(
            repo => repo.ListIdsByIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        output.Id.Should().Be(input.Id);
        output.Name.Should().Be(input.Name);
        output.IsActive.Should().Be(genre.IsActive);
        output.Categories.Should().HaveCount(input.Categories.Count);
        output.Categories.Should().BeEquivalentTo(input.Categories);
        output.CreatedAt.Should().NotBeSameDateAs(default);
    }

    [Fact(DisplayName = nameof(UpdateShouldThrowWhenCategoryNotFound))]
    [Trait("Genre", "Update Genre - Use Case")]
    public async Task UpdateShouldThrowWhenCategoryNotFound()
    {
        var genre = fixture.GetValidGenre();
        var genreOldCategories = Enumerable.Range(1, 3).Select(_ => Guid.NewGuid()).ToList();
        genreOldCategories.ForEach(genre.AddCategory);
        var input = fixture.GetValidInput(
            genre.Id,
            categories: Enumerable.Range(5, 15).Select(_ => Guid.NewGuid()).ToList()
        );
        var genreRepository = fixture.GetGenreRepository();
        var unitOfWork = fixture.GetUnitOfWork();
        var categoryRepository = fixture.GetCategoryRepository();
        var invalidCategoryIds = input.Categories!.GetRange(input.Categories.Count - 3, 2);
        genreRepository
            .Setup(repo => repo.Get(genre.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(genre);
        categoryRepository
            .Setup(repo => repo.ListIdsByIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                input.Categories!.FindAll(categoryId => !invalidCategoryIds.Contains(categoryId))
            );
        var useCase = new Catalog.Application.UseCases.Genre.Update.UpdateGenre(
            genreRepository.Object,
            unitOfWork.Object,
            categoryRepository.Object
        );

        var action = () => useCase.Handle(input, CancellationToken.None);

        await action
            .Should()
            .ThrowAsync<RelatedEntityException>()
            .WithMessage($"Related category ids not found {string.Join(", ", invalidCategoryIds)}");
        genreRepository.Verify(
            repo => repo.Get(genre.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        genreRepository.Verify(
            repo =>
                repo.Update(It.IsAny<Catalog.Domain.Entity.Genre>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        unitOfWork.Verify(uow => uow.Commit(It.IsAny<CancellationToken>()), Times.Never);
        categoryRepository.Verify(
            repo => repo.ListIdsByIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact(DisplayName = nameof(UpdateWithoutCategories))]
    [Trait("Genre", "Update Genre - Use Case")]
    public async Task UpdateWithoutCategories()
    {
        var genre = fixture.GetValidGenre();
        var genreOldCategories = Enumerable.Range(1, 3).Select(_ => Guid.NewGuid()).ToList();
        genreOldCategories.ForEach(genre.AddCategory);
        var input = fixture.GetValidInput(genre.Id);
        var genreRepository = fixture.GetGenreRepository();
        var unitOfWork = fixture.GetUnitOfWork();
        var categoryRepository = fixture.GetCategoryRepository();
        genreRepository
            .Setup(repo => repo.Get(genre.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(genre);
        var useCase = new Catalog.Application.UseCases.Genre.Update.UpdateGenre(
            genreRepository.Object,
            unitOfWork.Object,
            categoryRepository.Object
        );

        var output = await useCase.Handle(input, CancellationToken.None);

        genreRepository.Verify(
            repo => repo.Get(genre.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        genreRepository.Verify(
            repo =>
                repo.Update(It.IsAny<Catalog.Domain.Entity.Genre>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        categoryRepository.Verify(
            repo => repo.ListIdsByIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        unitOfWork.Verify(uow => uow.Commit(It.IsAny<CancellationToken>()), Times.Once);
        output.Id.Should().Be(input.Id);
        output.Name.Should().Be(input.Name);
        output.IsActive.Should().Be(genre.IsActive);
        output.Categories.Should().HaveCount(genre.Categories.Count);
        output.Categories.Should().BeEquivalentTo(genre.Categories);
        output.CreatedAt.Should().NotBeSameDateAs(default);
    }

    [Fact(DisplayName = nameof(UpdateWithEmptyCategoriesList))]
    [Trait("Genre", "Update Genre - Use Case")]
    public async Task UpdateWithEmptyCategoriesList()
    {
        var genre = fixture.GetValidGenre();
        var genreOldCategories = Enumerable.Range(1, 3).Select(_ => Guid.NewGuid()).ToList();
        genreOldCategories.ForEach(genre.AddCategory);
        var input = fixture.GetValidInput(genre.Id, categories: []);
        var genreRepository = fixture.GetGenreRepository();
        var unitOfWork = fixture.GetUnitOfWork();
        var categoryRepository = fixture.GetCategoryRepository();
        genreRepository
            .Setup(repo => repo.Get(genre.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(genre);
        var useCase = new Catalog.Application.UseCases.Genre.Update.UpdateGenre(
            genreRepository.Object,
            unitOfWork.Object,
            categoryRepository.Object
        );

        var output = await useCase.Handle(input, CancellationToken.None);

        genreRepository.Verify(
            repo => repo.Get(genre.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        genreRepository.Verify(
            repo =>
                repo.Update(It.IsAny<Catalog.Domain.Entity.Genre>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        categoryRepository.Verify(
            repo => repo.ListIdsByIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        unitOfWork.Verify(uow => uow.Commit(It.IsAny<CancellationToken>()), Times.Once);
        output.Id.Should().Be(input.Id);
        output.Name.Should().Be(input.Name);
        output.IsActive.Should().Be(genre.IsActive);
        output.Categories.Should().BeEmpty();
        output.CreatedAt.Should().NotBeSameDateAs(default);
    }
}
