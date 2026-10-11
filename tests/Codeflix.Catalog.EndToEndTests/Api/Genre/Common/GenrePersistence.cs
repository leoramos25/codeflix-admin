using Codeflix.Catalog.Infra.Data.EF;
using Codeflix.Catalog.Infra.Data.EF.Models;
using Microsoft.EntityFrameworkCore;
using DomainEntity = Codeflix.Catalog.Domain.Entity;

namespace Codeflix.Catalog.EndToEndTests.Api.Genre.Common;

public class GenrePersistence(CodeflixCatalogDbContext context)
{
    public async Task InsertList(
        List<DomainEntity.Genre> genres,
        CancellationToken cancellationToken = default
    )
    {
        await context.Genres.AddRangeAsync(genres, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<DomainEntity.Genre>> GetAll(
        CancellationToken cancellationToken = default
    )
    {
        return await context.Genres.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task InsertGenresCategoriesRelationsList(
        List<GenresCategories> relations,
        CancellationToken cancellationToken = default
    )
    {
        await context.GenresCategories.AddRangeAsync(relations, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
