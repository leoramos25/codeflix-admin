using Codeflix.Catalog.Domain.Exceptions;
using FluentAssertions;
using Moq;
using UseCases = Codeflix.Catalog.Application.UseCases.Category.Create;

namespace Codeflix.Catalog.UnitTests.Application.Category.CreateCategory;

[Collection(nameof(CreateCategoryTestFixture))]
public class CreateCategoryTest(CreateCategoryTestFixture fixture)
{
    [Fact(DisplayName = nameof(CreateCategory))]
    [Trait("Application", "CreateCategory - Use Cases")]
    public async Task CreateCategory()
    {
        var repositoryMock = fixture.GetRepositoryMock();
        var unitOfWorkMock = fixture.GetUnitOfWorkMock();
        var useCase = new UseCases.CreateCategory(repositoryMock.Object, unitOfWorkMock.Object);
        var input = fixture.GetValidInput();

        var output = await useCase.Handle(input, CancellationToken.None);

        repositoryMock.Verify(
            repo =>
                repo.Insert(
                    It.IsAny<Catalog.Domain.Entity.Category>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        unitOfWorkMock.Verify(uow => uow.Commit(It.IsAny<CancellationToken>()), Times.Once);
        output.Should().NotBeNull();
        output.Id.Should().NotBeEmpty();
        output.Name.Should().Be(input.Name);
        output.Description.Should().Be(input.Description);
        output.IsActive.Should().Be(input.IsActive);
        output.CreatedAt.Should().NotBeSameDateAs(default);
    }

    [Fact(DisplayName = nameof(CreateCategoryWithOnlyName))]
    [Trait("Application", "CreateCategory - Use Cases")]
    public async Task CreateCategoryWithOnlyName()
    {
        var repositoryMock = fixture.GetRepositoryMock();
        var unitOfWorkMock = fixture.GetUnitOfWorkMock();
        var useCase = new UseCases.CreateCategory(repositoryMock.Object, unitOfWorkMock.Object);
        var input = new UseCases.CreateCategoryInput(fixture.GetValidCategoryName());

        var output = await useCase.Handle(input, CancellationToken.None);

        repositoryMock.Verify(
            repo =>
                repo.Insert(
                    It.IsAny<Catalog.Domain.Entity.Category>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        unitOfWorkMock.Verify(uow => uow.Commit(It.IsAny<CancellationToken>()), Times.Once);
        output.Should().NotBeNull();
        output.Id.Should().NotBeEmpty();
        output.Name.Should().Be(input.Name);
        output.Description.Should().Be(string.Empty);
        output.IsActive.Should().BeTrue();
        output.CreatedAt.Should().NotBeSameDateAs(default);
    }

    [Fact(DisplayName = nameof(CreateCategoryWithOnlyNameAndDescription))]
    [Trait("Application", "CreateCategory - Use Cases")]
    public async Task CreateCategoryWithOnlyNameAndDescription()
    {
        var repositoryMock = fixture.GetRepositoryMock();
        var unitOfWorkMock = fixture.GetUnitOfWorkMock();
        var useCase = new UseCases.CreateCategory(repositoryMock.Object, unitOfWorkMock.Object);
        var input = new UseCases.CreateCategoryInput(
            fixture.GetValidCategoryName(),
            fixture.GetValidCategoryDescription()
        );

        var output = await useCase.Handle(input, CancellationToken.None);

        repositoryMock.Verify(
            repo =>
                repo.Insert(
                    It.IsAny<Catalog.Domain.Entity.Category>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        unitOfWorkMock.Verify(uow => uow.Commit(It.IsAny<CancellationToken>()), Times.Once);
        output.Should().NotBeNull();
        output.Id.Should().NotBeEmpty();
        output.Name.Should().Be(input.Name);
        output.Description.Should().Be(input.Description);
        output.IsActive.Should().BeTrue();
        output.CreatedAt.Should().NotBeSameDateAs(default);
    }

    [Theory(DisplayName = nameof(CreateCategoryThrowWhenCantInstantiateCategory))]
    [Trait("Application", "CreateCategory - Use Cases")]
    [MemberData(
        nameof(CreateCategoryTestDataGenerator.GetInvalidInputs),
        parameters: 15,
        MemberType = typeof(CreateCategoryTestDataGenerator)
    )]
    public async Task CreateCategoryThrowWhenCantInstantiateCategory(
        UseCases.CreateCategoryInput invalidInput,
        string exceptionMessage
    )
    {
        var repositoryMock = fixture.GetRepositoryMock();
        var unitOfWorkMock = fixture.GetUnitOfWorkMock();
        var useCase = new UseCases.CreateCategory(repositoryMock.Object, unitOfWorkMock.Object);

        var action = () => useCase.Handle(invalidInput, CancellationToken.None);

        repositoryMock.Verify(
            repo =>
                repo.Insert(
                    It.IsAny<Catalog.Domain.Entity.Category>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
        unitOfWorkMock.Verify(uow => uow.Commit(It.IsAny<CancellationToken>()), Times.Never);
        await action.Should().ThrowAsync<EntityValidationException>().WithMessage(exceptionMessage);
    }
}
