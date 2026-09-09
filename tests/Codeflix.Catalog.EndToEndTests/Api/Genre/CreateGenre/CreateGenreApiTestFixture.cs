using Codeflix.Catalog.Application.UseCases.Genre.Create;
using Codeflix.Catalog.EndToEndTests.Api.Genre.Common;

namespace Codeflix.Catalog.EndToEndTests.Api.Genre.CreateGenre;

[CollectionDefinition(nameof(CreateGenreApiTestFixture))]
public class CreateGenreApiTestFixtureCollection : ICollectionFixture<CreateGenreApiTestFixture>;

public class CreateGenreApiTestFixture : GenreBaseFixture
{
    public CreateGenreInput GetValidInput(List<Guid>? categoryIds = null)
    {
        return new CreateGenreInput(GetValidName(), GetRandomBoolean(), categoryIds);
    }
}
