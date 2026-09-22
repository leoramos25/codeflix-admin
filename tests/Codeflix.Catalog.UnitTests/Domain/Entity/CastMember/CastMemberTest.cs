using Codeflix.Catalog.Domain.Exceptions;
using FluentAssertions;
using DomainEntity = Codeflix.Catalog.Domain.Entity;

namespace Codeflix.Catalog.UnitTests.Domain.Entity.CastMember;

[Collection(nameof(CastMemberTestFixture))]
public class CastMemberTest(CastMemberTestFixture fixture)
{
    [Fact(DisplayName = nameof(Instantiate))]
    [Trait("Domain", "CastMember - Aggregates")]
    public void Instantiate()
    {
        var name = fixture.GetValidName();
        var type = fixture.GetRandomCastMemberType();
        var castMember = new DomainEntity.CastMember(name, type);

        castMember.Id.Should().NotBeEmpty();
        castMember.Name.Should().Be(name);
        castMember.Type.Should().Be(type);
        castMember.CreatedAt.Should().NotBeSameDateAs(default);
    }

    [Theory(DisplayName = nameof(ThrowErrorWhenInvalidName))]
    [Trait("Domain", "CastMember - Aggregates")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ThrowErrorWhenInvalidName(string? name)
    {
        var action = () => new DomainEntity.CastMember(name!, fixture.GetRandomCastMemberType());

        action
            .Should()
            .Throw<EntityValidationException>()
            .WithMessage($"{nameof(DomainEntity.CastMember.Name)} should not be empty or null");
    }

    [Fact(DisplayName = nameof(Update))]
    [Trait("Domain", "CastMembers - Aggregates")]
    public void Update()
    {
        var castMember = fixture.GetValidCastMember();
        var newName = fixture.GetValidName();
        var newType = fixture.GetRandomCastMemberType();

        castMember.Update(newName, newType);

        castMember.Id.Should().NotBeEmpty();
        castMember.Name.Should().Be(newName);
        castMember.Type.Should().Be(newType);
        castMember.CreatedAt.Should().NotBeSameDateAs(default);
    }

    [Theory(DisplayName = nameof(UpdateThrowWhenInvalidName))]
    [Trait("Domain", "CastMembers - Aggregates")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void UpdateThrowWhenInvalidName(string? newName)
    {
        var castMember = fixture.GetValidCastMember();
        var newCastMemberType = fixture.GetRandomCastMemberType();

        var action = () => castMember.Update(newName!, newCastMemberType);

        action
            .Should()
            .Throw<EntityValidationException>()
            .WithMessage($"{nameof(DomainEntity.CastMember.Name)} should not be empty or null");
    }
}
