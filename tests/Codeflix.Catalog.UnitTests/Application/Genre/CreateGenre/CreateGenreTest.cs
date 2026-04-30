using Codeflix.Catalog.Application.UseCases.Genre.Create;
using Codeflix.Catalog.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace Codeflix.Catalog.UnitTests.Application.Genre.CreateGenre;

[Collection(nameof(CreateGenreTestFixture))]
public class CreateGenreTest(CreateGenreTestFixture fixture)
{
    [Fact(DisplayName = nameof(Create))]
    [Trait("Genre", "Create Genre - Use Case")]
    public async Task Create()
    {
        var unitOfWork = fixture.GetUnitOfWork();
        var genreRepository = fixture.GetGenreRepository();
        var categoryRepository = fixture.GetCategoryRepository();
        var useCase = new Catalog.Application.UseCases.Genre.Create.CreateGenre(
            genreRepository.Object,
            unitOfWork.Object,
            categoryRepository.Object
        );
        var input = fixture.GetValidInput();

        var output = await useCase.Handle(input, CancellationToken.None);

        genreRepository.Verify(
            repo =>
                repo.Insert(It.IsAny<Catalog.Domain.Entity.Genre>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        unitOfWork.Verify(uow => uow.Commit(It.IsAny<CancellationToken>()), Times.Once());
        output.Id.Should().NotBeEmpty();
        output.Name.Should().Be(input.Name);
        output.IsActive.Should().Be(input.IsActive);
        output.Categories.Should().BeEmpty();
        output.CreatedAt.Should().NotBeSameDateAs(default);
    }

    [Fact(DisplayName = nameof(CreateWithOnlyName))]
    [Trait("Genre", "Create Genre - Use Case")]
    public async Task CreateWithOnlyName()
    {
        var unitOfWork = fixture.GetUnitOfWork();
        var genreRepository = fixture.GetGenreRepository();
        var categoryRepository = fixture.GetCategoryRepository();
        var useCase = new Catalog.Application.UseCases.Genre.Create.CreateGenre(
            genreRepository.Object,
            unitOfWork.Object,
            categoryRepository.Object
        );
        var input = new CreateGenreInput(fixture.GetValidName());

        var output = await useCase.Handle(input, CancellationToken.None);

        genreRepository.Verify(
            repo =>
                repo.Insert(It.IsAny<Catalog.Domain.Entity.Genre>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        unitOfWork.Verify(uow => uow.Commit(It.IsAny<CancellationToken>()), Times.Once());
        output.Id.Should().NotBeEmpty();
        output.Name.Should().Be(input.Name);
        output.IsActive.Should().BeTrue();
        output.Categories.Should().BeEmpty();
        output.CreatedAt.Should().NotBeSameDateAs(default);
    }

    [Fact(DisplayName = nameof(CreateGenreWithCategories))]
    [Trait("Genre", "Create Genre - Use Case")]
    public async Task CreateGenreWithCategories()
    {
        var unitOfWork = fixture.GetUnitOfWork();
        var genreRepository = fixture.GetGenreRepository();
        var categoryRepository = fixture.GetCategoryRepository();
        var useCase = new Catalog.Application.UseCases.Genre.Create.CreateGenre(
            genreRepository.Object,
            unitOfWork.Object,
            categoryRepository.Object
        );
        var input = fixture.GetValidInputWithCategories();
        categoryRepository
            .Setup(categoryRepo =>
                categoryRepo.ListIdsByIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(input.Categories.AsReadOnly());

        var output = await useCase.Handle(input, CancellationToken.None);

        genreRepository.Verify(
            repo =>
                repo.Insert(It.IsAny<Catalog.Domain.Entity.Genre>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        unitOfWork.Verify(uow => uow.Commit(It.IsAny<CancellationToken>()), Times.Once());
        output.Id.Should().NotBeEmpty();
        output.Name.Should().Be(input.Name);
        output.IsActive.Should().Be(input.IsActive);
        output.Categories.Should().HaveCount(input.Categories?.Count ?? 0);
        output.Categories.Should().BeEquivalentTo(input.Categories);
        output.CreatedAt.Should().NotBeSameDateAs(default);
    }

    [Fact(DisplayName = nameof(CreateGenreThrowWhenRelatedCategoryNotFound))]
    [Trait("Genre", "Create Genre - Use Case")]
    public async Task CreateGenreThrowWhenRelatedCategoryNotFound()
    {
        var input = fixture.GetValidInputWithCategories();
        var unitOfWork = fixture.GetUnitOfWork();
        var genreRepository = fixture.GetGenreRepository();
        var categoryRepository = fixture.GetCategoryRepository();
        var invalidCategory = input.Categories?[^1];
        categoryRepository
            .Setup(x => x.ListIdsByIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                input.Categories!.FindAll(categoryId => categoryId != invalidCategory).AsReadOnly()
            );
        var useCase = new Catalog.Application.UseCases.Genre.Create.CreateGenre(
            genreRepository.Object,
            unitOfWork.Object,
            categoryRepository.Object
        );

        var action = () => useCase.Handle(input, CancellationToken.None);

        await action
            .Should()
            .ThrowAsync<RelatedEntityException>()
            .WithMessage($"Related category ids not found {invalidCategory}");
        categoryRepository.Verify(
            repo => repo.ListIdsByIds(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        genreRepository.Verify(
            repo =>
                repo.Insert(It.IsAny<Catalog.Domain.Entity.Genre>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        unitOfWork.Verify(uow => uow.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory(DisplayName = nameof(CreateShouldThrowExceptionWhenNameIsInvalid))]
    [Trait("Genre", "Create Genre - Use Case")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task CreateShouldThrowExceptionWhenNameIsInvalid(string? invalidName)
    {
        var unitOfWork = fixture.GetUnitOfWork();
        var genreRepository = fixture.GetGenreRepository();
        var categoryRepository = fixture.GetCategoryRepository();
        var useCase = new Catalog.Application.UseCases.Genre.Create.CreateGenre(
            genreRepository.Object,
            unitOfWork.Object,
            categoryRepository.Object
        );
        var input = new CreateGenreInput(invalidName!);

        var action = () => useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<EntityValidationException>();
        genreRepository.Verify(
            repo =>
                repo.Insert(It.IsAny<Catalog.Domain.Entity.Genre>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        unitOfWork.Verify(uow => uow.Commit(It.IsAny<CancellationToken>()), Times.Never());
    }
}
