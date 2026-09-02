using Codeflix.Catalog.Domain.Repository;

namespace Codeflix.Catalog.Application.UseCases.Genre.List;

public class ListGenres(IGenreRepository genreRepository, ICategoryRepository categoryRepository)
    : IListGenres
{
    public async Task<ListGenresOutput> Handle(
        ListGenresInput request,
        CancellationToken cancellationToken
    )
    {
        var searchOutput = await genreRepository.Search(request.ToSearchInput(), cancellationToken);
        var output = ListGenresOutput.FromSearchOutput(searchOutput);
        var relatedCategoryIds = searchOutput
            .Items.SelectMany(x => x.Categories)
            .Distinct()
            .ToList();
        if (relatedCategoryIds.Count > 0)
        {
            var categories = await categoryRepository.ListByIds(
                relatedCategoryIds,
                cancellationToken
            );
            output.FillCategoriesWithName(categories);
        }
        return output;
    }
}
