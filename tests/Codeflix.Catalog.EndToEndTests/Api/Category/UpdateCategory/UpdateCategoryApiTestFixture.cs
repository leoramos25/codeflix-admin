using Codeflix.Catalog.Api.ApiModels.Category;
using Codeflix.Catalog.EndToEndTests.Api.Category.Common;

namespace Codeflix.Catalog.EndToEndTests.Api.Category.UpdateCategory;

[CollectionDefinition(nameof(UpdateCategoryApiTestFixture))]
public class UpdateCategoryApiTestFixtureCollection
    : ICollectionFixture<UpdateCategoryApiTestFixture>;

public class UpdateCategoryApiTestFixture : CategoryBaseFixture
{
    public UpdateCategoryApiInput GetValidInput()
    {
        return new UpdateCategoryApiInput(GetValidCategoryName(), GetValidCategoryDescription(), GetRandomBoolean());
    }
}