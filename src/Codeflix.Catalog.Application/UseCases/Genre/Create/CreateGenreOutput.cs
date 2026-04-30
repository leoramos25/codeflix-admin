namespace Codeflix.Catalog.Application.UseCases.Genre.Create;

public record CreateGenreOutput(
    Guid Id,
    string Name,
    bool? IsActive,
    IReadOnlyCollection<Guid> Categories,
    DateTime CreatedAt
)
{
    public static CreateGenreOutput FromGenre(Domain.Entity.Genre genre)
    {
        return new CreateGenreOutput(
            genre.Id,
            genre.Name,
            genre.IsActive,
            genre.Categories,
            genre.CreatedAt
        );
    }
}
