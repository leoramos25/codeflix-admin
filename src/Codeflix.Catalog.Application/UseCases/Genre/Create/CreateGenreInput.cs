using MediatR;

namespace Codeflix.Catalog.Application.UseCases.Genre.Create;

public record CreateGenreInput(string Name, bool IsActive = true, List<Guid>? Categories = null)
    : IRequest<CreateGenreOutput>;
