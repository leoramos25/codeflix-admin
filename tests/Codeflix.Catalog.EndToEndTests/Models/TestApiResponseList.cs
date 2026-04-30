namespace Codeflix.Catalog.EndToEndTests.Models;

public class TestApiResponseList<TOutputItem> : TestApiResponse<List<TOutputItem>>
{
    public TestApiResponseList(List<TOutputItem> data)
        : base(data) { }

    public TestApiResponseList() { }

    public TestApiResponseList(List<TOutputItem> data, TestApiResponseListMeta meta)
        : base(data)
    {
        Meta = meta;
    }

    public TestApiResponseListMeta? Meta { get; set; }
}

public class TestApiResponseListMeta
{
    public TestApiResponseListMeta() { }

    public TestApiResponseListMeta(int currentPage, int perPage, int total)
    {
        CurrentPage = currentPage;
        PerPage = perPage;
        Total = total;
    }

    public int CurrentPage { get; set; }
    public int PerPage { get; set; }
    public int Total { get; set; }
}

public class TestApiResponse<TOutput>
{
    public TestApiResponse() { }

    public TestApiResponse(TOutput data)
    {
        Data = data;
    }

    public TOutput? Data { get; set; }
}
