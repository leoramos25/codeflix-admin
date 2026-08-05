using Codeflix.Catalog.Application.UseCases.Genre.Create;
using Codeflix.Catalog.IntegrationTests.Application.UseCases.Genre.Common;

namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.Genre.CreateGenre;

[CollectionDefinition(nameof(CreateGenreTestFixture))]
public class CreateGenreTestFixtureCollection : ICollectionFixture<CreateGenreTestFixture>;

public class CreateGenreTestFixture : GenreUseCaseTestFixture
{
    public CreateGenreInput GetValidInput(List<Guid>? categories = null) => new CreateGenreInput(Faker.Music.Genre(), GetRandomBoolean(), categories);

};