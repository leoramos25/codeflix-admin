# Domain templates

Replace `<Aggregate>` with the singular entity name. These are the shapes to copy;
grow them one behaviour at a time under the TDD cycle, never all at once.

## Entity — simple aggregate

`src/Codeflix.Catalog.Domain/Entity/<Aggregate>.cs`

```csharp
using Codeflix.Catalog.Domain.SeedWork;
using Codeflix.Catalog.Domain.Validation;

namespace Codeflix.Catalog.Domain.Entity;

public class <Aggregate> : AggregateRoot
{
    private const int MinNameSize = 3;
    private const int MaxNameSize = 255;

    public <Aggregate>(string name, bool isActive = true)
    {
        Name = name;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
        Validate();
    }

    public string Name { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

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

    public void Update(string name)
    {
        Name = name;
        Validate();
    }

    private void Validate()
    {
        DomainValidation.NotNullOrEmpty(Name, nameof(Name));
        DomainValidation.MinLength(Name, MinNameSize, nameof(Name));
        DomainValidation.MaxLength(Name, MaxNameSize, nameof(Name));
    }
}
```

## Entity — aggregate with a relation

Add to the class above. `<Relation>` is the singular related aggregate name.

```csharp
    private readonly List<Guid> _<relation>s;

    // in the constructor, before Validate():
    _<relation>s = [];

    public IReadOnlyCollection<Guid> <Relation>s => _<relation>s.AsReadOnly();

    public void Add<Relation>(Guid <relation>Id)
    {
        _<relation>s.Add(<relation>Id);
        Validate();
    }

    public void Remove<Relation>(Guid <relation>Id)
    {
        _<relation>s.Remove(<relation>Id);
        Validate();
    }

    public void RemoveAll<Relation>s()
    {
        _<relation>s.Clear();
        Validate();
    }
```

The list is never returned, never passed out, never assigned after the constructor.
Callers that need the ids get the read-only view.

## Repository interface

`src/Codeflix.Catalog.Domain/Repository/I<Aggregate>Repository.cs`

```csharp
using Codeflix.Catalog.Domain.Entity;
using Codeflix.Catalog.Domain.SeedWork;
using Codeflix.Catalog.Domain.SeedWork.SearchableRepository;

namespace Codeflix.Catalog.Domain.Repository;

public interface I<Aggregate>Repository
    : IGenericRepository<<Aggregate>>, ISearchableRepository<<Aggregate>>;
```

With an extra read needed by another aggregate's use case:

```csharp
public interface I<Aggregate>Repository
    : IGenericRepository<<Aggregate>>, ISearchableRepository<<Aggregate>>
{
    Task<IReadOnlyCollection<Guid>> ListIdsByIds(
        List<Guid> ids,
        CancellationToken cancellationToken
    );

    Task<IReadOnlyCollection<<Aggregate>>> ListByIds(
        List<Guid> ids,
        CancellationToken cancellationToken
    );
}
```

## Test fixture

`tests/Codeflix.Catalog.UnitTests/Domain/Entity/<Aggregate>/<Aggregate>TestFixture.cs`

Collection definition and fixture in the same file, never split.

```csharp
using Codeflix.Catalog.UnitTests.Common;

namespace Codeflix.Catalog.UnitTests.Domain.Entity.<Aggregate>;

[CollectionDefinition(nameof(<Aggregate>TestFixture))]
public class <Aggregate>TestFixtureCollection : ICollectionFixture<<Aggregate>TestFixture>;

public class <Aggregate>TestFixture : BaseFixture
{
    public string GetValidName()
    {
        return Faker.Commerce.ProductName();
    }

    public Catalog.Domain.Entity.<Aggregate> GetValid<Aggregate>(List<Guid>? <relation>Ids = null)
    {
        var <aggregate> = new Catalog.Domain.Entity.<Aggregate>(GetValidName(), GetRandomBoolean());
        <relation>Ids?.ForEach(<aggregate>.Add<Relation>);
        return <aggregate>;
    }
}
```

`GetRandomBoolean()` comes from `BaseFixture`. Do not redeclare it.

## Test class

`tests/Codeflix.Catalog.UnitTests/Domain/Entity/<Aggregate>/<Aggregate>Test.cs`

```csharp
using Codeflix.Catalog.Domain.Exceptions;
using FluentAssertions;
using DomainEntity = Codeflix.Catalog.Domain.Entity;

namespace Codeflix.Catalog.UnitTests.Domain.Entity.<Aggregate>;

[Collection(nameof(<Aggregate>TestFixture))]
public class <Aggregate>Test(<Aggregate>TestFixture fixture)
{
    [Fact(DisplayName = nameof(Instantiate))]
    [Trait("Domain", "<Aggregate> - Aggregate")]
    public void Instantiate()
    {
        var validName = fixture.GetValidName();

        var <aggregate> = new DomainEntity.<Aggregate>(validName);

        <aggregate>.Should().NotBeNull();
        <aggregate>.Id.Should().NotBeEmpty();
        <aggregate>.Name.Should().Be(validName);
        <aggregate>.IsActive.Should().BeTrue();
        <aggregate>.CreatedAt.Should().NotBeSameDateAs(default);
    }

    [Theory(DisplayName = nameof(InstantiateShouldThrowWhenNameIsInvalid))]
    [Trait("Domain", "<Aggregate> - Aggregate")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void InstantiateShouldThrowWhenNameIsInvalid(string? name)
    {
        var action = () => new DomainEntity.<Aggregate>(name!);

        action
            .Should()
            .Throw<EntityValidationException>()
            .WithMessage("Name should not be empty or null");
    }

    [Fact(DisplayName = nameof(Add<Relation>))]
    [Trait("Domain", "<Aggregate> - Aggregate")]
    public void Add<Relation>()
    {
        var <aggregate> = fixture.GetValid<Aggregate>();
        var <relation>Id = Guid.NewGuid();

        <aggregate>.Add<Relation>(<relation>Id);

        <aggregate>.<Relation>s.Should().HaveCount(1);
        <aggregate>.<Relation>s.Should().Contain(<relation>Id);
    }
}
```

`DisplayName` must be `nameof` the method itself — the `--filter "DisplayName=X"`
workflow depends on it.

## Encapsulation smells to reject in review

| Smell | Correct form |
|---|---|
| `public string Name { get; set; }` | `{ get; private set; }` |
| `public List<Guid> Categories { get; }` | private backing field + `IReadOnlyCollection` view |
| `public void Validate()` | `private void Validate()` |
| `entity.Categories.Add(id)` from outside | `entity.AddCategory(id)` |
| a mutator that does not end in `Validate()` | append `Validate()` |
| `Guid id` accepted by the constructor | `Id` is assigned by `Entity` |
| `DateTime.Now` | `DateTime.UtcNow` |
| `[Required]`, `[MaxLength]` or any EF attribute | `DomainValidation` inside `Validate()` |
