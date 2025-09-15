using Codeflix.Catalog.Application.UseCases.Category.Create;
using Codeflix.Catalog.EndToEndTests.Api.Category.Common;

namespace Codeflix.Catalog.EndToEndTests.Api.Category.CreateCategory;

[CollectionDefinition(nameof(CreateCategoryApiTestFixture))]
public class CreateCategoryApiTestFixtureCollection
    : ICollectionFixture<CreateCategoryApiTestFixture>;

public class CreateCategoryApiTestFixture : CategoryBaseFixture
{
    public CreateCategoryInput GetValidInput() =>
        new(GetValidCategoryName(), GetValidCategoryDescription(), GetRandomBoolean());
}
