using DomainEntity = Codeflix.Catalog.Domain.Entity;

namespace Codeflix.Catalog.Application.UseCases.Genre.Get;

public record GetGenreOutput(
    Guid Id,
    string Name,
    bool IsActive,
    IReadOnlyCollection<GetGenreCategoryOutput> Categories,
    DateTime CreatedAt
)
{
    public static GetGenreOutput FromGenre(DomainEntity.Genre genre)
    {
        return new GetGenreOutput(
            genre.Id,
            genre.Name,
            genre.IsActive,
            genre.Categories.Select(cat => new GetGenreCategoryOutput(cat)).ToList().AsReadOnly(),
            genre.CreatedAt
        );
    }

    public void FillCategoriesWithName(IReadOnlyCollection<DomainEntity.Category> categories)
    {
        foreach (var categoryOutput in Categories)
        {
            categoryOutput.Name = categories
                ?.FirstOrDefault(category => category.Id == categoryOutput.Id)
                ?.Name;
        }
    }
}

public class GetGenreCategoryOutput(Guid id, string? name = null)
{
    public Guid Id { get; set; } = id;
    public string? Name { get; set; } = name;
}
