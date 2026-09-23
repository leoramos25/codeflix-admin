using FluentAssertions;
using Moq;
using UseCases = Codeflix.Catalog.Application.UseCases.CastMember.Create;
using DomainEntity = Codeflix.Catalog.Domain.Entity;
using Codeflix.Catalog.Domain.Exceptions;

namespace Codeflix.Catalog.UnitTests.Application.CastMember.CreateCastMember;

[Collection(nameof(CreateCastMemberTestFixture))]
public class CreateCastMemberTest(CreateCastMemberTestFixture fixture)
{
    [Fact(DisplayName = nameof(Create))]
    [Trait("Application", "Create Cast Member - Use Cases")]
    public async Task Create()
    {
        var castMemberRepositoryMock = fixture.GetCastMemberRepositoryMock();
        var unitOfWorkMock = fixture.GetUnitOfWorkMock();
        var useCase = new UseCases.CreateCastMember(
            castMemberRepositoryMock.Object,
            unitOfWorkMock.Object
        );
        var input = new UseCases.CreateCastMemberInput(
            fixture.GetValidName(),
            fixture.GetRandomCastMemberType()
        );

        var output = await useCase.Handle(input, CancellationToken.None);

        output.Id.Should().NotBeEmpty();
        output.Name.Should().Be(input.Name);
        output.Type.Should().Be(input.Type);
        output.CreatedAt.Should().NotBeSameDateAs(default);
        castMemberRepositoryMock.Verify(repo =>
            repo.Insert(
                It.IsAny<DomainEntity.CastMember>(),
                It.IsAny<CancellationToken>()
            )
        );
        unitOfWorkMock.Verify(uow => uow.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory(DisplayName = nameof(ThrowWhenInvalidName))]
    [Trait("Application", "Create Cast Member - Use Cases")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ThrowWhenInvalidName(string? name)
    {
        var castMemberRepositoryMock = fixture.GetCastMemberRepositoryMock();
        var unitOfWorkMock = fixture.GetUnitOfWorkMock();
        var useCase = new UseCases.CreateCastMember(
            castMemberRepositoryMock.Object,
            unitOfWorkMock.Object
        );
        var input = new UseCases.CreateCastMemberInput(
            name!,
            fixture.GetRandomCastMemberType()
        );

        var action = () => useCase.Handle(input, CancellationToken.None);

        await action.Should().ThrowAsync<EntityValidationException>();
        castMemberRepositoryMock.Verify(repo =>
            repo.Insert(
                It.IsAny<DomainEntity.CastMember>(),
                It.IsAny<CancellationToken>()
            ), Times.Never
        );
        unitOfWorkMock.Verify(uow => uow.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }
}
