using Bogus;
using Codeflix.Catalog.Infra.Data.EF;
using Microsoft.EntityFrameworkCore;

namespace Codeflix.Catalog.IntegrationTests.Common;

public class BaseFixture
{
    public Faker Faker { get; set; }

    protected BaseFixture() => Faker = new("pt_BR");

    public CodeflixCatalogDbContext CreateDbContext(bool preserveData = false)
    {
        var dbContext = new CodeflixCatalogDbContext(
            new DbContextOptionsBuilder<CodeflixCatalogDbContext>()
                .UseInMemoryDatabase("integration-tests-db")
                .Options
        );
        if (!preserveData)
            dbContext.Database.EnsureDeleted();
        return dbContext;
    }
}
