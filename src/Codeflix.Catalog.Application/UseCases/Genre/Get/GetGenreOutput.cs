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
            genre
                .Categories.Distinct()
                .Select(categoryId => new GetGenreCategoryOutput(categoryId))
                .ToList()
                .AsReadOnly(),
            genre.CreatedAt
        );
    }

    public void FillCategoriesWithName(IReadOnlyCollection<DomainEntity.Category> categories)
    {
        var namesById = categories
            .DistinctBy(category => category.Id)
            .ToDictionary(category => category.Id, category => category.Name);
        foreach (var categoryOutput in Categories)
            categoryOutput.Name = namesById.GetValueOrDefault(categoryOutput.Id);
    }
}

public class GetGenreCategoryOutput(Guid id, string? name = null)
{
    public Guid Id { get; set; } = id;
    public string? Name { get; set; } = name;
}
