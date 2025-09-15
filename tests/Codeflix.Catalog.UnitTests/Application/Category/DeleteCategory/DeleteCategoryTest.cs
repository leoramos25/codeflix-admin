using Codeflix.Catalog.Application.Exceptions;
using FluentAssertions;
using Moq;
using UseCases = Codeflix.Catalog.Application.UseCases.Category.Delete;

namespace Codeflix.Catalog.UnitTests.Application.Category.DeleteCategory;

[Collection(nameof(DeleteCategoryTestFixture))]
public class DeleteCategoryTest(DeleteCategoryTestFixture fixture)
{
    [Fact(DisplayName = nameof(DeleteCategory))]
    [Trait("Application", "DeleteCategory - Use Cases")]
    public async Task DeleteCategory()
    {
        var repositoryMock = fixture.GetRepositoryMock();
        var unitOfWorkMock = fixture.GetUnitOfWorkMock();
        var category = fixture.GetValidCategory();
        repositoryMock
            .Setup(repo => repo.Get(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        var input = new UseCases.DeleteCategoryInput(category.Id);
        var useCase = new UseCases.DeleteCategory(repositoryMock.Object, unitOfWorkMock.Object);

        await useCase.Handle(input, CancellationToken.None);

        repositoryMock.Verify(
            repo => repo.Get(category.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        repositoryMock.Verify(
            repo => repo.Delete(category, It.IsAny<CancellationToken>()),
            Times.Once
        );
        unitOfWorkMock.Verify(uow => uow.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = nameof(ThrowWhenCategoryNotFound))]
    [Trait("Application", "DeleteCategory - Use Cases")]
    public async Task ThrowWhenCategoryNotFound()
    {
        var repositoryMock = fixture.GetRepositoryMock();
        var unitOfWorkMock = fixture.GetUnitOfWorkMock();
        var categoryId = Guid.NewGuid();
        repositoryMock
            .Setup(repo => repo.Get(categoryId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Category '{categoryId}' not found'"));
        var input = new UseCases.DeleteCategoryInput(categoryId);
        var useCase = new UseCases.DeleteCategory(repositoryMock.Object, unitOfWorkMock.Object);

        var action = () => useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>();
        repositoryMock.Verify(
            repo => repo.Get(categoryId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        repositoryMock.Verify(
            repo =>
                repo.Delete(
                    It.IsAny<Catalog.Domain.Entity.Category>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
        unitOfWorkMock.Verify(uow => uow.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }
}
