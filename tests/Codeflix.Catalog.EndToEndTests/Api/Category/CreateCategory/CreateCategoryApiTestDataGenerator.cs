namespace Codeflix.Catalog.EndToEndTests.Api.Category.CreateCategory;

public class CreateCategoryApiTestDataGenerator
{
    public static IEnumerable<object[]> GetInvalidInputs()
    {
        var fixture = new CreateCategoryApiTestFixture();
        var invalidInputs = new List<object[]>();
        const int totalInvalidCases = 4;

        for (var i = 0; i < totalInvalidCases; i++)
        {
            var input = fixture.GetValidInput();
            switch (i % totalInvalidCases)
            {
                case 0:
                    var invalidInputShortName = fixture.GetInvalidInputShortName();
                    input.Name = invalidInputShortName;
                    invalidInputs.Add([input, "Name should be at least 3 characters long"]);
                    break;
                case 1:
                    var invalidInputLongName = fixture.GetInvalidInputLongName();
                    input.Name = invalidInputLongName;
                    invalidInputs.Add([input, "Name should be less or equal 255 characters long"]);
                    break;
                case 2:
                    var invalidInputDescriptionLongName = fixture.GetInvalidInputDescriptionLong();
                    input.Description = invalidInputDescriptionLongName;
                    invalidInputs.Add(
                        [input, "Description should be less or equal 10000 characters long"]
                    );
                    break;
                case 3:
                    input.Name = " ";
                    invalidInputs.Add([input, "Name should not be empty or null"]);
                    break;
            }
        }

        return invalidInputs;
    }
}
