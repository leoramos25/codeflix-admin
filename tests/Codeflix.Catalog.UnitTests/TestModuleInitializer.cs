using System.Globalization;
using System.Runtime.CompilerServices;

namespace Codeflix.Catalog.UnitTests;

public static class TestModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        var culture = new CultureInfo("en-US");

        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }
}
