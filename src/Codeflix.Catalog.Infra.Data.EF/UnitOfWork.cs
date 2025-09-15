using Codeflix.Catalog.Application.Interfaces;

namespace Codeflix.Catalog.Infra.Data.EF;

public class UnitOfWork(CodeflixCatalogDbContext context) : IUnitOfWork
{
    public async Task Commit(CancellationToken cancellationToken) =>
        await context.SaveChangesAsync(cancellationToken);

    public async Task Rollback(CancellationToken cancellationToken) => await Task.CompletedTask;
}
