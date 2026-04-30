namespace Codeflix.Catalog.Application.UseCases.Category.List;

public class ListCategoriesItemOutput
{
    public ListCategoriesItemOutput(
        Guid id,
        string name,
        string description,
        bool isActive,
        DateTime createdAt
    )
    {
        Id = id;
        Name = name;
        Description = description;
        IsActive = isActive;
        CreatedAt = createdAt;
    }

    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public static ListCategoriesItemOutput FromCategory(Domain.Entity.Category category)
    {
        return new ListCategoriesItemOutput(
            category.Id,
            category.Name,
            category.Description,
            category.IsActive,
            category.CreatedAt
        );
    }
}
