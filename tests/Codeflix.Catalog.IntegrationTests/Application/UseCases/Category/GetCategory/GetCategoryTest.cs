using Codeflix.Catalog.Application.Exceptions;
using Codeflix.Catalog.Infra.Data.EF.Repositories;
using FluentAssertions;
using UseCase = Codeflix.Catalog.Application.UseCases.Category.Get;

namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.Category.GetCategory;

[Collection(nameof(GetCategoryTestFixture))]
public class GetCategoryTest(GetCategoryTestFixture fixture)
{
    [Fact(DisplayName = nameof(GetCategory))]
    [Trait("Integration/Application", "GetCategory - Use Cases")]
    public async Task GetCategory()
    {
        var dbContext = fixture.CreateDbContext();
        var category = fixture.GetValidCategory();
        await dbContext.Categories.AddAsync(category);
        await dbContext.SaveChangesAsync(CancellationToken.None);
        var repository = new CategoryRepository(fixture.CreateDbContext(true));
        var input = new UseCase.GetCategoryInput(category.Id);
        var useCase = new UseCase.GetCategory(repository);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().Be(category.Id);
        output.Name.Should().Be(category.Name);
        output.Description.Should().Be(category.Description);
        output.IsActive.Should().Be(category.IsActive);
        output.CreatedAt.Should().BeSameDateAs(category.CreatedAt);
    }

    [Fact(DisplayName = nameof(ThrowExceptionWhenCategoryNotFound))]
    [Trait("Integration/Application", "GetCategory - Use Cases")]
    public async Task ThrowExceptionWhenCategoryNotFound()
    {
        var dbContext = fixture.CreateDbContext();
        var category = fixture.GetValidCategory();
        var repository = new CategoryRepository(fixture.CreateDbContext(true));
        var input = new UseCase.GetCategoryInput(category.Id);
        var useCase = new UseCase.GetCategory(repository);

        var action = () => useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>();
    }
}
