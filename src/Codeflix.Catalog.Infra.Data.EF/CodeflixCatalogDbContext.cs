using Codeflix.Catalog.Domain.Entity;
using Codeflix.Catalog.Infra.Data.EF.Models;
using Microsoft.EntityFrameworkCore;

namespace Codeflix.Catalog.Infra.Data.EF;

public class CodeflixCatalogDbContext(DbContextOptions<CodeflixCatalogDbContext> options)
    : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<GenresCategories> GenresCategories => Set<GenresCategories>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CodeflixCatalogDbContext).Assembly);
    }
}
