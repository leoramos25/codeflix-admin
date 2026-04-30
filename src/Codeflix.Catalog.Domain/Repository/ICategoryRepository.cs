using Codeflix.Catalog.Domain.Entity;
using Codeflix.Catalog.Domain.SeedWork;
using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;

namespace Codeflix.Catalog.Domain.Repository;

public interface ICategoryRepository : IGenericRepository<Category>, ISearchableRepository<Category>
{
    Task<IReadOnlyCollection<Guid>> ListIdsByIds(
        List<Guid> ids,
        CancellationToken cancellationToken
    );
}
