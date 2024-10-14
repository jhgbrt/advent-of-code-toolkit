namespace Net.Code.AdventOfCode.Toolkit.Web;
using System.Net;

using Microsoft.Extensions.Logging;

using Net.Code.AdventOfCode.Toolkit.Core;

class NotAuthenticatedException : AoCException
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

class HttpClientWrapper(Configuration configuration, ILogger<HttpClientWrapper> logger, HttpClient client) : IHttpClientWrapper
{
    private async Task EnsureAuthenticated()
    {
        //if (string.IsNullOrEmpty(configuration.SessionCookie))
        //{
        //    throw new NotAuthenticatedException("This command requires logging in, but the AOC_SESSION cookie is not set. " +
        //        "Log in to adventofcode.com, and find the 'session' cookie value in your browser devtools. " +
        //        "Copy this value and set it as an environment variable in your shell, or as a dotnet user-secret for your project.");
        //}
        var response = await client.GetAsync($"{2015}/day/4/input");
        if (response.StatusCode != HttpStatusCode.OK)
        {
            logger.LogError("Unauthorized");
            throw new NotAuthenticatedException("This command requires logging in. The AOC_SESSION cookie is set, but may be expired. " +
                "Log in to adventofcode.com, and find the 'session' cookie value in your browser devtools. " +
                "Copy this value and set it as an environment variable in your shell, or as a dotnet user-secret for your project.");
        }
    }
    public async Task<(HttpStatusCode status, string content)> PostAsync(string path, HttpContent body)
    {
        await EnsureAuthenticated();
        var response = await client.PostAsync(path, body);
        logger.LogTrace("POST: {path} - {statusCode}", path, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        return (response.StatusCode, content);
    }
    public async Task<(HttpStatusCode status, string content)> GetAsync(string path)
    {
        await EnsureAuthenticated();
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

}
