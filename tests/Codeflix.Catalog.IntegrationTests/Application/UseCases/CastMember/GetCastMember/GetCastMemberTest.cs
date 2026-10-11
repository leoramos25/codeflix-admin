using Codeflix.Catalog.Application.Exceptions;
using Codeflix.Catalog.Infra.Data.EF.Repositories;
using FluentAssertions;
using UseCase = Codeflix.Catalog.Application.UseCases.CastMember.Get;

namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.CastMember.GetCastMember;

[Collection(nameof(GetCastMemberTestFixture))]
public class GetCastMemberTest(GetCastMemberTestFixture fixture)
{
    [Fact(DisplayName = nameof(GetCastMember))]
    [Trait("Integration/Application", "GetCastMember - Use Cases")]
    public async Task GetCastMember()
    {
        var castMembers = fixture.GetValidCastMembers();
        var castMember = castMembers[3];
        var arrangeDbContext = fixture.CreateDbContext();
        await arrangeDbContext.CastMembers.AddRangeAsync(castMembers, CancellationToken.None);
        await arrangeDbContext.SaveChangesAsync(CancellationToken.None);
        var useCase = new UseCase.GetCastMember(
            new CastMemberRepository(fixture.CreateDbContext(true))
        );
        var input = new UseCase.GetCastMemberInput(castMember.Id);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Should().NotBeNull();
        output.Id.Should().Be(castMember.Id);
        output.Name.Should().Be(castMember.Name);
        output.Type.Should().Be(castMember.Type);
        output.CreatedAt.Should().Be(castMember.CreatedAt);
    }

    [Fact(DisplayName = nameof(ThrowWhenCastMemberNotFound))]
    [Trait("Integration/Application", "GetCastMember - Use Cases")]
    public async Task ThrowWhenCastMemberNotFound()
    {
        var arrangeDbContext = fixture.CreateDbContext();
        await arrangeDbContext.CastMembers.AddRangeAsync(
            fixture.GetValidCastMembers(),
            CancellationToken.None
        );
        await arrangeDbContext.SaveChangesAsync(CancellationToken.None);
        var invalidId = Guid.NewGuid();
        var useCase = new UseCase.GetCastMember(
            new CastMemberRepository(fixture.CreateDbContext(true))
        );
        var input = new UseCase.GetCastMemberInput(invalidId);

        var action = () => useCase.Handle(input, CancellationToken.None);

        await action
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"CastMember {invalidId} not found.");
    }
}
