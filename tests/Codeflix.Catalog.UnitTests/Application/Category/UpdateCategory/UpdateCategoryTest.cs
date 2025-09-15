using Codeflix.Catalog.Application.Exceptions;
using Codeflix.Catalog.Domain.Exceptions;
using FluentAssertions;
using Moq;
using UseCases = Codeflix.Catalog.Application.UseCases.Category.Update;

namespace Codeflix.Catalog.UnitTests.Application.Category.UpdateCategory;

[Collection(nameof(UpdateCategoryTestFixture))]
public class UpdateCategoryTest(UpdateCategoryTestFixture fixture)
{
    [Theory(DisplayName = nameof(UpdateCategory))]
    [Trait("Integration/Application", "UpdateCategory - Use Cases")]
    [MemberData(
        nameof(UpdateCategoryTestDataGenerator.GetCategoriesToUpdate),
        parameters: 10,
        MemberType = typeof(UpdateCategoryTestDataGenerator)
    )]
    public async Task UpdateCategory(
        Catalog.Domain.Entity.Category category,
        UseCases.UpdateCategoryInput input
    )
    {
        var repositoryMock = fixture.GetRepositoryMock();
        var unitOfWorkMock = fixture.GetUnitOfWorkMock();
        repositoryMock
            .Setup(repo => repo.Get(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        var useCase = new UseCases.UpdateCategory(repositoryMock.Object, unitOfWorkMock.Object);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().Be(category.Id);
        output.Name.Should().Be(input.Name);
        output.Description.Should().Be(input.Description);
        output.IsActive.Should().Be(input.IsActive!.Value);
        output.CreatedAt.Should().Be(category.CreatedAt);
        repositoryMock.Verify(
            repo => repo.Get(category.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        repositoryMock.Verify(
            repo => repo.Update(category, It.IsAny<CancellationToken>()),
            Times.Once
        );
        unitOfWorkMock.Verify(uow => uow.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = nameof(ThrowWhenCategoryNotFound))]
    [Trait("Integration/Application", "UpdateCategory - Use Cases")]
    public async Task ThrowWhenCategoryNotFound()
    {
        var repositoryMock = fixture.GetRepositoryMock();
        var unitOfWorkMock = fixture.GetUnitOfWorkMock();
        var input = fixture.GetValidInput();
        repositoryMock
            .Setup(repo => repo.Get(input.Id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Category '{input.Id}' not found"));
        var useCase = new UseCases.UpdateCategory(repositoryMock.Object, unitOfWorkMock.Object);

        var action = () => useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>();
        repositoryMock.Verify(
            repo => repo.Get(input.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        repositoryMock.Verify(
            repo =>
                repo.Update(
                    It.IsAny<Catalog.Domain.Entity.Category>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
        unitOfWorkMock.Verify(uow => uow.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory(DisplayName = nameof(UpdateCategoryWithoutProvidingIsActive))]
    [Trait("Integration/Application", "UpdateCategory - Use Cases")]
    [MemberData(
        nameof(UpdateCategoryTestDataGenerator.GetCategoriesToUpdate),
        parameters: 10,
        MemberType = typeof(UpdateCategoryTestDataGenerator)
    )]
    public async Task UpdateCategoryWithoutProvidingIsActive(
        Catalog.Domain.Entity.Category category,
        UseCases.UpdateCategoryInput exampleInput
    )
    {
        var input = new UseCases.UpdateCategoryInput(
            exampleInput.Id,
            exampleInput.Name,
            exampleInput.Description
        );
        var repositoryMock = fixture.GetRepositoryMock();
        var unitOfWorkMock = fixture.GetUnitOfWorkMock();
        repositoryMock
            .Setup(repo => repo.Get(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        var useCase = new UseCases.UpdateCategory(repositoryMock.Object, unitOfWorkMock.Object);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().Be(category.Id);
        output.Name.Should().Be(input.Name);
        output.Description.Should().Be(input.Description);
        output.IsActive.Should().Be(category.IsActive);
        output.CreatedAt.Should().Be(category.CreatedAt);
        repositoryMock.Verify(
            repo => repo.Get(category.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        repositoryMock.Verify(
            repo => repo.Update(category, It.IsAny<CancellationToken>()),
            Times.Once
        );
        unitOfWorkMock.Verify(uow => uow.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory(DisplayName = nameof(UpdateCategoryOnlyName))]
    [Trait("Integration/Application", "UpdateCategory - Use Cases")]
    [MemberData(
        nameof(UpdateCategoryTestDataGenerator.GetCategoriesToUpdate),
        parameters: 10,
        MemberType = typeof(UpdateCategoryTestDataGenerator)
    )]
    public async Task UpdateCategoryOnlyName(
        Catalog.Domain.Entity.Category category,
        UseCases.UpdateCategoryInput exampleInput
    )
    {
        var input = new UseCases.UpdateCategoryInput(exampleInput.Id, exampleInput.Name);
        var repositoryMock = fixture.GetRepositoryMock();
        var unitOfWorkMock = fixture.GetUnitOfWorkMock();
        repositoryMock
            .Setup(repo => repo.Get(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        var useCase = new UseCases.UpdateCategory(repositoryMock.Object, unitOfWorkMock.Object);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().Be(category.Id);
        output.Name.Should().Be(input.Name);
        output.Description.Should().Be(category.Description);
        output.IsActive.Should().Be(category.IsActive);
        output.CreatedAt.Should().Be(category.CreatedAt);
        repositoryMock.Verify(
            repo => repo.Get(category.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        repositoryMock.Verify(
            repo => repo.Update(category, It.IsAny<CancellationToken>()),
            Times.Once
        );
        unitOfWorkMock.Verify(uow => uow.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory(DisplayName = nameof(UpdateThrowWhenCantInstantiateCategory))]
    [Trait("Integration/Application", "CreateCategory - Use Cases")]
    [MemberData(
        nameof(UpdateCategoryTestDataGenerator.GetInvalidInputs),
        parameters: 12,
        MemberType = typeof(UpdateCategoryTestDataGenerator)
    )]
    public async Task UpdateThrowWhenCantInstantiateCategory(
        UseCases.UpdateCategoryInput invalidInput,
        string exceptionMessage
    )
    {
        var repositoryMock = fixture.GetRepositoryMock();
        var unitOfWorkMock = fixture.GetUnitOfWorkMock();
        var category = fixture.GetValidCategory();
        invalidInput.Id = category.Id;
        repositoryMock
            .Setup(repo => repo.Get(invalidInput.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        var useCase = new UseCases.UpdateCategory(repositoryMock.Object, unitOfWorkMock.Object);

        var action = () => useCase.Handle(invalidInput, CancellationToken.None);

        await action.Should().ThrowAsync<EntityValidationException>().WithMessage(exceptionMessage);
        repositoryMock.Verify(
            repo => repo.Get(invalidInput.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        repositoryMock.Verify(
            repo =>
                repo.Update(
                    It.IsAny<Catalog.Domain.Entity.Category>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
        unitOfWorkMock.Verify(uow => uow.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }
}
