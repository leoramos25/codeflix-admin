using Codeflix.Catalog.Application.Exceptions;
using FluentAssertions;
using Moq;
using UseCases = Codeflix.Catalog.Application.UseCases.CastMember.Get;

namespace Codeflix.Catalog.UnitTests.Application.CastMember.GetCastMember;

[Collection(nameof(GetCastMemberTestFixture))]
public class GetCastMemberTest(GetCastMemberTestFixture fixture)
{
    [Fact(DisplayName = nameof(Get))]
    [Trait("Application", "Get Cast Member - Use Cases")]
    public async Task Get()
    {
        var castMember = fixture.GetValidCastMember();
        var castMemberRepositoryMock = fixture.GetCastMemberRepositoryMock();
        var useCase = new UseCases.GetCastMember(castMemberRepositoryMock.Object);
        var input = new UseCases.GetCastMemberInput(castMember.Id);
        castMemberRepositoryMock
            .Setup(repo =>
                repo.Get(It.Is<Guid>(x => x == castMember.Id), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(castMember);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Id.Should().Be(castMember.Id);
        output.Name.Should().Be(castMember.Name);
        output.Type.Should().Be(castMember.Type);
        output.CreatedAt.Should().BeSameDateAs(castMember.CreatedAt);
        castMemberRepositoryMock.Verify(
            repo => repo.Get(It.Is<Guid>(x => x == castMember.Id), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact(DisplayName = nameof(ThrowExceptionWhenCastMemberNotFound))]
    [Trait("Application", "Get Cast Member - Use Cases")]
    public async Task ThrowExceptionWhenCastMemberNotFound()
    {
        var castMember = fixture.GetValidCastMember();
        var castMemberRepositoryMock = fixture.GetCastMemberRepositoryMock();
        var useCase = new UseCases.GetCastMember(castMemberRepositoryMock.Object);
        var input = new UseCases.GetCastMemberInput(castMember.Id);
        castMemberRepositoryMock
            .Setup(repo => repo.Get(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"CastMember {castMember.Id} not found"));

        var action = () => useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>();
        castMemberRepositoryMock.Verify(
            repo => repo.Get(It.Is<Guid>(x => x == castMember.Id), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
