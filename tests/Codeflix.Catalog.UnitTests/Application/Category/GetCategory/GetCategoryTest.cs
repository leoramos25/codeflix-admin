using Codeflix.Catalog.Application.Exceptions;
using FluentAssertions;
using Moq;
using UseCases = Codeflix.Catalog.Application.UseCases.Category.Get;

namespace Codeflix.Catalog.UnitTests.Application.Category.GetCategory;

[Collection(nameof(GetCategoryTestFixture))]
public class GetCategoryTest(GetCategoryTestFixture fixture)
{
    [Fact(DisplayName = nameof(GetCategory))]
    [Trait("Application", "GetCategory - Use Cases")]
    public async Task GetCategory()
    {
        var repositoryMock = fixture.GetRepositoryMock();
        var category = fixture.GetValidCategory();
        repositoryMock
            .Setup(repo => repo.Get(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        var input = new UseCases.GetCategoryInput(category.Id);
        var useCase = new UseCases.GetCategory(repositoryMock.Object);

        var output = await useCase.Handle(input, CancellationToken.None);

        repositoryMock.Verify(
            repo => repo.Get(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        output.Should().NotBeNull();
        output.Id.Should().Be(category.Id);
        output.Name.Should().Be(category.Name);
        output.Description.Should().Be(category.Description);
        output.IsActive.Should().Be(category.IsActive);
        output.CreatedAt.Should().BeSameDateAs(category.CreatedAt);
    }

    [Fact(DisplayName = nameof(ThrowExceptionWhenCategoryNotFound))]
    [Trait("Application", "GetCategory - Use Cases")]
    public async Task ThrowExceptionWhenCategoryNotFound()
    {
        var repositoryMock = fixture.GetRepositoryMock();
        var category = fixture.GetValidCategory();
        repositoryMock
            .Setup(repo => repo.Get(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Category {category.Id} not found"));
        var input = new UseCases.GetCategoryInput(category.Id);
        var useCase = new UseCases.GetCategory(repositoryMock.Object);

        var action = () => useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>();
        repositoryMock.Verify(
            repo => repo.Get(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
