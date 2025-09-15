namespace Codeflix.Catalog.IntegrationTests.Application.UseCases.Category.CreateCategory;

public class CreateCategoryTestDataGenerator
{
    public static IEnumerable<object[]> GetInvalidInputs(int times = 15)
    {
        var fixture = new CreateCategoryTestFixture();
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
                    var invalidNullDescriptionInput = fixture.GetInvalidInputDescriptionNull();
                    invalidInputs.Add(
                        [invalidNullDescriptionInput, "Description should not be null"]
                    );
                    break;
                case 4:
                    var invalidNullNameInput = fixture.GetInvalidInputNameNull();
                    invalidInputs.Add([invalidNullNameInput, "Name should not be empty or null"]);
                    break;
            }
        }

        return invalidInputs;
    }
}
