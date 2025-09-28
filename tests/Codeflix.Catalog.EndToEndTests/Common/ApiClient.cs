using System.Net.Http.Json;
using System.Text.Json;
using System.Web;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Codeflix.Catalog.EndToEndTests.Common;

public class ApiClient(HttpClient httpClient)
{
    public async Task<(HttpResponseMessage, TOutput?)> Post<TOutput>(
        string url,
        object payload,
        CancellationToken cancellationToken
    )
        where TOutput : class
    {
        var response = await httpClient.PostAsJsonAsync(url, payload, cancellationToken);
        var output = await GetOutput<TOutput>(response, cancellationToken);
        return (response, output);
    }

    public async Task<(HttpResponseMessage, TOutput?)> Get<TOutput>(
        string url,
        CancellationToken cancellationToken,
        object? query = null
    )
        where TOutput : class
    {
        var route = PrepareGetRoute(url, query);
        var response = await httpClient.GetAsync(route, cancellationToken);
        var output = await GetOutput<TOutput>(response, cancellationToken);
        return (response, output);
    }

    public async Task<(HttpResponseMessage, TOutput?)> Delete<TOutput>(
        string url,
        CancellationToken cancellationToken
    )
        where TOutput : class
    {
        var response = await httpClient.DeleteAsync(url, cancellationToken);
        var output = await GetOutput<TOutput>(response, cancellationToken);
        return (response, output);
    }

    public async Task<(HttpResponseMessage, TOutput?)> Put<TOutput>(
        string url,
        object payload,
        CancellationToken cancellationToken
    )
        where TOutput : class
    {
        var response = await httpClient.PutAsJsonAsync(url, payload, cancellationToken);
        var output = await GetOutput<TOutput>(response, cancellationToken);
        return (response, output);
    }

    private async Task<TOutput?> GetOutput<TOutput>(
        HttpResponseMessage response,
        CancellationToken cancellationToken
    )
        where TOutput : class
    {
        TOutput? output = null;
        var outputString = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(outputString))
            output = JsonSerializer.Deserialize<TOutput>(
                outputString,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
        return output;
    }

    private string PrepareGetRoute(string route, object? queryParams)
    {
        if (queryParams is null)
            return route;
        var parameters = JsonSerializer.Serialize(queryParams);
        var dictionaryParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(
            parameters
        );
        return QueryHelpers.AddQueryString(route, dictionaryParams!);
    }
}
