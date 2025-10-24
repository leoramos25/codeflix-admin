namespace Codeflix.Catalog.Application.UseCases.Category.Update;

public class UpdateCategoryOutput
{
    public UpdateCategoryOutput(
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

    public static UpdateCategoryOutput FromCategory(Domain.Entity.Category category)
    {
        return new UpdateCategoryOutput(
            category.Id,
            category.Name,
            category.Description,
            category.IsActive,
            category.CreatedAt
        );
    }
}