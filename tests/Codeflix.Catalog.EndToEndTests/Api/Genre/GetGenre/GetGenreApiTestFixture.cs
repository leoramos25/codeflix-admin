using Codeflix.Catalog.EndToEndTests.Api.Genre.Common;

namespace Codeflix.Catalog.EndToEndTests.Api.Genre.GetGenre;

[CollectionDefinition(nameof(GetGenreApiTestFixture))]
public class GetGenreApiTestFixtureCollection : ICollectionFixture<GetGenreApiTestFixture>;

public class GetGenreApiTestFixture : GenreBaseFixture;
