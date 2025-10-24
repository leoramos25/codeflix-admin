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
        NotFoundException.ThrowIfNull(genre, $"Genre '{id}' not found");
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

    public Task Update(Genre aggregate, CancellationToken cancellationToken)
    {
        _genres.Update(aggregate);
        return Task.CompletedTask;
    }

    public Task<SearchOutput<Genre>> Search(SearchInput input, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}