using Codeflix.Catalog.Application.Exceptions;
using Codeflix.Catalog.Infra.Data.EF;
using Codeflix.Catalog.Infra.Data.EF.Repositories;
using FluentAssertions;
using UseCase = Codeflix.Catalog.Application.UseCases.CastMember.Delete;

namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.CastMember.DeleteCastMember;

[Collection(nameof(DeleteCastMemberTestFixture))]
public class DeleteCastMemberTest(DeleteCastMemberTestFixture fixture)
{
    [Fact(DisplayName = nameof(DeleteCastMember))]
    [Trait("Integration/Application", "DeleteCastMember - Use Cases")]
    public async Task DeleteCastMember()
    {
        var castMembers = fixture.GetValidCastMembers();
        var castMember = castMembers[3];
        var arrangeDbContext = fixture.CreateDbContext();
        await arrangeDbContext.CastMembers.AddRangeAsync(castMembers, CancellationToken.None);
        await arrangeDbContext.SaveChangesAsync(CancellationToken.None);
        var actDbContext = fixture.CreateDbContext(true);
        var useCase = new UseCase.DeleteCastMember(
            new CastMemberRepository(actDbContext),
            new UnitOfWork(actDbContext)
        );
        var input = new UseCase.DeleteCastMemberInput(castMember.Id);

        await useCase.Handle(input, CancellationToken.None);

        var assertDbContext = fixture.CreateDbContext(true);
        var castMemberFromDb = await assertDbContext.CastMembers.FindAsync(castMember.Id);
        castMemberFromDb.Should().BeNull();
        assertDbContext.CastMembers.Should().HaveCount(castMembers.Count - 1);
    }

    [Fact(DisplayName = nameof(ThrowWhenCastMemberNotFound))]
    [Trait("Integration/Application", "DeleteCastMember - Use Cases")]
    public async Task ThrowWhenCastMemberNotFound()
    {
        var castMembers = fixture.GetValidCastMembers();
        var arrangeDbContext = fixture.CreateDbContext();
        await arrangeDbContext.CastMembers.AddRangeAsync(castMembers, CancellationToken.None);
        await arrangeDbContext.SaveChangesAsync(CancellationToken.None);
        var actDbContext = fixture.CreateDbContext(true);
        var useCase = new UseCase.DeleteCastMember(
            new CastMemberRepository(actDbContext),
            new UnitOfWork(actDbContext)
        );
        var invalidId = Guid.NewGuid();
        var input = new UseCase.DeleteCastMemberInput(invalidId);

        var action = () => useCase.Handle(input, CancellationToken.None);

        await action
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"CastMember {invalidId} not found.");
        var assertDbContext = fixture.CreateDbContext(true);
        assertDbContext.CastMembers.Should().HaveCount(castMembers.Count);
    }
}
