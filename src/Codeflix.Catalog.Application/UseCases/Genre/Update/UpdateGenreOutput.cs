namespace Codeflix.Catalog.Application.UseCases.Genre.Update;

public record UpdateGenreOutput(
    Guid Id,
    string Name,
    IReadOnlyCollection<Guid> Categories,
    bool IsActive,
    DateTime CreatedAt
)
{
    public static UpdateGenreOutput FromGenre(Domain.Entity.Genre genre)
    {
        return new UpdateGenreOutput(
            genre.Id,
            genre.Name,
            genre.Categories,
            genre.IsActive,
            genre.CreatedAt
        );
    }
}
