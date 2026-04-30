using MediatR;

namespace Codeflix.Catalog.Application.UseCases.Genre.Get;

public interface IGetGenre : IRequestHandler<GetGenreInput, GetGenreOutput>;
