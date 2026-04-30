using Codeflix.Catalog.IntegrationTests.Application.UseCases.Category.Common;

namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.Category.GetCategory;

[CollectionDefinition(nameof(GetCategoryTestFixture))]
public class GetCategoryTestFixtureCollection : ICollectionFixture<GetCategoryTestFixture>;

public class GetCategoryTestFixture : CategoryUseCaseBaseFixture;
