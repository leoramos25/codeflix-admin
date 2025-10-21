using MediatR;

namespace Codeflix.Catalog.Application.UseCases.Genre.Update;

public interface IUpdateGenre : IRequestHandler<UpdateGenreInput, UpdateGenreOutput>;
