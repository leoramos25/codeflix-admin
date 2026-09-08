using Codeflix.Catalog.Domain.Repository;

namespace Codeflix.Catalog.Application.UseCases.Genre.Get;

public class GetGenre(IGenreRepository genreRepository, ICategoryRepository categoryRepository)
    : IGetGenre
{
    public async Task<GetGenreOutput> Handle(
        GetGenreInput request,
        CancellationToken cancellationToken
    )
    {
        var genre = await genreRepository.Get(request.Id, cancellationToken);
        var output = GetGenreOutput.FromGenre(genre);
        if (output.Categories.Count > 0)
        {
            var categories = await categoryRepository.ListByIds(
                [.. output.Categories.Select(category => category.Id)],
                cancellationToken
            );
            output.FillCategoriesWithName(categories);
        }
        return output;
    }
}
