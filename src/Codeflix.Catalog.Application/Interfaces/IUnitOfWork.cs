namespace Codeflix.Catalog.Application.Interfaces;

public interface IUnitOfWork
{
    Task Commit(CancellationToken cancellationToken);
    Task Rollback(CancellationToken cancellationToken);
}
