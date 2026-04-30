using Codeflix.Catalog.Infra.Data.EF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codeflix.Catalog.Infra.Data.EF.Configurations;

public class GenresCategoriesConfiguration : IEntityTypeConfiguration<GenresCategories>
{
    public void Configure(EntityTypeBuilder<GenresCategories> builder)
    {
        builder.HasKey(relation => new { relation.GenreId, relation.CategoryId });
    }
}
