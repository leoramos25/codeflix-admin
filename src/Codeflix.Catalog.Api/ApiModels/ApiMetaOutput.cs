namespace Codeflix.Catalog.Api.ApiModels;

public class ApiMetaOutput
{
    public ApiMetaOutput(int currentPage, int perPage, int total)
    {
        CurrentPage = currentPage;
        PerPage = perPage;
        Total = total;
    }

    public int CurrentPage { get; private set; }
    public int PerPage { get; private set; }
    public int Total { get; private set; }
}