using System.Net.Http.Json;
using System.Text.Json;

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
        CancellationToken cancellationToken
    )
        where TOutput : class
    {
        var response = await httpClient.GetAsync(url, cancellationToken);
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
}
