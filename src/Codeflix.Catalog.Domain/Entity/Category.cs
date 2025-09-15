using Codeflix.Catalog.Domain.SeedWork;
using Codeflix.Catalog.Domain.Validation;

namespace Codeflix.Catalog.Domain.Entity;

public class Category : AggregateRoot
{
    private const int MinNameSize = 3;
    private const int MaxNameSize = 255;
    private const int MaxDescriptionSize = 10_000;

    public string Name { get; private set; }
    public string Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Category(string name, string description, bool isActive = true)
        : base()
    {
        Name = name;
        Description = description;
        IsActive = isActive;
        CreatedAt = DateTime.Now;
        Validate();
    }

    public void Activate()
    {
        IsActive = true;
        Validate();
    }

    public void Deactivate()
    {
        IsActive = false;
        Validate();
    }

    public void Update(string name, string? description = null)
    {
        Name = name;
        Description = description ?? Description;
        Validate();
    }

    private void Validate()
    {
        DomainValidation.NotNullOrEmpty(Name, nameof(Name));
        DomainValidation.MinLength(Name, MinNameSize, nameof(Name));
        DomainValidation.MaxLength(Name, MaxNameSize, nameof(Name));
        DomainValidation.NotNull(Description, nameof(Description));
        DomainValidation.MaxLength(Description, MaxDescriptionSize, nameof(Description));
    }
}
