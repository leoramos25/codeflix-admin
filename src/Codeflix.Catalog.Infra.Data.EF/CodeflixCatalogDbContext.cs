using Codeflix.Catalog.Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Codeflix.Catalog.Infra.Data.EF;

public class CodeflixCatalogDbContext(DbContextOptions<CodeflixCatalogDbContext> options)
    : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CodeflixCatalogDbContext).Assembly);
    }
}
