namespace Net.Code.AdventOfCode.Toolkit.Web;
using System.Net;

using Microsoft.Extensions.Logging;

using Net.Code.AdventOfCode.Toolkit.Core;

class NotAuthenticatedException : Exception
{
    public NotAuthenticatedException() : base()
    {
    }

    public NotAuthenticatedException(string? message) : base(message)
    {
    }

    public NotAuthenticatedException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

class HttpClientWrapper : IHttpClientWrapper
{
    readonly HttpClientHandler handler;
    readonly HttpClient client;
    private readonly ILogger logger;
    private readonly string sessionCookie;
    public HttpClientWrapper(Configuration configuration, ILogger<HttpClientWrapper> logger)
    {
        var baseAddress = new Uri(configuration.BaseAddress);
        sessionCookie = configuration.SessionCookie;

        var cookieContainer = new CookieContainer();
        cookieContainer.Add(baseAddress, new Cookie("session", sessionCookie));

        handler = new HttpClientHandler { CookieContainer = cookieContainer };
        client = new HttpClient(handler) { BaseAddress = baseAddress };
        this.logger = logger;
    }
    private void EnsureAuthenticated()
    {
        if (string.IsNullOrEmpty(sessionCookie))
        {
            throw new NotAuthenticatedException("This command requires logging in, but the AOC_SESSION cookie is not set. " +
                "Log in to adventofcode.com, and find the 'session' cookie value in your browser devtools. " +
                "Copy this value and set it as an environment variable in your shell, or as a dotnet user-secret for your project.");
        }
    }
    public async Task<(HttpStatusCode status, string content)> PostAsync(string path, HttpContent body)
    {
        EnsureAuthenticated();
        var response = await client.PostAsync(path, body);
        logger.LogTrace("POST: {path} - {statusCode}", path, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        return (response.StatusCode, content);
    }
    public async Task<(HttpStatusCode status, string content)> GetAsync(string path)
    {
        EnsureAuthenticated();
        var response = await client.GetAsync(path);
        var content = await response.Content.ReadAsStringAsync();
        if (response.StatusCode != HttpStatusCode.OK)
        {
            logger.LogError("GET: {path} returned {statusCode}", path, response.StatusCode);
            logger.LogTrace("GET: {path} returned {statusCode}: {content}", path, response.StatusCode, content);
        }
        else
        {
            logger.LogTrace("GET: {path} - {statusCode}", path, response.StatusCode);
        }
        return (response.StatusCode, content);
    }
    public void Dispose()
    {
        client.Dispose();
        handler.Dispose();
    }

}
