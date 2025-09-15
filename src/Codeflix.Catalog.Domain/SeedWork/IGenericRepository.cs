namespace Codeflix.Catalog.Domain.SeedWork;

public interface IGenericRepository<TAggregate> : IRepository
    where TAggregate : AggregateRoot
{
    Task Insert(TAggregate aggregate, CancellationToken cancellationToken);
    Task<TAggregate> Get(Guid id, CancellationToken cancellationToken);
    Task Delete(TAggregate aggregate, CancellationToken cancellationToken);
    Task Update(TAggregate aggregate, CancellationToken cancellationToken);
}
