using Codeflix.Catalog.Api.ApiModels.Genre;
using Codeflix.Catalog.EndToEndTests.Api.Genre.Common;

namespace Codeflix.Catalog.EndToEndTests.Api.Genre.UpdateGenre;

[CollectionDefinition(nameof(UpdateGenreApiTestFixture))]
public class UpdateGenreApiTestFixtureCollection : ICollectionFixture<UpdateGenreApiTestFixture>;

public class UpdateGenreApiTestFixture : GenreBaseFixture
{
    public UpdateGenreApiInput GetValidInput(List<Guid>? categoryIds = null)
    {
        return new UpdateGenreApiInput(GetValidName(), GetRandomBoolean(), categoryIds);
    }
}
