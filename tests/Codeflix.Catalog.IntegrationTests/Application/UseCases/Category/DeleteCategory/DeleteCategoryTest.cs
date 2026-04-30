using Codeflix.Catalog.Application.Exceptions;
using Codeflix.Catalog.Infra.Data.EF;
using Codeflix.Catalog.Infra.Data.EF.Repositories;
using FluentAssertions;
using UseCase = Codeflix.Catalog.Application.UseCases.Category.Delete;

namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.Category.DeleteCategory;

[Collection(nameof(DeleteCategoryTestFixture))]
public class DeleteCategoryTest(DeleteCategoryTestFixture fixture)
{
    [Fact(DisplayName = nameof(DeleteCategory))]
    [Trait("Integration/Application", "DeleteCategory - Use Cases")]
    public async Task DeleteCategory()
    {
        var dbContext = fixture.CreateDbContext();
        var repository = new CategoryRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);
        var category = fixture.GetValidCategory();
        var input = new UseCase.DeleteCategoryInput(category.Id);
        var useCase = new UseCase.DeleteCategory(repository, unitOfWork);
        await dbContext.AddAsync(category);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        await useCase.Handle(input, CancellationToken.None);

        var deletedCategory = await fixture
            .CreateDbContext(true)
            .Categories.FindAsync(input.CategoryId, CancellationToken.None);

        deletedCategory.Should().BeNull();
    }

    [Fact(DisplayName = nameof(ThrowWhenCategoryNotFound))]
    [Trait("Integration/Application", "DeleteCategory - Use Cases")]
    public async Task ThrowWhenCategoryNotFound()
    {
        var dbContext = fixture.CreateDbContext();
        var repository = new CategoryRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext);
        var categoryId = Guid.NewGuid();
        var input = new UseCase.DeleteCategoryInput(categoryId);
        var useCase = new UseCase.DeleteCategory(repository, unitOfWork);

        var action = () => useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>();
    }
}
