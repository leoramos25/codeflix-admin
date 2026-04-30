using MediatR;

namespace Codeflix.Catalog.Application.UseCases.Genre.Create;

public interface ICreateGenre : IRequestHandler<CreateGenreInput, CreateGenreOutput>;
