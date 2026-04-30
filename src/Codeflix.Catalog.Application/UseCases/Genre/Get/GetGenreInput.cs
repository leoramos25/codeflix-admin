using MediatR;

namespace Codeflix.Catalog.Application.UseCases.Genre.Get;

public record GetGenreInput(Guid Id) : IRequest<GetGenreOutput>;
