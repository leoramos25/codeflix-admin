using Codeflix.Catalog.Application.Exceptions;
using Codeflix.Catalog.Domain.Exceptions;
using FluentAssertions;
using Moq;
using DomainEntity = Codeflix.Catalog.Domain.Entity;
using UseCases = Codeflix.Catalog.Application.UseCases.CastMember.Update;

namespace Codeflix.Catalog.UnitTests.Application.CastMember.UpdateCastMember;

[Collection(nameof(UpdateCastMemberTestFixture))]
public class UpdateCastMemberTest(UpdateCastMemberTestFixture fixture)
{
    [Fact(DisplayName = nameof(Update))]
    [Trait("Application", "Update Cast Member - Use Cases")]
    public async Task Update()
    {
        var castMember = fixture.GetValidCastMember();
        var unitOfWork = fixture.GetUnitOfWorkMock();
        var castMemberRepository = fixture.GetCastMemberRepositoryMock();
        var useCase = new UseCases.UpdateCastMember(castMemberRepository.Object, unitOfWork.Object);
        var input = new UseCases.UpdateCastMemberInput(
            castMember.Id,
            fixture.GetValidName(),
            fixture.GetRandomCastMemberType()
        );
        castMemberRepository
            .Setup(repo =>
                repo.Get(It.Is<Guid>(x => x == castMember.Id), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(castMember);

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Id.Should().NotBeEmpty();
        output.Name.Should().Be(input.Name);
        output.Type.Should().Be(input.Type);
        output.CreatedAt.Should().NotBeSameDateAs(default);
        castMemberRepository.Verify(
            repo => repo.Get(It.Is<Guid>(x => x == castMember.Id), It.IsAny<CancellationToken>()),
            Times.Once
        );
        castMemberRepository.Verify(
            repo => repo.Update(It.IsAny<DomainEntity.CastMember>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        unitOfWork.Verify(uow => uow.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory(DisplayName = nameof(ThrowExceptionWhenInvalidName))]
    [Trait("Application", "Update Cast Member - Use Cases")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ThrowExceptionWhenInvalidName(string? name)
    {
        var castMember = fixture.GetValidCastMember();
        var unitOfWork = fixture.GetUnitOfWorkMock();
        var castMemberRepository = fixture.GetCastMemberRepositoryMock();
        var useCase = new UseCases.UpdateCastMember(castMemberRepository.Object, unitOfWork.Object);
        var input = new UseCases.UpdateCastMemberInput(
            castMember.Id,
            name!,
            fixture.GetRandomCastMemberType()
        );
        castMemberRepository
            .Setup(repo =>
                repo.Get(It.Is<Guid>(x => x == castMember.Id), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(castMember);

        var action = () => useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<EntityValidationException>();
        castMemberRepository.Verify(
            repo => repo.Get(It.Is<Guid>(x => x == castMember.Id), It.IsAny<CancellationToken>()),
            Times.Once
        );
        castMemberRepository.Verify(
            repo => repo.Update(It.IsAny<DomainEntity.CastMember>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        unitOfWork.Verify(uow => uow.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = nameof(ThrowExceptionWhenCastMemberNotFound))]
    [Trait("Application", "Update Cast Member - Use Cases")]
    public async Task ThrowExceptionWhenCastMemberNotFound()
    {
        var castMember = fixture.GetValidCastMember();
        var unitOfWork = fixture.GetUnitOfWorkMock();
        var castMemberRepository = fixture.GetCastMemberRepositoryMock();
        var useCase = new UseCases.UpdateCastMember(castMemberRepository.Object, unitOfWork.Object);
        var input = new UseCases.UpdateCastMemberInput(
            castMember.Id,
            fixture.GetValidName(),
            fixture.GetRandomCastMemberType()
        );
        castMemberRepository
            .Setup(repo => repo.Get(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"CastMember {castMember.Id} not found"));

        var action = () => useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<NotFoundException>();
        castMemberRepository.Verify(
            repo => repo.Get(It.Is<Guid>(x => x == castMember.Id), It.IsAny<CancellationToken>()),
            Times.Once
        );
        castMemberRepository.Verify(
            repo => repo.Update(It.IsAny<DomainEntity.CastMember>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        unitOfWork.Verify(uow => uow.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }
}
