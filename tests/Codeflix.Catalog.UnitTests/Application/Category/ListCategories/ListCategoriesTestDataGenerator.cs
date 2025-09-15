using Codeflix.Catalog.Application.UseCases.Category.List;

namespace Codeflix.Catalog.UnitTests.Application.Category.ListCategories;

public class ListCategoriesTestDataGenerator
{
    public static IEnumerable<object[]> GetValidInputs(int times = 15)
    {
        var fixture = new ListCategoriesTestFixture();
        var input = fixture.GetValidInput();
        for (var i = 0; i < times; i++)
        {
            switch (i % 5)
            {
                case 0:
                    yield return [new ListCategoriesInput()];
                    break;
                case 1:
                    yield return [new ListCategoriesInput(input.Page)];
                    break;
                case 2:
                    yield return [new ListCategoriesInput(input.Page, input.PerPage)];
                    break;
                case 3:
                    yield return [new ListCategoriesInput(input.Page, input.PerPage, input.Search)];
                    break;
                case 4:
                    yield return
                    [
                        new ListCategoriesInput(
                            input.Page,
                            input.PerPage,
                            input.Search,
                            input.Sort
                        ),
                    ];
                    break;
                case 5:
                    yield return [input];
                    break;
            }
        }
    }
}
