namespace Codeflix.Catalog.Application.UseCases.Genre.Get;

public record GetGenreOutput(
    Guid Id,
    string Name,
    bool IsActive,
    IReadOnlyCollection<GetGenreCategoryOutput> Categories,
    DateTime CreatedAt
)
{
    public static GetGenreOutput FromGenre(Domain.Entity.Genre genre)
    {
        return new GetGenreOutput(
            genre.Id,
            genre.Name,
            genre.IsActive,
            genre.Categories.Select(cat => new GetGenreCategoryOutput(cat)).ToList().AsReadOnly(),
            genre.CreatedAt
        );
    }
}

public record GetGenreCategoryOutput(Guid Id, string? Name = null)
{
    public static GetGenreCategoryOutput Create(Guid id, string? name)
    => new GetGenreCategoryOutput(id, name);

};
