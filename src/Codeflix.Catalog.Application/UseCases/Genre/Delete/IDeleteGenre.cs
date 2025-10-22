using MediatR;

namespace Codeflix.Catalog.Application.UseCases.Genre.Delete;

public interface IDeleteGenre : IRequestHandler<DeleteGenreInput>;
