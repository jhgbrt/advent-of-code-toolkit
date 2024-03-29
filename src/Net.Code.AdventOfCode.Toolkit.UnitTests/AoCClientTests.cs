﻿using Microsoft.Extensions.Logging;

using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Web;

using NSubstitute;

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
        (string path, string content)[] items = new[]
        {
            (path: "settings", content: settings),
            (path: $"{Year}/leaderboard/private/view/148156.json", content: leaderboardjson),
            (path: $"{Year}/day/{Day}", content: puzzle),
            (path: $"{Year}/day/{Day}/input", content: "input"),
            (path: $"{Year}/leaderboard/private", content: leaderboard)
        };

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
            var wrapper = Substitute.For<IHttpClientWrapper>();
            var logger = Substitute.For<ILogger<AoCClient>>();

            var year = 2017;
            var day = 5;
            var path = $"{year}/day/{day}/answer";
            wrapper.PostAsync(path, Arg.Any<FormUrlEncodedContent>()).Returns(Task.FromResult((HttpStatusCode.OK, "<article>CONTENT</article>")));

            var client = new AoCClient(wrapper, logger);
            await client.PostAnswerAsync(year, day, 1, "ANSWER");

            await wrapper.Received().PostAsync(path, Arg.Any<HttpContent>());
        }
    }
}