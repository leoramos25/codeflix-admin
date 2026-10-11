using Codeflix.Catalog.Domain.Exceptions;
using Codeflix.Catalog.Infra.Data.EF;
using Codeflix.Catalog.Infra.Data.EF.Repositories;
using FluentAssertions;
using UseCase = Codeflix.Catalog.Application.UseCases.CastMember.Create;

namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.CastMember.CreateCastMember;

[Collection(nameof(CreateCastMemberTestFixture))]
public class CreateCastMemberTest(CreateCastMemberTestFixture fixture)
{
    [Fact(DisplayName = nameof(CreateCastMember))]
    [Trait("Integration/Application", "CreateCastMember - Use Cases")]
    public async Task CreateCastMember()
    {
        var dbContext = fixture.CreateDbContext();
        var useCase = new UseCase.CreateCastMember(
            new CastMemberRepository(dbContext),
            new UnitOfWork(dbContext)
        );
        var input = fixture.GetValidInput();

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().NotBeEmpty();
        output.Name.Should().Be(input.Name);
        output.Type.Should().Be(input.Type);
        output.CreatedAt.Should().NotBe(default);
        var assertDbContext = fixture.CreateDbContext(true);
        var castMemberFromDb = await assertDbContext.CastMembers.FindAsync(output.Id);
        castMemberFromDb.Should().NotBeNull();
        castMemberFromDb!.Name.Should().Be(input.Name);
        castMemberFromDb.Type.Should().Be(input.Type);
        castMemberFromDb.CreatedAt.Should().Be(output.CreatedAt);
    }

    [Theory(DisplayName = nameof(ThrowWhenNameIsInvalid))]
    [Trait("Integration/Application", "CreateCastMember - Use Cases")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ThrowWhenNameIsInvalid(string? invalidName)
    {
        var dbContext = fixture.CreateDbContext();
        var useCase = new UseCase.CreateCastMember(
            new CastMemberRepository(dbContext),
            new UnitOfWork(dbContext)
        );
        var input = new UseCase.CreateCastMemberInput(
            invalidName!,
            fixture.GetRandomCastMemberType()
        );

        var action = () => useCase.Handle(input, CancellationToken.None);

        await action
            .Should()
            .ThrowAsync<EntityValidationException>()
            .WithMessage("Name should not be empty or null");
        var assertDbContext = fixture.CreateDbContext(true);
        assertDbContext.CastMembers.Should().BeEmpty();
    }
}
