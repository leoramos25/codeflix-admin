using Codeflix.Catalog.IntegrationTests.Common;
using DomainEntity = Codeflix.Catalog.Domain.Entity;

namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.Genre.Common;

public class GenreUseCaseTestFixture : BaseFixture
{
    public DomainEntity.Genre GetValidGenre(bool? isActive = null)
    {
        return new DomainEntity.Genre(GetValidName(), isActive ?? GetRandomBoolean());
    }

    public string GetValidName()
    {
        return Faker.Music.Genre();
    }
}
