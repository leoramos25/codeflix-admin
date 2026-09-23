using Codeflix.Catalog.Application.Exceptions;
using FluentAssertions;
using Moq;
using DomainEntity = Codeflix.Catalog.Domain.Entity;
using UseCases = Codeflix.Catalog.Application.UseCases.CastMember.Delete;

namespace Codeflix.Catalog.UnitTests.Application.CastMember.DeleteCastMember;

[Collection(nameof(DeleteCastMemberTestFixture))]
public class DeleteCastMemberTest(DeleteCastMemberTestFixture fixture)
{
    [Fact(DisplayName = nameof(Delete))]
    [Trait("Application", "Delete Cast Member - Use Cases")]
    public async Task Delete()
    {
        var castMember = fixture.GetValidCastMember();
        var unitOfWorkMock = fixture.GetUnitOfWorkMock();
        var castMemberRepositoryMock = fixture.GetCastMemberRepositoryMock();
        var useCase = new UseCases.DeleteCastMember(
            castMemberRepositoryMock.Object,
            unitOfWorkMock.Object
        );
        var input = new UseCases.DeleteCastMemberInput(castMember.Id);
        castMemberRepositoryMock
            .Setup(repo =>
                repo.Get(It.Is<Guid>(x => x == castMember.Id), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(castMember);

        await useCase.Handle(input, CancellationToken.None);

        castMemberRepositoryMock.Verify(
            repo => repo.Get(It.Is<Guid>(x => x == castMember.Id), It.IsAny<CancellationToken>()),
            Times.Once
        );
        castMemberRepositoryMock.Verify(
            repo => repo.Delete(It.IsAny<DomainEntity.CastMember>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        unitOfWorkMock.Verify(uow => uow.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = nameof(ThrowExceptionWhenCastMemberNotFound))]
    [Trait("Application", "Delete Cast Member - Use Cases")]
    public async Task ThrowExceptionWhenCastMemberNotFound()
    {
        var castMember = fixture.GetValidCastMember();
        var unitOfWorkMock = fixture.GetUnitOfWorkMock();
        var castMemberRepositoryMock = fixture.GetCastMemberRepositoryMock();
        var useCase = new UseCases.DeleteCastMember(
            castMemberRepositoryMock.Object,
            unitOfWorkMock.Object
        );
        var input = new UseCases.DeleteCastMemberInput(castMember.Id);
        castMemberRepositoryMock
            .Setup(repo =>
                repo.Get(It.Is<Guid>(x => x == castMember.Id), It.IsAny<CancellationToken>())
            )
            .ThrowsAsync(new NotFoundException($"CastMember {castMember.Id} not found"));

        var action = () => useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>();
        castMemberRepositoryMock.Verify(
            repo => repo.Get(It.Is<Guid>(x => x == castMember.Id), It.IsAny<CancellationToken>()),
            Times.Once
        );
        castMemberRepositoryMock.Verify(
            repo => repo.Delete(It.IsAny<DomainEntity.CastMember>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        unitOfWorkMock.Verify(uow => uow.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }
}
