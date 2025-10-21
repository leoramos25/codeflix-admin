using Codeflix.Catalog.Application.Interfaces;
using Codeflix.Catalog.Domain.Exceptions;
using Codeflix.Catalog.Domain.Repository;

namespace Codeflix.Catalog.Application.UseCases.Genre.Update;

public class UpdateGenre(
    IGenreRepository genreRepository,
    IUnitOfWork unitOfWork,
    ICategoryRepository categoryRepository
) : IUpdateGenre
{
    public async Task<UpdateGenreOutput> Handle(
        UpdateGenreInput request,
        CancellationToken cancellationToken
    )
    {
        var genre = await genreRepository.Get(request.Id, cancellationToken);
        genre.Update(request.Name);
        if (request.IsActive.HasValue && request.IsActive.Value != genre.IsActive)
            if (request.IsActive.Value)
                genre.Activate();
            else
                genre.Deactivate();
        if (request.Categories is not null)
        {
            genre.RemoveAllCategories();
            if (request.Categories.Count > 0)
            {
                await ValidateRelatedCategories(request.Categories, cancellationToken);
                request.Categories.ForEach(genre.AddCategory);
            }
        }
        await genreRepository.Update(genre, cancellationToken);
        await unitOfWork.Commit(cancellationToken);
        return UpdateGenreOutput.FromGenre(genre);
    }

    private async Task ValidateRelatedCategories(
        List<Guid> categoryIds,
        CancellationToken cancellationToken
    )
    {
        var categories = await categoryRepository.ListIdsByIds(categoryIds, cancellationToken);
        if (categories.Count < categoryIds.Count)
        {
            var notFoundCategories = categoryIds.Except(categories);
            throw new RelatedEntityException(
                $"Related category ids not found {string.Join(", ", notFoundCategories)}"
            );
        }
    }
}
