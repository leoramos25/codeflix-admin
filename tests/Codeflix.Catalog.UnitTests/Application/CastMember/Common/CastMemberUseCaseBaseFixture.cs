using Codeflix.Catalog.Application.Interfaces;
using Codeflix.Catalog.Domain.Enum;
using Codeflix.Catalog.Domain.Repository;
using Codeflix.Catalog.UnitTests.Common;
using Moq;
using DomainEntity = Codeflix.Catalog.Domain.Entity;

namespace Codeflix.Catalog.UnitTests.Application.CastMember.Common;

public class CastMemberUseCaseBaseFixture : BaseFixture
{
    public Mock<ICastMemberRepository> GetCastMemberRepositoryMock() => new();

    public Mock<IUnitOfWork> GetUnitOfWorkMock() => new();

    public DomainEntity.CastMember GetValidCastMember() =>
        new(GetValidName(), GetRandomCastMemberType());

    public string GetValidName() => Faker.Name.FullName();

    public CastMemberType GetRandomCastMemberType() => (CastMemberType)new Random().Next(1, 2);
}
