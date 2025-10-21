using Codeflix.Catalog.Application.UseCases.Genre.Create;
using Codeflix.Catalog.UnitTests.Application.Genre.Common;

namespace Codeflix.Catalog.UnitTests.Application.Genre.CreateGenre;

[CollectionDefinition(nameof(CreateGenreTestFixture))]
public class CreateGenreTestFixtureCollection : ICollectionFixture<CreateGenreTestFixture>;

public class CreateGenreTestFixture : GenreUseCaseBaseFixture
{
    public CreateGenreInput GetValidInput() => new(GetValidName(), GetRandomBoolean());

    public CreateGenreInput GetValidInputWithCategories()
    {
        var numberOfCategoryIds = new Random().Next(1, 10);
        var categories = Enumerable
            .Range(1, numberOfCategoryIds)
            .Select(_ => Guid.NewGuid())
            .ToList();
        return new CreateGenreInput(GetValidName(), GetRandomBoolean(), categories);
    }
}
