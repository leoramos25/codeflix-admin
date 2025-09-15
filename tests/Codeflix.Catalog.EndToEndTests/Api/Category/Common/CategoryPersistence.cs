using Codeflix.Catalog.Infra.Data.EF;
using Microsoft.EntityFrameworkCore;

namespace Codeflix.Catalog.EndToEndTests.Api.Category.Common;

public class CategoryPersistence(CodeflixCatalogDbContext context)
{
    public async Task<Domain.Entity.Category?> GetById(Guid id)
    {
        return await context.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task InsertList(
        List<Domain.Entity.Category> categories,
        CancellationToken cancellationToken = default
    )
    {
        await context.Categories.AddRangeAsync(categories, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
