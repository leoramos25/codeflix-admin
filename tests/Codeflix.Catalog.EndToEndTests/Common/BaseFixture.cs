using Bogus;
using Codeflix.Catalog.Infra.Data.EF;
using Microsoft.EntityFrameworkCore;

namespace Codeflix.Catalog.EndToEndTests.Common;

public abstract class BaseFixture
{
    protected Faker Faker { get; set; }
    public CustomWebApplicationFactory<Program> WebApplicationFactory { get; set; }
    public HttpClient HttpClient { get; set; }
    public ApiClient ApiClient { get; set; }

    protected BaseFixture()
    {
        Faker = new("pt_BR");
        WebApplicationFactory = new CustomWebApplicationFactory<Program>();
        HttpClient = WebApplicationFactory.CreateClient();
        ApiClient = new ApiClient(HttpClient);
    }

    public CodeflixCatalogDbContext CreateDbContext(bool preserveData = false)
    {
        var context = new CodeflixCatalogDbContext(
            new DbContextOptionsBuilder<CodeflixCatalogDbContext>()
                .UseInMemoryDatabase("e2e-tests-db")
                .Options
        );
        if (!preserveData)
            context.Database.EnsureDeleted();
        return context;
    }
}
