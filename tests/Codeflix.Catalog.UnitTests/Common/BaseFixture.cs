using Bogus;

namespace Codeflix.Catalog.UnitTests.Common;

public abstract class BaseFixture
{
    protected BaseFixture()
    {
        Faker = new Faker("pt_BR");
    }

    public Faker Faker { get; set; }

    public bool GetRandomBoolean()
    {
        return new Random().NextDouble() < 0.5;
    }
}
