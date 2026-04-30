using Codeflix.Catalog.Domain.Exceptions;
using FluentAssertions;
using DomainEntity = Codeflix.Catalog.Domain.Entity;

namespace Codeflix.Catalog.UnitTests.Domain.Entity.Genre;

[Collection(nameof(GenreTestFixture))]
public class GenreTest(GenreTestFixture fixture)
{
    [Fact(DisplayName = nameof(Instantiate))]
    [Trait("Domain", "Genre - Aggregate")]
    public void Instantiate()
    {
        var validName = fixture.GetValidName();
        var genre = new DomainEntity.Genre(validName);

        genre.Should().NotBeNull();
        genre.Name.Should().Be(validName);
        genre.IsActive.Should().BeTrue();
        genre.CreatedAt.Should().NotBeSameDateAs(default);
    }

    [Theory(DisplayName = nameof(InstantiateWithIsActive))]
    [Trait("Domain", "Genre - Aggregate")]
    [InlineData(true)]
    [InlineData(false)]
    public void InstantiateWithIsActive(bool isActive)
    {
        var validName = fixture.GetValidName();
        var genre = new DomainEntity.Genre(validName, isActive);

        genre.Should().NotBeNull();
        genre.Name.Should().Be(validName);
        genre.IsActive.Should().Be(isActive);
        genre.CreatedAt.Should().NotBeSameDateAs(default);
    }

    [Theory(DisplayName = nameof(Activate))]
    [Trait("Domain", "Genre - Aggregate")]
    [InlineData(true)]
    [InlineData(false)]
    public void Activate(bool isActive)
    {
        var validName = fixture.GetValidName();
        var genre = new DomainEntity.Genre(validName, isActive);

        genre.Activate();

        genre.Should().NotBeNull();
        genre.Name.Should().Be(validName);
        genre.IsActive.Should().BeTrue();
        genre.CreatedAt.Should().NotBeSameDateAs(default);
    }

    [Theory(DisplayName = nameof(Deactivate))]
    [Trait("Domain", "Genre - Aggregate")]
    [InlineData(true)]
    [InlineData(false)]
    public void Deactivate(bool isActive)
    {
        var validName = fixture.GetValidName();
        var genre = new DomainEntity.Genre(validName, isActive);

        genre.Deactivate();

        genre.Should().NotBeNull();
        genre.Name.Should().Be(validName);
        genre.IsActive.Should().BeFalse();
        genre.CreatedAt.Should().NotBeSameDateAs(default);
    }

    [Fact(DisplayName = nameof(Update))]
    [Trait("Domain", "Genre - Aggregate")]
    public void Update()
    {
        var genre = fixture.GetValidGenre();
        var input = new { Name = fixture.GetValidName() };
        genre.Update(input.Name);

        genre.Name.Should().Be(input.Name);
        genre.IsActive.Should().Be(genre.IsActive);
        genre.CreatedAt.Should().NotBeSameDateAs(default);
    }

    [Theory(DisplayName = nameof(InstantiateShouldThrowWhenNameIsInvalid))]
    [Trait("Domain", "Genre - Aggregate")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void InstantiateShouldThrowWhenNameIsInvalid(string? invalidName)
    {
        var action = () => new DomainEntity.Genre(invalidName!);

        action
            .Should()
            .Throw<EntityValidationException>()
            .WithMessage($"{nameof(DomainEntity.Genre.Name)} should not be empty or null");
    }

    [Theory(DisplayName = nameof(InstantiateShouldThrowExceptionWhenNameIsInvalid))]
    [Trait("Domain", "Genre - Aggregate")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void InstantiateShouldThrowExceptionWhenNameIsInvalid(string invalidName)
    {
        var genre = fixture.GetValidGenre();

        var action = () => genre.Update(invalidName);

        action
            .Should()
            .Throw<EntityValidationException>()
            .WithMessage($"{nameof(DomainEntity.Genre.Name)} should not be empty or null");
    }

    [Fact(DisplayName = nameof(AddCategory))]
    [Trait("Domain", "Genre - Aggregate")]
    public void AddCategory()
    {
        var genre = fixture.GetValidGenre();
        var categoryId = Guid.NewGuid();

        genre.AddCategory(categoryId);

        genre.Categories.Should().Contain(categoryId);
    }

    [Fact(DisplayName = nameof(AddManyCategories))]
    [Trait("Domain", "Genre - Aggregate")]
    public void AddManyCategories()
    {
        var genre = fixture.GetValidGenre();
        var categoryIds = Enumerable.Range(0, 5).Select(_ => Guid.NewGuid()).ToList();

        categoryIds.ForEach(categoryId => genre.AddCategory(categoryId));

        genre.Categories.Should().BeEquivalentTo(categoryIds);
    }

    [Fact(DisplayName = nameof(RemoveCategory))]
    [Trait("Domain", "Genre - Aggregate")]
    public void RemoveCategory()
    {
        var categoryIds = Enumerable.Range(0, 2).Select(_ => Guid.NewGuid()).ToList();
        var categoryId = Guid.NewGuid();
        categoryIds.Add(categoryId);
        var genre = fixture.GetValidGenre(categoryIds);

        genre.Categories.Should().Contain(categoryId);

        genre.RemoveCategory(categoryId);

        genre.Categories.Should().NotContain(categoryId);
    }

    [Fact(DisplayName = nameof(RemoveAllCategories))]
    [Trait("Domain", "Genre - Aggregate")]
    public void RemoveAllCategories()
    {
        var categoryIds = Enumerable.Range(0, 3).Select(_ => Guid.NewGuid()).ToList();
        var genre = fixture.GetValidGenre(categoryIds);

        genre.RemoveAllCategories();

        genre.Categories.Should().BeEmpty();
    }
}
