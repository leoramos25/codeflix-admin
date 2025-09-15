using Codeflix.Catalog.EndToEndTests.Api.Category.Common;

namespace Codeflix.Catalog.EndToEndTests.Api.Category.GetCategory;

[CollectionDefinition(nameof(GetCategoryApiTestFixture))]
public class GetCategoryApiTestFixtureCollection : IClassFixture<GetCategoryApiTestFixture>;

public class GetCategoryApiTestFixture : CategoryBaseFixture
{
    public List<Domain.Entity.Category> GetCategories(int size = 10) =>
        Enumerable.Range(0, size).Select(_ => GetValidCategory()).ToList();
}
