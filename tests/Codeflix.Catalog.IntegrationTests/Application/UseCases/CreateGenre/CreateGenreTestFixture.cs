using Codeflix.Catalog.IntegrationTests.Application.UseCases.Genre.Common;

[CollectionDefinition(nameof(CreateGenreTestFixture))]
public class CreateGenreTestFixtureCollection : ICollectionFixture<CreateGenreTestFixture>;

public class CreateGenreTestFixture : GenreUseCaseTestFixture {}