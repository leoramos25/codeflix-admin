using Codeflix.Catalog.UnitTests.Application.Category.Common;

namespace Codeflix.Catalog.UnitTests.Application.Category.DeleteCategory;

[CollectionDefinition(nameof(DeleteCategoryTestFixture))]
public class DeleteCategoryTestFixtureCollection : ICollectionFixture<DeleteCategoryTestFixture>;

public class DeleteCategoryTestFixture : CategoryUseCaseBaseFixture;
