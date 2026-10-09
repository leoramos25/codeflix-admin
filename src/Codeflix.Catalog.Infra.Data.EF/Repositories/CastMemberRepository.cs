using Codeflix.Catalog.Application.Exceptions;
using Codeflix.Catalog.Domain.Entity;
using Codeflix.Catalog.Domain.Repository;
using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using Microsoft.EntityFrameworkCore;

namespace Codeflix.Catalog.Infra.Data.EF.Repositories;

public class CastMemberRepository(CodeflixCatalogDbContext context) : ICastMemberRepository
{
    private readonly DbSet<CastMember> _castMembers = context.CastMembers;

    public async Task Insert(CastMember aggregate, CancellationToken cancellationToken)
    {
        await _castMembers.AddAsync(aggregate, cancellationToken);
    }

    public async Task<CastMember> Get(Guid id, CancellationToken cancellationToken)
    {
        var castMember = await _castMembers.FindAsync([id, cancellationToken], cancellationToken);
        NotFoundException.ThrowIfNull(castMember, $"CastMember {id} not found.");
        return castMember!;
    }

    public async Task Delete(CastMember aggregate, CancellationToken cancellationToken)
    {
        await Task.FromResult(_castMembers.Remove(aggregate));
    }

    public async Task Update(CastMember aggregate, CancellationToken cancellationToken)
    {
        await Task.FromResult(_castMembers.Update(aggregate));
    }

    public async Task<SearchOutput<CastMember>> Search(
        SearchInput input,
        CancellationToken cancellationToken
    )
    {
        var toSkip = (input.Page - 1) * input.PerPage;
        var query = _castMembers.AsNoTracking();
        query = AddOrderToQuery(query, input.OrderBy, input.Order);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip(toSkip).Take(input.PerPage).ToListAsync(cancellationToken);
        return new SearchOutput<CastMember>(input.Page, input.PerPage, total, items);
    }

    private static IQueryable<CastMember> AddOrderToQuery(
        IQueryable<CastMember> query,
        string orderProperty,
        SearchOrder order
    )
    {
        var orderedQuery = (orderProperty.ToLower(), order) switch
        {
            ("name", SearchOrder.Asc) => query.OrderBy(x => x.Name),
            ("name", SearchOrder.Desc) => query.OrderByDescending(x => x.Name),
            ("type", SearchOrder.Asc) => query.OrderBy(x => x.Type),
            ("type", SearchOrder.Desc) => query.OrderByDescending(x => x.Type),
            ("createdat", SearchOrder.Asc) => query.OrderBy(x => x.CreatedAt),
            ("createdat", SearchOrder.Desc) => query.OrderByDescending(x => x.CreatedAt),
            _ => query.OrderBy(x => x.Name),
        };
        return orderedQuery.ThenBy(x => x.Id);
    }
}
