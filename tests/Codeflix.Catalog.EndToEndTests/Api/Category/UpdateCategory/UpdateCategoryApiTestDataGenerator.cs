namespace Codeflix.Catalog.EndToEndTests.Api.Category.UpdateCategory;

public class UpdateCategoryApiTestDataGenerator
{
    public static IEnumerable<object[]> GetInvalidData(int times = 3)
    {
        var fixture = new UpdateCategoryApiTestFixture();
        var invalidInputs = new List<object[]>();
        const int totalInvalidCases = 3;

        for (var i = 0; i < times; i++)
        {
            switch (i % totalInvalidCases)
            {
                case 0:
                    var invalidInputShortName = fixture.GetValidInput();
                    invalidInputShortName.Name = fixture.GetInvalidInputShortName();
                    invalidInputs.Add(
                        [invalidInputShortName, "Name should be at least 3 characters long"]
                    );
                    break;
                case 1:
                    var invalidInputLongName = fixture.GetValidInput();
                    invalidInputLongName.Name = fixture.GetInvalidInputLongName();
                    invalidInputs.Add(
                        [invalidInputLongName, "Name should be less or equal 255 characters long"]
                    );
                    break;
                case 2:
                    var invalidInputDescriptionLongName = fixture.GetValidInput();
                    invalidInputDescriptionLongName.Description =
                        fixture.GetInvalidInputDescriptionLong();
                    invalidInputs.Add(
                        [
                            invalidInputDescriptionLongName,
                            "Description should be less or equal 10000 characters long",
                        ]
                    );
                    break;
            }
        }

        return invalidInputs;
    }
}
