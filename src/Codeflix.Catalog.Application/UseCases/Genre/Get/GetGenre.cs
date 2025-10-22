using Codeflix.Catalog.Domain.Repository;

namespace Codeflix.Catalog.Application.UseCases.Genre.Get;

public class GetGenre(IGenreRepository genreRepository) : IGetGenre
{
    public async Task<GetGenreOutput> Handle(
        GetGenreInput request,
        CancellationToken cancellationToken
    )
    {
        var genre = await genreRepository.Get(request.Id, cancellationToken);
        return GetGenreOutput.FromGenre(genre);
    }
}
