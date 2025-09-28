using Codeflix.Catalog.Application.Exceptions;
using Codeflix.Catalog.Domain.Entity;
using Codeflix.Catalog.Domain.Repository;
using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using Microsoft.EntityFrameworkCore;

namespace Codeflix.Catalog.Infra.Data.EF.Repositories;

public class CategoryRepository(CodeflixCatalogDbContext context) : ICategoryRepository
{
    private readonly DbSet<Category> _categories = context.Categories;

    public async Task Insert(Category aggregate, CancellationToken cancellationToken) =>
        await _categories.AddAsync(aggregate, cancellationToken);

    public async Task<Category> Get(Guid id, CancellationToken cancellationToken)
    {
        var category = await _categories.FindAsync(
            [id, cancellationToken],
            cancellationToken: cancellationToken
        );
        NotFoundException.ThrowIfNull(category, $"Category '{id}' not found.");
        return category!;
    }

    public async Task Delete(Category aggregate, CancellationToken cancellationToken) =>
        await Task.FromResult(_categories.Remove(aggregate));

    public async Task Update(Category aggregate, CancellationToken cancellationToken) =>
        await Task.FromResult(_categories.Update(aggregate));

    public async Task<SearchOutput<Category>> Search(
        SearchInput input,
        CancellationToken cancellationToken
    )
    {
        var toSkip = (input.Page - 1) * input.PerPage;
        var query = _categories.AsNoTracking();
        query = AddOrderToQuery(query, input.OrderBy, input.Order);
        if (!string.IsNullOrWhiteSpace(input.Search))
            query = query.Where(x => x.Name.Contains(input.Search));
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip(toSkip).Take(input.PerPage).ToListAsync(cancellationToken);
        return new SearchOutput<Category>(input.Page, input.PerPage, total, items);
    }

    private static IQueryable<Category> AddOrderToQuery(
        IQueryable<Category> query,
        string orderProperty,
        SearchOrder order
    )
    {
        var orderedQuery = (orderProperty.ToLower(), order) switch
        {
            ("name", SearchOrder.Asc) => query.OrderBy(x => x.Name),
            ("name", SearchOrder.Desc) => query.OrderByDescending(x => x.Name),
            ("id", SearchOrder.Asc) => query.OrderBy(x => x.Id).ThenBy(x => x.Id),
            ("id", SearchOrder.Desc) => query.OrderByDescending(x => x.Id),
            ("createdat", SearchOrder.Asc) => query.OrderBy(x => x.CreatedAt),
            ("createdat", SearchOrder.Desc) => query.OrderByDescending(x => x.CreatedAt),
            _ => query.OrderBy(x => x.Name),
        };
        return orderedQuery.ThenBy(x => x.Id);
    }
}
