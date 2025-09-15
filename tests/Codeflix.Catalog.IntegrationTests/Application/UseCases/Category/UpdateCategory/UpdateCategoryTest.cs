using Codeflix.Catalog.Application.Exceptions;
using Codeflix.Catalog.Domain.Exceptions;
using Codeflix.Catalog.Infra.Data.EF;
using Codeflix.Catalog.Infra.Data.EF.Repositories;
using FluentAssertions;
using UseCase = Codeflix.Catalog.Application.UseCases.Category.Update;

namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.Category.UpdateCategory;

[Collection(nameof(UpdateCategoryTestFixture))]
public class UpdateCategoryTest(UpdateCategoryTestFixture fixture)
{
    [Theory(DisplayName = nameof(UpdateCategory))]
    [Trait("Application", "UpdateCategory - Use Cases")]
    [MemberData(
        nameof(UpdateCategoryTestDataGenerator.GetCategoriesToUpdate),
        parameters: 5,
        MemberType = typeof(UpdateCategoryTestDataGenerator)
    )]
    public async Task UpdateCategory(
        Catalog.Domain.Entity.Category category,
        UseCase.UpdateCategoryInput input
    )
    {
        var dbContext = fixture.CreateDbContext();
        var repository = new CategoryRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);
        var useCase = new UseCase.UpdateCategory(repository, unitOfWork);
        await dbContext.AddAsync(category);
        await dbContext.SaveChangesAsync();

        var output = await useCase.Handle(input, CancellationToken.None);

        var updatedCategory = await fixture
            .CreateDbContext(true)
            .Categories.FindAsync(output.Id, CancellationToken.None);

        output.Should().NotBeNull();
        updatedCategory.Should().NotBeNull();
        output.Id.Should().Be(category.Id);
        output.Name.Should().Be(input.Name);
        output.Description.Should().Be(input.Description);
        output.IsActive.Should().Be(input.IsActive!.Value);
        output.CreatedAt.Should().Be(category.CreatedAt);
        updatedCategory.Id.Should().Be(category.Id);
        updatedCategory.Name.Should().Be(input.Name);
        updatedCategory.Description.Should().Be(input.Description);
        updatedCategory.IsActive.Should().Be(input.IsActive!.Value);
        updatedCategory.CreatedAt.Should().Be(category.CreatedAt);
    }

    [Fact(DisplayName = nameof(ThrowWhenCategoryNotFound))]
    [Trait("Application", "UpdateCategory - Use Cases")]
    public async Task ThrowWhenCategoryNotFound()
    {
        var dbContext = fixture.CreateDbContext();
        var repository = new CategoryRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);
        var useCase = new UseCase.UpdateCategory(repository, unitOfWork);
        var input = fixture.GetValidInput();

        var action = () => useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Theory(DisplayName = nameof(UpdateCategoryWithoutProvidingIsActive))]
    [Trait("Application", "UpdateCategory - Use Cases")]
    [MemberData(
        nameof(UpdateCategoryTestDataGenerator.GetCategoriesToUpdate),
        parameters: 5,
        MemberType = typeof(UpdateCategoryTestDataGenerator)
    )]
    public async Task UpdateCategoryWithoutProvidingIsActive(
        Catalog.Domain.Entity.Category category,
        UseCase.UpdateCategoryInput exampleInput
    )
    {
        var input = new UseCase.UpdateCategoryInput(
            exampleInput.Id,
            exampleInput.Name,
            exampleInput.Description
        );
        var dbContext = fixture.CreateDbContext();
        var repository = new CategoryRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);
        var useCase = new UseCase.UpdateCategory(repository, unitOfWork);
        await dbContext.AddAsync(category);
        await dbContext.SaveChangesAsync();

        var output = await useCase.Handle(input, CancellationToken.None);

        var updatedCategory = await fixture
            .CreateDbContext(true)
            .Categories.FindAsync(output.Id, CancellationToken.None);

        output.Should().NotBeNull();
        updatedCategory.Should().NotBeNull();
        output.Id.Should().Be(category.Id);
        output.Name.Should().Be(input.Name);
        output.Description.Should().Be(input.Description);
        output.IsActive.Should().Be(category.IsActive);
        output.CreatedAt.Should().Be(category.CreatedAt);
        updatedCategory.Id.Should().Be(category.Id);
        updatedCategory.Name.Should().Be(input.Name);
        updatedCategory.Description.Should().Be(input.Description);
        updatedCategory.IsActive.Should().Be(category.IsActive);
        updatedCategory.CreatedAt.Should().Be(category.CreatedAt);
    }

    [Theory(DisplayName = nameof(UpdateCategoryOnlyName))]
    [Trait("Application", "UpdateCategory - Use Cases")]
    [MemberData(
        nameof(UpdateCategoryTestDataGenerator.GetCategoriesToUpdate),
        parameters: 10,
        MemberType = typeof(UpdateCategoryTestDataGenerator)
    )]
    public async Task UpdateCategoryOnlyName(
        Catalog.Domain.Entity.Category category,
        UseCase.UpdateCategoryInput exampleInput
    )
    {
        var input = new UseCase.UpdateCategoryInput(exampleInput.Id, exampleInput.Name);
        var dbContext = fixture.CreateDbContext();
        var repository = new CategoryRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);
        var useCase = new UseCase.UpdateCategory(repository, unitOfWork);
        await dbContext.AddAsync(category);
        await dbContext.SaveChangesAsync();

        var output = await useCase.Handle(input, CancellationToken.None);

        var updatedCategory = await fixture
            .CreateDbContext(true)
            .Categories.FindAsync(output.Id, CancellationToken.None);

        output.Should().NotBeNull();
        updatedCategory.Should().NotBeNull();
        output.Id.Should().Be(category.Id);
        output.Name.Should().Be(input.Name);
        output.Description.Should().Be(category.Description);
        output.IsActive.Should().Be(category.IsActive);
        output.CreatedAt.Should().Be(category.CreatedAt);
        updatedCategory.Id.Should().Be(category.Id);
        updatedCategory.Name.Should().Be(input.Name);
        updatedCategory.Description.Should().Be(category.Description);
        updatedCategory.IsActive.Should().Be(category.IsActive);
        updatedCategory.CreatedAt.Should().Be(category.CreatedAt);
    }

    [Theory(DisplayName = nameof(UpdateThrowWhenCantInstantiateCategory))]
    [Trait("Application", "CreateCategory - Use Cases")]
    [MemberData(
        nameof(UpdateCategoryTestDataGenerator.GetInvalidInputs),
        parameters: 12,
        MemberType = typeof(UpdateCategoryTestDataGenerator)
    )]
    public async Task UpdateThrowWhenCantInstantiateCategory(
        UseCase.UpdateCategoryInput invalidInput,
        string exceptionMessage
    )
    {
        var dbContext = fixture.CreateDbContext();
        var category = fixture.GetValidCategory();
        invalidInput.Id = category.Id;
        await dbContext.AddAsync(category);
        await dbContext.SaveChangesAsync();
        var repository = new CategoryRepository(fixture.CreateDbContext(true));
        var unitOfWork = new UnitOfWork(dbContext);
        var useCase = new UseCase.UpdateCategory(repository, unitOfWork);

        var action = () => useCase.Handle(invalidInput, CancellationToken.None);

        await action.Should().ThrowAsync<EntityValidationException>().WithMessage(exceptionMessage);
    }
}
