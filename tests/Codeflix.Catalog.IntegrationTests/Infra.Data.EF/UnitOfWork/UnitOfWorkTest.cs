using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using InfraEF = Codeflix.Catalog.Infra.Data.EF;

namespace Codeflix.Catalog.IntegrationTests.Infra.Data.EF.UnitOfWork;

[Collection(nameof(UnitOfWorkTestFixture))]
public class UnitOfWorkTest(UnitOfWorkTestFixture fixture)
{
    [Fact(DisplayName = nameof(Commit))]
    [Trait("Integration/Infra.Data", "UnitOfWork - Persistence")]
    public async Task Commit()
    {
        var dbContext = fixture.CreateDbContext();
        var exampleCategoriesList = fixture.GetValidCategories();
        await dbContext.AddRangeAsync(exampleCategoriesList);
        var unitOfWork = new InfraEF.UnitOfWork(dbContext);

        await unitOfWork.Commit(CancellationToken.None);

        var assertDbContext = fixture.CreateDbContext(true);
        var persistedCategories = assertDbContext.Categories.AsNoTracking().ToList();

        persistedCategories.Should().HaveCount(exampleCategoriesList.Count);
    }

    [Fact(DisplayName = nameof(Rollback))]
    [Trait("Integration/Infra.Data", "UnitOfWork - Persistence")]
    public async Task Rollback()
    {
        var dbContext = fixture.CreateDbContext();
        var exampleCategoriesList = fixture.GetValidCategories();
        await dbContext.AddRangeAsync(exampleCategoriesList);
        var unitOfWork = new InfraEF.UnitOfWork(dbContext);

        var action = () => unitOfWork.Rollback(CancellationToken.None);

        await action.Should().NotThrowAsync();
    }
}
