using Bogus;
using Codeflix.Catalog.Infra.Data.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using DomainEntity = Codeflix.Catalog.Domain.Entity;

namespace Codeflix.Catalog.EndToEndTests.Common;

public abstract class BaseFixture
{
    protected const int CategoryNameMinLength = 3;
    protected const int CategoryNameMaxLength = 255;
    protected const int CategoryDescriptionMaxLength = 10_000;

    private readonly string _connectionString;

    protected BaseFixture()
    {
        Faker = new Faker("pt_BR");
        WebApplicationFactory = new CustomWebApplicationFactory<Program>();
        HttpClient = WebApplicationFactory.CreateClient();
        ApiClient = new ApiClient(HttpClient);
        var configuration = WebApplicationFactory.Services.GetService(typeof(IConfiguration));
        ArgumentNullException.ThrowIfNull(configuration);
        _connectionString =
            ((IConfiguration)configuration).GetConnectionString("CatalogDb")
            ?? throw new InvalidOperationException();
    }

    protected Faker Faker { get; set; }
    public CustomWebApplicationFactory<Program> WebApplicationFactory { get; set; }
    public HttpClient HttpClient { get; set; }
    public ApiClient ApiClient { get; set; }

    public CodeflixCatalogDbContext CreateDbContext()
    {
        var context = new CodeflixCatalogDbContext(
            new DbContextOptionsBuilder<CodeflixCatalogDbContext>()
                .UseMySql(_connectionString, ServerVersion.AutoDetect(_connectionString))
                .Options
        );
        return context;
    }

    public void CleanPersistence()
    {
        var context = CreateDbContext();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }

    public List<DomainEntity.Category> GetValidCategories(int size = 10)
    {
        return [.. Enumerable.Range(1, size).Select(_ => GetValidCategory())];
    }

    public DomainEntity.Category GetValidCategory()
    {
        return new DomainEntity.Category(
            GetValidCategoryName(),
            GetValidCategoryDescription(),
            GetRandomBoolean()
        );
    }

    public string GetValidCategoryName()
    {
        var categoryName = string.Empty;
        while (categoryName.Length < CategoryNameMinLength)
            categoryName = Faker.Commerce.Categories(1)[0];
        if (categoryName.Length > CategoryNameMaxLength)
            categoryName = categoryName[..CategoryNameMaxLength];
        return categoryName;
    }

    public string GetValidCategoryDescription()
    {
        var categoryDescription = Faker.Commerce.ProductDescription();
        if (categoryDescription.Length > CategoryDescriptionMaxLength)
            categoryDescription = categoryDescription[..CategoryDescriptionMaxLength];
        return categoryDescription;
    }

    public bool GetRandomBoolean()
    {
        return new Random().NextDouble() < 0.5;
    }
}
