using Codeflix.Catalog.Application.UseCases.Genre.Update;
using Codeflix.Catalog.IntegrationTests.Application.UseCases.Genre.Common;

namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.Genre.UpdateGenre;

[CollectionDefinition(nameof(UpdateGenreTestFixture))]
public class UpdateGenreTestFixtureCollection : ICollectionFixture<UpdateGenreTestFixture>;

public class UpdateGenreTestFixture : GenreUseCaseTestFixture
{
    public UpdateGenreInput GetValidInput(Guid id, List<Guid>? categories = null) =>
        new(id, GetValidName(), GetRandomBoolean(), categories);
};
