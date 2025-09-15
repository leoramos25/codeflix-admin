namespace Codeflix.Catalog.UnitTests.Application.Category.UpdateCategory;

public class UpdateCategoryTestDataGenerator
{
    public static IEnumerable<object[]> GetCategoriesToUpdate(int times = 10)
    {
        var fixture = new UpdateCategoryTestFixture();
        for (var index = 0; index < times; index++)
        {
            var category = fixture.GetValidCategory();
            var input = fixture.GetValidInput(category.Id);
            yield return [category, input];
        }
    }

    public static IEnumerable<object[]> GetInvalidInputs(int times = 12)
    {
        var fixture = new UpdateCategoryTestFixture();
        var invalidInputs = new List<object[]>();
        const int totalInvalidCases = 5;

        for (var i = 0; i < times; i++)
        {
            switch (i % totalInvalidCases)
            {
                case 0:
                    var invalidInputShortName = fixture.GetInvalidInputShortName();
                    invalidInputs.Add(
                        [invalidInputShortName, "Name should be at least 3 characters long"]
                    );
                    break;
                case 1:
                    var invalidInputLongName = fixture.GetInvalidInputLongName();
                    invalidInputs.Add(
                        [invalidInputLongName, "Name should be less or equal 255 characters long"]
                    );
                    break;
                case 2:
                    var invalidInputDescriptionLongName = fixture.GetInvalidInputDescriptionLong();
                    invalidInputs.Add(
                        [
                            invalidInputDescriptionLongName,
                            "Description should be less or equal 10000 characters long",
                        ]
                    );
                    break;
                case 3:
                    var invalidNullNameInput = fixture.GetInvalidInputNameNull();
                    invalidInputs.Add([invalidNullNameInput, "Name should not be empty or null"]);
                    break;
            }
        }

        return invalidInputs;
    }
}
