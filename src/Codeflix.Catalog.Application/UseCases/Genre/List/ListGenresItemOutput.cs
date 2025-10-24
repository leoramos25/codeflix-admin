namespace Codeflix.Catalog.Application.UseCases.Genre.List;

public record ListGenresItemOutput(
    Guid Id,
    string Name,
    bool IsActive,
    IReadOnlyCollection<Guid> Categories,
    DateTime CreatedAt
)
{
    public static ListGenresItemOutput FromGenre(Domain.Entity.Genre genre) =>
        new(genre.Id, genre.Name, genre.IsActive, genre.Categories, genre.CreatedAt);
}
