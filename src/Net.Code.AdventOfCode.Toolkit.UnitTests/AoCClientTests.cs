using Microsoft.Extensions.Logging;

using Net.Code.AdventOfCode.Toolkit.Commands;
using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Web;

using NSubstitute;

using RichardSzalay.MockHttp;

using System.Net;

namespace Net.Code.AdventOfCode.Toolkit.UnitTests
{

    public class AoCClientGetTests
    {
        readonly static string settings = Content.ReadAllText("settings.html");
        readonly static string leaderboardjson = Content.ReadAllText("leaderboard-148156.json");
        readonly static string leaderboard = Content.ReadAllText("leaderboard.html");
        readonly static string puzzle = Content.ReadAllText("puzzle-answered-both-parts.html");
        const int Year = 2015;
        const int Day = 1;
        readonly static (string path, string content)[] items =
        [
            (path: "settings", content: settings),
            (path: $"{Year}/leaderboard/private/view/148156.json", content: leaderboardjson),
            (path: $"{Year}/day/{Day}", content: puzzle),
            (path: $"{Year}/day/{Day}/input", content: "input"),
            (path: $"{Year}/leaderboard/private", content: leaderboard),
            (path: $"2015/day/4/input", content: "OK")
        ];

        IHttpClientWrapper wrapper;
        IAoCClient client;
        IAoCClient CreateClient()
        {
            var logger = Substitute.For<ILogger<AoCClient>>();
            foreach (var (path,content) in items) 
            {
                wrapper.GetAsync(path).Returns(Task.FromResult((HttpStatusCode.OK, content)));
            }
            var client = new AoCClient(wrapper, logger);
            return client;
        }

        public AoCClientGetTests()
        {
            wrapper = Substitute.For<IHttpClientWrapper>();
            client = CreateClient();
        }


        private async Task Verify(params string[] paths)
        {
            foreach (var path in paths)
                await wrapper.Received().GetAsync(path);
        }

        [Fact]
        public async Task GetLeaderBoardIds()
        {
            var path = $"{Year}/leaderboard/private";
            await client.GetLeaderboardIds(Year);
            await Verify(path);
        }

        [Fact]
        public async Task GetLeaderBoard()
        {
            var year = Year;
            await client.GetLeaderBoardAsync(year, 148156);
            await Verify("2015/leaderboard/private/view/148156.json");
        }

        [Fact]
        public async Task GetMemberId()
        {
            await client.GetMemberId();
            await Verify("settings");
        }
        
        [Fact]
        public async Task GetMemberAsync()
        {
            await client.GetPersonalStatsAsync(Year);
            await Verify("settings", $"{Year}/leaderboard/private/view/148156.json");
        }

        [Fact]
        public async Task GetPuzzle()
        {
            await client.GetPuzzleAsync(new(Year, Day));
            await Verify($"{Year}/day/{Day}");
        }

        [Fact]
        public async Task GetPuzzleInput()
        {
            await client.GetPuzzleInputAsync(new(Year, Day));
            await Verify($"{Year}/day/{Day}/input");
        }
    }

    public class AoCClientPostTests
    {
        [Fact]
        public async Task PostAnswerAsync()
        {
            IHttpClientWrapper wrapper = CreateClient();
            var logger = Substitute.For<ILogger<AoCClient>>();

            var year = 2017;
            var day = 5;
            var path = $"{year}/day/{day}/answer";
            wrapper.PostAsync(path, Arg.Any<FormUrlEncodedContent>()).Returns(Task.FromResult((HttpStatusCode.OK, "<article>CONTENT</article>")));

            var client = new AoCClient(wrapper, logger);
            await client.PostAnswerAsync(year, day, 1, "ANSWER");

            await wrapper.Received().PostAsync(path, Arg.Any<HttpContent>());
        }

        private static IHttpClientWrapper CreateClient()
        {
            var wrapper = Substitute.For<IHttpClientWrapper>();
            wrapper.GetAsync("2015/day/4/input").Returns(Task.FromResult((HttpStatusCode.OK, "OK")));
            return wrapper;
        }
    }

    public class ClientWrapperTests
    {
        [Fact]
        public async Task GetAsync_WhenSessionCookieInvalid_ThrowsNotAuthenticated()
        {
            HttpClientWrapper wrapper = CreateClient("someCookie", false);
            await Assert.ThrowsAsync<NotAuthenticatedException>(() => wrapper.GetAsync("2017/day/1"));
        }

        [Fact]
        public async Task GetAsync_WhenSessionCookieValid_ThrowsNotAuthenticated()
        {
            HttpClientWrapper wrapper = CreateClient("someCookie", true);
            await wrapper.GetAsync("2017/day/1");
        }

        private static HttpClientWrapper CreateClient(string cookieValue, bool isValid)
        {

            var client = Mocks.HttpClient("https://example.com",
                (HttpMethod.Get, "2015/day/4/input", isValid ? HttpStatusCode.OK : HttpStatusCode.InternalServerError, new StringContent("")),
                (HttpMethod.Get, "2017/day/1", HttpStatusCode.OK, new StringContent(""))
                );
            var config = new Configuration("https://example.com", cookieValue);
            var wrapper = new HttpClientWrapper(config, Substitute.For<ILogger<HttpClientWrapper>>(), client);
            return wrapper;
        }


    }
}