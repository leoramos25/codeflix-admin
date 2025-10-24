namespace Codeflix.Catalog.Api.ApiModels;

public class ApiOutput<TData>
    where TData : class
{
    public ApiOutput(TData data)
    {
        Data = data;
    }

    public TData Data { get; private set; }
}