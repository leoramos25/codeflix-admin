namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.Genre.GetGenre;

[CollectionDefinition(nameof(GetGenreTestFixture))]
public class GetGenreTestFixtureCollection : ICollectionFixture<GetGenreTestFixtureCollection>;

public class GetGenreTestFixture : Common.GenreUseCaseTestFixture { }
