using Codeflix.Catalog.Application.Interfaces;
using Codeflix.Catalog.Domain.Exceptions;
using Codeflix.Catalog.Domain.Repository;

namespace Codeflix.Catalog.Application.UseCases.Genre.Create;

public class CreateGenre(
    IGenreRepository genreRepository,
    IUnitOfWork unitOfWork,
    ICategoryRepository categoryRepository
) : ICreateGenre
{
    public async Task<CreateGenreOutput> Handle(
        CreateGenreInput request,
        CancellationToken cancellationToken
    )
    {
        var genre = new Domain.Entity.Genre(request.Name, request.IsActive);
        if (request.Categories is not null && request.Categories.Count > 0)
        {
            await ValidateRelatedCategories(request.Categories, cancellationToken);
            request.Categories.ForEach(genre.AddCategory);
        }
        await genreRepository.Insert(genre, cancellationToken);
        await unitOfWork.Commit(cancellationToken);
        return CreateGenreOutput.FromGenre(genre);
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
