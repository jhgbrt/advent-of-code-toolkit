using Microsoft.Extensions.Logging;

using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Web;

using NSubstitute;

using RichardSzalay.MockHttp;

using System.IO;
using System.Net;

using Xunit.Abstractions;

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

        MockHttpMessageHandler wrapper;
        string baseAddress = "https://example.com";
        IAoCClient client;
        IAoCClient CreateClient()
        {
            var logger = Substitute.For<ILogger<AoCClient>>();
            wrapper.Expect($"{baseAddress}/2015/day/4/input").Respond(HttpStatusCode.OK, new StringContent("OK"));
            var client = new HttpClient(wrapper) { BaseAddress = new Uri(baseAddress) };
            return new AoCClient(client, logger);
        }

        public AoCClientGetTests()
        {
            wrapper = new MockHttpMessageHandler();
            client = CreateClient();
        }


        private async Task Verify()
        {
            wrapper.VerifyNoOutstandingExpectation();
            wrapper.VerifyNoOutstandingRequest();
            await Task.FromResult(0);
        }

        [Fact]
        public async Task GetLeaderBoardIds()
        {
            var path = $"{Year}/leaderboard/private";

            wrapper.Expect($"{baseAddress}/{path}").Respond(HttpStatusCode.OK, new StringContent(leaderboard));

            await client.GetLeaderboardIds(Year);
            await Verify();
        }

        [Fact]
        public async Task GetLeaderBoard()
        {
            var year = Year;
            wrapper.Expect($"{baseAddress}/2015/leaderboard/private/view/148156.json").Respond(HttpStatusCode.OK, new StringContent(leaderboardjson));
            await client.GetLeaderBoardAsync(year, 148156);
            await Verify();
        }

        [Fact]
        public async Task GetMemberId()
        {
            wrapper.Expect($"{baseAddress}/settings").Respond(HttpStatusCode.OK, new StringContent(settings));
            await client.GetMemberId();
            await Verify();
        }
        
        [Fact]
        public async Task GetMemberAsync()
        {
            wrapper.Expect($"{baseAddress}/settings").Respond(HttpStatusCode.OK, new StringContent(settings));
            wrapper.Expect($"{baseAddress}/{Year}/leaderboard/private/view/148156.json").Respond(HttpStatusCode.OK, new StringContent(leaderboardjson));
            await client.GetPersonalStatsAsync(Year);
            await Verify();
        }

        [Fact]
        public async Task GetPuzzle()
        {
            wrapper.Expect($"{baseAddress}/{Year}/day/{Day}").Respond(HttpStatusCode.OK, new StringContent(puzzle));
            wrapper.Expect($"{baseAddress}/{Year}/day/{Day}/input").Respond(HttpStatusCode.OK, new StringContent(puzzle));
            await client.GetPuzzleAsync(new(Year, Day));
            await Verify();
        }

    }

    public class AoCClientPostTests
    {
        MockHttpMessageHandler wrapper = new MockHttpMessageHandler();
        [Fact]
        public async Task PostAnswerAsync()
        {
            var client = CreateClient();

            await client.PostAnswerAsync(2017, 5, 1, "ANSWER");

            wrapper.VerifyNoOutstandingExpectation();
            wrapper.VerifyNoOutstandingRequest();
        }

        IAoCClient CreateClient()
        {
            var baseAddress = "https://example.com";
            var logger = Substitute.For<ILogger<AoCClient>>();
            wrapper.Expect($"{baseAddress}/2015/day/4/input").Respond(HttpStatusCode.OK, new StringContent("OK"));
            wrapper.Expect(HttpMethod.Post, $"{baseAddress}/2017/day/5/answer").Respond(HttpStatusCode.OK, new StringContent("<article>CONTENT</article>"));
            var client = new HttpClient(wrapper) { BaseAddress = new Uri(baseAddress) };
            return new AoCClient(client, logger);
        }


    }

}