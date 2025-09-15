using Codeflix.Catalog.Domain.Exceptions;
using FluentAssertions;
using DomainEntity = Codeflix.Catalog.Domain.Entity;

namespace Codeflix.Catalog.UnitTests.Domain.Entity.Category;

[Collection(nameof(CategoryTestFixture))]
public class CategoryTest(CategoryTestFixture fixture)
{
    [Fact(DisplayName = nameof(Instantiate))]
    [Trait("Domain", "Category - Aggregates")]
    public void Instantiate()
    {
        var validCategory = fixture.GetValidCategory();

        var category = new DomainEntity.Category(validCategory.Name, validCategory.Description);

        category.Should().NotBeNull();
        category.Name.Should().Be(validCategory.Name);
        category.Description.Should().Be(validCategory.Description);
        category.Id.Should().NotBe(Guid.Empty);
        category.CreatedAt.Should().NotBe(default);
        category.IsActive.Should().BeTrue();
    }

    [Theory(DisplayName = nameof(InstantiateWithIsActiveStatus))]
    [Trait("Domain", "Category - Aggregates")]
    [InlineData(true)]
    [InlineData(false)]
    public void InstantiateWithIsActiveStatus(bool isActive)
    {
        var validCategory = fixture.GetValidCategory();

        var category = new DomainEntity.Category(
            validCategory.Name,
            validCategory.Description,
            isActive
        );

        category.Should().NotBeNull();
        category.Name.Should().Be(validCategory.Name);
        category.Description.Should().Be(validCategory.Description);
        category.Id.Should().NotBe(Guid.Empty);
        category.CreatedAt.Should().NotBeSameDateAs(default);
        category.IsActive.Should().Be(isActive);
    }

    [Theory(DisplayName = nameof(ThrowExceptionWhenNameIsEmpty))]
    [Trait("Domain", "Category - Aggregates")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    public void ThrowExceptionWhenNameIsEmpty(string? name)
    {
        var validCategory = fixture.GetValidCategory();
        var action = () => new DomainEntity.Category(name!, validCategory.Description);

        action
            .Should()
            .Throw<EntityValidationException>()
            .WithMessage("Name should not be empty or null");
    }

    [Fact(DisplayName = nameof(ThrowExceptionWhenDescriptionIsNull))]
    [Trait("Domain", "Category - Aggregates")]
    public void ThrowExceptionWhenDescriptionIsNull()
    {
        var validCategory = fixture.GetValidCategory();
        var action = () => new DomainEntity.Category(validCategory.Name, null!);

        action
            .Should()
            .Throw<EntityValidationException>()
            .WithMessage("Description should not be null");
    }

    [Theory(DisplayName = nameof(InstantiateThrowExceptionWhenNameIsLess3Characters))]
    [Trait("Domain", "Category - Aggregates")]
    [InlineData("ca")]
    [InlineData("a")]
    [InlineData("10")]
    public void InstantiateThrowExceptionWhenNameIsLess3Characters(string invalidName)
    {
        var validCategory = fixture.GetValidCategory();
        var action = () => new DomainEntity.Category(invalidName, validCategory.Description);

        action
            .Should()
            .Throw<EntityValidationException>()
            .WithMessage("Name should be at least 3 characters long");
    }

    [Fact(DisplayName = nameof(InstantiateThrowExceptionWhenNameGreaterThan255Characters))]
    [Trait("Domain", "Category - Aggregates")]
    public void InstantiateThrowExceptionWhenNameGreaterThan255Characters()
    {
        var validCategory = fixture.GetValidCategory();
        var invalidName = string.Join(null, Enumerable.Range(1, 256).Select(_ => "a").ToArray());
        var action = () => new DomainEntity.Category(invalidName, validCategory.Description);

        action
            .Should()
            .Throw<EntityValidationException>()
            .WithMessage("Name should be less or equal 255 characters long");
    }

    [Fact(
        DisplayName = nameof(InstantiateThrowExceptionWhenDescriptionGreaterThan10_000Characters)
    )]
    [Trait("Domain", "Category - Aggregates")]
    public void InstantiateThrowExceptionWhenDescriptionGreaterThan10_000Characters()
    {
        var validCategory = fixture.GetValidCategory();
        var invalidDescription = string.Join(
            null,
            Enumerable.Range(1, 10_001).Select(_ => "a").ToArray()
        );
        var action = () => new DomainEntity.Category(validCategory.Name, invalidDescription);

        action
            .Should()
            .Throw<EntityValidationException>()
            .WithMessage("Description should be less or equal 10000 characters long");
    }

    [Fact(DisplayName = nameof(Activate))]
    [Trait("Domain", "Category - Aggregates")]
    public void Activate()
    {
        var validCategory = fixture.GetValidCategory();

        var category = new DomainEntity.Category(
            validCategory.Name,
            validCategory.Description,
            isActive: false
        );
        category.Activate();

        category.IsActive.Should().BeTrue();
    }

    [Fact(DisplayName = nameof(Deactivate))]
    [Trait("Domain", "Category - Aggregates")]
    public void Deactivate()
    {
        var validCategory = fixture.GetValidCategory();

        var category = new DomainEntity.Category(validCategory.Name, validCategory.Description);
        category.Deactivate();

        category.IsActive.Should().BeFalse();
    }

    [Fact(DisplayName = nameof(Update))]
    [Trait("Domain", "Category - Aggregates")]
    public void Update()
    {
        var category = fixture.GetValidCategory();
        var newData = new
        {
            Name = fixture.GetValidCategoryName(),
            Description = fixture.GetValidCategoryDescription(),
        };

        category.Update(newData.Name, newData.Description);

        category.Name.Should().Be(newData.Name);
        category.Description.Should().Be(newData.Description);
    }

    [Fact(DisplayName = nameof(Update))]
    [Trait("Domain", "Category - Aggregates")]
    public void UpdateOnlyName()
    {
        var category = fixture.GetValidCategory();
        var newData = new { Name = fixture.GetValidCategoryName() };
        var currentDescription = category.Description;

        category.Update(newData.Name);

        category.Name.Should().Be(newData.Name);
        category.Description.Should().Be(currentDescription);
    }

    [Theory(DisplayName = nameof(UpdateThrowExceptionWhenNameIsEmpty))]
    [Trait("Domain", "Category - Aggregates")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    public void UpdateThrowExceptionWhenNameIsEmpty(string? name)
    {
        var category = fixture.GetValidCategory();
        var action = () => category.Update(name!);

        action
            .Should()
            .Throw<EntityValidationException>()
            .WithMessage("Name should not be empty or null");
    }

    [Theory(DisplayName = nameof(UpdateThrowExceptionWhenNameIsLess3Characters))]
    [Trait("Domain", "Category - Aggregates")]
    [MemberData(nameof(GetNameWithLessThan3Characters), parameters: 10)]
    public void UpdateThrowExceptionWhenNameIsLess3Characters(string invalidName)
    {
        var category = fixture.GetValidCategory();
        var action = () => category.Update(invalidName);

        action
            .Should()
            .Throw<EntityValidationException>()
            .WithMessage("Name should be at least 3 characters long");
    }

    public static IEnumerable<object[]> GetNameWithLessThan3Characters(int numberOfTests)
    {
        var fixture = new CategoryTestFixture();
        for (var i = 0; i < numberOfTests; i++)
        {
            var isOdd = i % 2 == 1;
            yield return [fixture.GetValidCategoryName()[..(isOdd ? 1 : 2)]];
        }
    }

    [Fact(DisplayName = nameof(UpdateThrowExceptionWhenNameGreaterThan255Characters))]
    [Trait("Domain", "Category - Aggregates")]
    public void UpdateThrowExceptionWhenNameGreaterThan255Characters()
    {
        var invalidName = fixture.Faker.Lorem.Letter(256);
        var category = fixture.GetValidCategory();
        var action = () => category.Update(invalidName);

        action
            .Should()
            .Throw<EntityValidationException>()
            .WithMessage("Name should be less or equal 255 characters long");
    }

    [Fact(DisplayName = nameof(UpdateThrowExceptionWhenDescriptionGreaterThan10_000Characters))]
    [Trait("Domain", "Category - Aggregates")]
    public void UpdateThrowExceptionWhenDescriptionGreaterThan10_000Characters()
    {
        var invalidDescription = fixture.Faker.Commerce.ProductDescription();
        while (invalidDescription.Length <= 10_000)
            invalidDescription += fixture.Faker.Commerce.ProductDescription();
        var category = fixture.GetValidCategory();
        var action = () => category.Update(category.Name, invalidDescription);

        action
            .Should()
            .Throw<EntityValidationException>()
            .WithMessage("Description should be less or equal 10000 characters long");
    }
}
