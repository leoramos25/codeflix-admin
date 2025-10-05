namespace Codeflix.Catalog.Api.ApiModels;

public class ApiOutput<TData>
    where TData : class
{
    public TData Data { get; private set; }

    public ApiOutput(TData data)
    {
        Data = data;
    }
}
