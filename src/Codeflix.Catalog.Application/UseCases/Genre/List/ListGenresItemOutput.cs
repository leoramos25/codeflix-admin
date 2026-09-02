namespace Codeflix.Catalog.Application.UseCases.Genre.List;

public record ListGenresItemOutput(
    Guid Id,
    string Name,
    bool IsActive,
    IReadOnlyCollection<CategoryItemOutput> Categories,
    DateTime CreatedAt
)
{
    public static ListGenresItemOutput FromGenre(Domain.Entity.Genre genre)
    {
        return new ListGenresItemOutput(
            genre.Id,
            genre.Name,
            genre.IsActive,
            genre
                .Categories.Select(categoryId => new CategoryItemOutput(categoryId))
                .ToList()
                .AsReadOnly(),
            genre.CreatedAt
        );
    }
}

public class CategoryItemOutput(Guid id, string? name = null)
{
    public Guid Id { get; set; } = id;
    public string? Name { get; set; } = name;    
}
