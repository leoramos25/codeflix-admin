using Codeflix.Catalog.Application.UseCases.Genre.Update;
using Codeflix.Catalog.UnitTests.Application.Genre.Common;

namespace Codeflix.Catalog.UnitTests.Application.Genre.UpdateGenre;

[CollectionDefinition(nameof(UpdateGenreTestFixture))]
public class UpdateGenreTestFixtureCollection : ICollectionFixture<UpdateGenreTestFixture>;

public class UpdateGenreTestFixture : GenreUseCaseBaseFixture
{
    public UpdateGenreInput GetValidInput(
        Guid id,
        bool? isActive = null,
        List<Guid>? categories = null
    ) => new(id, GetValidName(), isActive, categories);

    public UpdateGenreInput GetValidInputWithCategories(Guid id, bool? isActive = null)
    {
        var numberOfCategoryIds = new Random().Next(1, 10);
        var categories = Enumerable
            .Range(1, numberOfCategoryIds)
            .Select(_ => Guid.NewGuid())
            .ToList();
        return new(id, GetValidName(), isActive, categories);
    }
}
