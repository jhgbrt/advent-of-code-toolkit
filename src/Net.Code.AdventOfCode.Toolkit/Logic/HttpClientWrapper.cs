namespace Net.Code.AdventOfCode.Toolkit.Logic;
using System.Net;

using Microsoft.Extensions.Logging;

using Net.Code.AdventOfCode.Toolkit.Core;

class HttpClientWrapper: IHttpClientWrapper
{
    readonly HttpClientHandler handler;
    readonly HttpClient client;
    private readonly ILogger logger;
    public HttpClientWrapper(Configuration configuration, ILogger<HttpClientWrapper> logger)
    {
        var baseAddress = new Uri(configuration.BaseAddress);
        var sessionCookie = configuration.SessionCookie;

        var cookieContainer = new CookieContainer();
        cookieContainer.Add(baseAddress, new Cookie("session", sessionCookie));

        handler = new HttpClientHandler { CookieContainer = cookieContainer };

        client = new HttpClient(handler) { BaseAddress = baseAddress };
        this.logger = logger;
    }
    public async Task<(HttpStatusCode status, string content)> PostAsync(string path, HttpContent body)
    {
        var response = await client.PostAsync(path, body);
        logger.LogTrace($"GET: {path} - {response.StatusCode}");
        var content = await response.Content.ReadAsStringAsync();
        return (response.StatusCode, content);
    }
    public async Task<(HttpStatusCode status, string content)> GetAsync(string path)
    {
        var response = await client.GetAsync(path);
        var content = await response.Content.ReadAsStringAsync();
        logger.LogTrace($"GET: {path} - {response.StatusCode}");
        if (response.StatusCode != HttpStatusCode.OK)
        {
            logger.LogWarning($"GET: {path} returned {response.StatusCode}: {content}");
        }
        return (response.StatusCode, content);
    }
    public void Dispose()
    {
        client.Dispose();
        handler.Dispose();
    }

}
