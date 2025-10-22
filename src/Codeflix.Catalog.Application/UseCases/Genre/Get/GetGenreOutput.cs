namespace Codeflix.Catalog.Application.UseCases.Genre.Get;

public record GetGenreOutput(
    Guid Id,
    string Name,
    bool IsActive,
    IReadOnlyCollection<Guid> Categories,
    DateTime CreatedAt
)
{
    public static GetGenreOutput FromGenre(Domain.Entity.Genre genre) =>
        new(genre.Id, genre.Name, genre.IsActive, genre.Categories, genre.CreatedAt);
}
