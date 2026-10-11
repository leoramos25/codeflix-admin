using Codeflix.Catalog.Application.Exceptions;
using Codeflix.Catalog.Domain.Exceptions;
using Codeflix.Catalog.Infra.Data.EF;
using Codeflix.Catalog.Infra.Data.EF.Repositories;
using FluentAssertions;
using UseCase = Codeflix.Catalog.Application.UseCases.CastMember.Update;

namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.CastMember.UpdateCastMember;

[Collection(nameof(UpdateCastMemberTestFixture))]
public class UpdateCastMemberTest(UpdateCastMemberTestFixture fixture)
{
    [Fact(DisplayName = nameof(UpdateCastMember))]
    [Trait("Integration/Application", "UpdateCastMember - Use Cases")]
    public async Task UpdateCastMember()
    {
        var castMembers = fixture.GetValidCastMembers();
        var castMember = castMembers[3];
        var arrangeDbContext = fixture.CreateDbContext();
        await arrangeDbContext.CastMembers.AddRangeAsync(castMembers, CancellationToken.None);
        await arrangeDbContext.SaveChangesAsync(CancellationToken.None);
        var actDbContext = fixture.CreateDbContext(true);
        var useCase = new UseCase.UpdateCastMember(
            new CastMemberRepository(actDbContext),
            new UnitOfWork(actDbContext)
        );
        var input = new UseCase.UpdateCastMemberInput(
            castMember.Id,
            fixture.GetValidName(),
            fixture.GetRandomCastMemberType()
        );

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().Be(castMember.Id);
        output.Name.Should().Be(input.Name);
        output.Type.Should().Be(input.Type);
        output.CreatedAt.Should().Be(castMember.CreatedAt);
        var assertDbContext = fixture.CreateDbContext(true);
        var castMemberFromDb = await assertDbContext.CastMembers.FindAsync(castMember.Id);
        castMemberFromDb.Should().NotBeNull();
        castMemberFromDb!.Name.Should().Be(input.Name);
        castMemberFromDb.Type.Should().Be(input.Type);
        castMemberFromDb.CreatedAt.Should().Be(castMember.CreatedAt);
    }

    [Fact(DisplayName = nameof(ThrowWhenCastMemberNotFound))]
    [Trait("Integration/Application", "UpdateCastMember - Use Cases")]
    public async Task ThrowWhenCastMemberNotFound()
    {
        var arrangeDbContext = fixture.CreateDbContext();
        await arrangeDbContext.CastMembers.AddRangeAsync(
            fixture.GetValidCastMembers(),
            CancellationToken.None
        );
        await arrangeDbContext.SaveChangesAsync(CancellationToken.None);
        var actDbContext = fixture.CreateDbContext(true);
        var useCase = new UseCase.UpdateCastMember(
            new CastMemberRepository(actDbContext),
            new UnitOfWork(actDbContext)
        );
        var invalidId = Guid.NewGuid();
        var input = new UseCase.UpdateCastMemberInput(
            invalidId,
            fixture.GetValidName(),
            fixture.GetRandomCastMemberType()
        );

        var action = () => useCase.Handle(input, CancellationToken.None);

        await action
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"CastMember {invalidId} not found.");
    }

    [Theory(DisplayName = nameof(ThrowWhenNameIsInvalid))]
    [Trait("Integration/Application", "UpdateCastMember - Use Cases")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ThrowWhenNameIsInvalid(string? invalidName)
    {
        var castMember = fixture.GetValidCastMember();
        var arrangeDbContext = fixture.CreateDbContext();
        await arrangeDbContext.CastMembers.AddAsync(castMember, CancellationToken.None);
        await arrangeDbContext.SaveChangesAsync(CancellationToken.None);
        var actDbContext = fixture.CreateDbContext(true);
        var useCase = new UseCase.UpdateCastMember(
            new CastMemberRepository(actDbContext),
            new UnitOfWork(actDbContext)
        );
        var input = new UseCase.UpdateCastMemberInput(
            castMember.Id,
            invalidName!,
            fixture.GetRandomCastMemberType()
        );

        var action = () => useCase.Handle(input, CancellationToken.None);

        await action
            .Should()
            .ThrowAsync<EntityValidationException>()
            .WithMessage("Name should not be empty or null");
        var assertDbContext = fixture.CreateDbContext(true);
        var castMemberFromDb = await assertDbContext.CastMembers.FindAsync(castMember.Id);
        castMemberFromDb.Should().NotBeNull();
        castMemberFromDb!.Name.Should().Be(castMember.Name);
        castMemberFromDb.Type.Should().Be(castMember.Type);
    }
}
