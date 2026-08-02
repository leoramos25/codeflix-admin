namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.Genre.GetGenre;

[CollectionDefinition(nameof(GetGenreTestFixture))]
public class GetGenreTestFixtureCollection : ICollectionFixture<GetGenreTestFixture>;

public class GetGenreTestFixture : Common.GenreUseCaseTestFixture { }
