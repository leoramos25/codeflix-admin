using Codeflix.Catalog.Application.Interfaces;
using Codeflix.Catalog.Domain.Repository;

namespace Codeflix.Catalog.Application.UseCases.Genre.Delete;

public class DeleteGenre(IGenreRepository genreRepository, IUnitOfWork unitOfWork) : IDeleteGenre
{
    public async Task Handle(DeleteGenreInput request, CancellationToken cancellationToken)
    {
        var genre = await genreRepository.Get(request.Id, cancellationToken);
        await genreRepository.Delete(genre, cancellationToken);
        await unitOfWork.Commit(cancellationToken);
    }
}
