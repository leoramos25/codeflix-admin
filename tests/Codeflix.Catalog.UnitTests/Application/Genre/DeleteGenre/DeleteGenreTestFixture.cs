using Codeflix.Catalog.UnitTests.Application.Genre.Common;

namespace Codeflix.Catalog.UnitTests.Application.Genre.DeleteGenre;

[CollectionDefinition(nameof(DeleteGenreTestFixture))]
public class DeleteGenreTestFixtureCollection : ICollectionFixture<DeleteGenreTestFixture>;

public class DeleteGenreTestFixture : GenreUseCaseBaseFixture { }
