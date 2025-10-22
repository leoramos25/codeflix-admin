using MediatR;

namespace Codeflix.Catalog.Application.UseCases.Genre.Delete;

public record DeleteGenreInput(Guid Id) : IRequest;
