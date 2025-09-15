using Codeflix.Catalog.Domain.Exceptions;
using Codeflix.Catalog.Infra.Data.EF;
using Codeflix.Catalog.Infra.Data.EF.Repositories;
using FluentAssertions;
using UseCase = Codeflix.Catalog.Application.UseCases.Category.Create;

namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.Category.CreateCategory;

[Collection(nameof(CreateCategoryTestFixture))]
public class CreateCategoryTest(CreateCategoryTestFixture fixture)
{
    [Fact(DisplayName = nameof(CreateCategory))]
    [Trait("Integration/Application", "CreateCategory - Use Cases")]
    public async Task CreateCategory()
    {
        var dbContext = fixture.CreateDbContext();
        var repository = new CategoryRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);
        var useCase = new UseCase.CreateCategory(repository, unitOfWork);
        var input = fixture.GetValidInput();

        var output = await useCase.Handle(input, CancellationToken.None);

        var persistedCategory = await fixture
            .CreateDbContext(true)
            .Categories.FindAsync(output.Id, CancellationToken.None);

        persistedCategory.Should().NotBeNull();
        persistedCategory.Id.Should().NotBeEmpty();
        persistedCategory.Name.Should().Be(input.Name);
        persistedCategory.Description.Should().Be(input.Description);
        persistedCategory.IsActive.Should().Be(input.IsActive);
        persistedCategory.CreatedAt.Should().NotBeSameDateAs(default);
        output.Should().NotBeNull();
        output.Id.Should().NotBeEmpty();
        output.Name.Should().Be(input.Name);
        output.Description.Should().Be(input.Description);
        output.IsActive.Should().Be(input.IsActive);
        output.CreatedAt.Should().NotBeSameDateAs(default);
    }

    [Fact(DisplayName = nameof(CreateCategoryWithOnlyName))]
    [Trait("Integration/Application", "CreateCategory - Use Cases")]
    public async Task CreateCategoryWithOnlyName()
    {
        var dbContext = fixture.CreateDbContext();
        var repository = new CategoryRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);
        var useCase = new UseCase.CreateCategory(repository, unitOfWork);
        var input = new UseCase.CreateCategoryInput(fixture.GetValidCategoryName());

        var output = await useCase.Handle(input, CancellationToken.None);

        var persistedCategory = await fixture
            .CreateDbContext(true)
            .Categories.FindAsync(output.Id, CancellationToken.None);
        persistedCategory.Should().NotBeNull();
        persistedCategory.Id.Should().NotBeEmpty();
        persistedCategory.Name.Should().Be(input.Name);
        persistedCategory.Description.Should().Be(string.Empty);
        persistedCategory.IsActive.Should().BeTrue();
        persistedCategory.CreatedAt.Should().NotBeSameDateAs(default);
        output.Should().NotBeNull();
        output.Id.Should().NotBeEmpty();
        output.Name.Should().Be(input.Name);
        output.Description.Should().Be(string.Empty);
        output.IsActive.Should().Be(input.IsActive);
        output.CreatedAt.Should().NotBeSameDateAs(default);
    }

    [Fact(DisplayName = nameof(CreateCategoryWithOnlyNameAndDescription))]
    [Trait("Integration/Application", "CreateCategory - Use Cases")]
    public async Task CreateCategoryWithOnlyNameAndDescription()
    {
        var dbContext = fixture.CreateDbContext();
        var repository = new CategoryRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);
        var useCase = new UseCase.CreateCategory(repository, unitOfWork);
        var input = new UseCase.CreateCategoryInput(
            fixture.GetValidCategoryName(),
            fixture.GetValidCategoryDescription()
        );

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().NotBeEmpty();
        output.Name.Should().Be(input.Name);
        output.Description.Should().Be(input.Description);
        output.IsActive.Should().BeTrue();
        output.CreatedAt.Should().NotBeSameDateAs(default);
    }

    [Theory(DisplayName = nameof(CreateCategoryThrowWhenCantInstantiateCategory))]
    [Trait("Integration/Application", "CreateCategory - Use Cases")]
    [MemberData(
        nameof(CreateCategoryTestDataGenerator.GetInvalidInputs),
        parameters: 5,
        MemberType = typeof(CreateCategoryTestDataGenerator)
    )]
    public async Task CreateCategoryThrowWhenCantInstantiateCategory(
        UseCase.CreateCategoryInput invalidInput,
        string exceptionMessage
    )
    {
        var dbContext = fixture.CreateDbContext();
        var repository = new CategoryRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);
        var useCase = new UseCase.CreateCategory(repository, unitOfWork);

        var action = () => useCase.Handle(invalidInput, CancellationToken.None);

        await action.Should().ThrowAsync<EntityValidationException>().WithMessage(exceptionMessage);
    }
}
