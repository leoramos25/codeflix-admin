using Codeflix.Catalog.Domain.Repository;
using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;

namespace Codeflix.Catalog.Application.UseCases.Genre.List;

public class ListGenres(IGenreRepository genreRepository) : IListGenres
{
    public async Task<ListGenresOutput> Handle(
        ListGenresInput request,
        CancellationToken cancellationToken
    )
    {
        var searchOutput = await genreRepository.Search(request.ToSearchInput(), cancellationToken);
        return new ListGenresOutput(
            request.Page,
            searchOutput.PerPage,
            searchOutput.Total,
            searchOutput.Items.Select(ListGenresItemOutput.FromGenre).ToList()
        );
    }
}
