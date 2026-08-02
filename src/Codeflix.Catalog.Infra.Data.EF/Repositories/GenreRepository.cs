using Codeflix.Catalog.Application.Exceptions;
using Codeflix.Catalog.Domain.Entity;
using Codeflix.Catalog.Domain.Repository;
using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;
using Codeflix.Catalog.Infra.Data.EF.Models;
using Microsoft.EntityFrameworkCore;

namespace Codeflix.Catalog.Infra.Data.EF.Repositories;

public class GenreRepository(CodeflixCatalogDbContext context) : IGenreRepository
{
    private readonly DbSet<Genre> _genres = context.Genres;
    private readonly DbSet<GenresCategories> _genresCategories = context.GenresCategories;

    public async Task Insert(Genre aggregate, CancellationToken cancellationToken)
    {
        await _genres.AddAsync(aggregate, cancellationToken);
        if (aggregate.Categories.Count > 0)
        {
            var relations = aggregate.Categories.Select(categoryId => new GenresCategories(
                aggregate.Id,
                categoryId
            ));
            await _genresCategories.AddRangeAsync(relations, cancellationToken);
        }
    }

    public async Task<Genre> Get(Guid id, CancellationToken cancellationToken)
    {
        var genre = await _genres.FindAsync([id], cancellationToken);
        NotFoundException.ThrowIfNull(genre, $"Genre '{id}' not found.");
        var genreCategories = await _genresCategories
            .AsNoTracking()
            .Where(relation => relation.GenreId == id)
            .ToListAsync(cancellationToken);
        if (genreCategories.Count > 0)
            genreCategories.ForEach(relation => genre!.AddCategory(relation.CategoryId));
        return genre!;
    }

    public Task Delete(Genre aggregate, CancellationToken cancellationToken)
    {
        if (aggregate.Categories.Count > 0)
            _genresCategories.RemoveRange(
                _genresCategories.Where(relation => relation.GenreId == aggregate.Id)
            );
        _genres.Remove(aggregate);
        return Task.CompletedTask;
    }

    public async Task Update(Genre aggregate, CancellationToken cancellationToken)
    {
        _genres.Update(aggregate);
        _genresCategories.RemoveRange(_genresCategories.Where(x => x.GenreId == aggregate.Id));
        if (aggregate.Categories.Count > 0)
        {
            var relations = aggregate.Categories.Select(categoryId => new GenresCategories(
                aggregate.Id,
                categoryId
            ));
            await _genresCategories.AddRangeAsync(relations, cancellationToken);
        }
    }

    public async Task<SearchOutput<Genre>> Search(
        SearchInput input,
        CancellationToken cancellationToken
    )
    {
        var toSkip = (input.Page - 1) * input.PerPage;
        var query = _genres.AsNoTracking();
        if (!string.IsNullOrEmpty(input.OrderBy))
            query = AddOrderToQuery(query, input.OrderBy, input.Order);
        if (!string.IsNullOrEmpty(input.Search))
            query = query.Where(genre => genre.Name.Contains(input.Search));
        var count = await query.CountAsync(cancellationToken);
        var genres = await query.Skip(toSkip).Take(input.PerPage).ToListAsync(cancellationToken);
        var genreIds = genres.Select(g => g.Id);
        var relations = await _genresCategories
            .Where(relation => genreIds.Contains(relation.GenreId))
            .ToListAsync(cancellationToken);
        var relationsGroupByGenreId = relations.GroupBy(relation => relation.GenreId).ToList();
        relationsGroupByGenreId.ForEach(relationGroup =>
        {
            var genre = genres.Find(genre => genre.Id == relationGroup.Key);
            if (genre is null)
                return;
            relationGroup.ToList().ForEach(category => genre!.AddCategory(category.CategoryId));
        });
        return new SearchOutput<Genre>(input.Page, input.PerPage, count, genres);
    }

    private static IQueryable<Genre> AddOrderToQuery(
        IQueryable<Genre> query,
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
