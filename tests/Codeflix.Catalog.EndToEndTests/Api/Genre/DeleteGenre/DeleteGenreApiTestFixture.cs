using Codeflix.Catalog.EndToEndTests.Api.Genre.Common;

namespace Codeflix.Catalog.EndToEndTests.Api.Genre.DeleteGenre;

[CollectionDefinition(nameof(DeleteGenreApiTestFixture))]
public class DeleteGenreApiTestFixtureCollection : ICollectionFixture<DeleteGenreApiTestFixture>;

public class DeleteGenreApiTestFixture : GenreBaseFixture;
