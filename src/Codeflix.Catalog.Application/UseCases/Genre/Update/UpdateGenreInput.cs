using MediatR;

namespace Codeflix.Catalog.Application.UseCases.Genre.Update;

public record UpdateGenreInput(
    Guid Id,
    string Name,
    bool? IsActive = null,
    List<Guid>? Categories = null
) : IRequest<UpdateGenreOutput>;
