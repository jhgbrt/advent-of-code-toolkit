using Microsoft.Extensions.Logging;

using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Logic;

using NSubstitute;

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Xunit;

namespace Net.Code.AdventOfCode.Toolkit.UnitTests
{
    public class AoCClientTests
    {
        IAoCClient CreateClient(string path, string content, HttpStatusCode statusCode)
        {
            var wrapper = Substitute.For<IHttpClientWrapper>();
            var logger = Substitute.For<ILogger<AoCClient>>();
            var year = DateTime.Now.Year;
            wrapper.GetAsync(path).Returns(Task.FromResult((statusCode, content)));
            var client = new AoCClient(wrapper, logger);
            return client;
        }
        [Fact]
        public async Task GetLeaderBoardIds_ValidContent_ExtractsCorrectIds()
        {
            var year = DateTime.Now.Year;
            var path = $"{year}/leaderboard/private";
            var content = File.ReadAllText("leaderboard.html");
            var client = CreateClient(path, content, HttpStatusCode.OK);
            var result = await client.GetLeaderboardIds();
            Assert.Equal(29328, result.First().id);
            Assert.Equal(148156, result.Last().id);
        }

        [Fact]
        public async Task GetLeaderBoard_RetrievesAndDeserializesLeaderboard()
        {
            var year = DateTime.Now.Year;
            var path = $"{year}/leaderboard/private/view/148156.json";
            var content = File.ReadAllText("leaderboard-148156.json");
            var client = CreateClient(path, content, HttpStatusCode.OK);
            var result = await client.GetLeaderBoardAsync(DateTime.Now.Year, 148156);
            Assert.NotNull(result);
            Assert.Equal("user1", result!.Members[1].Name);
            Assert.Equal(1, result.Members[1].Id);
            Assert.Equal(17, result.Members[2].TotalStars);
        }

        [Fact]
        public async Task GetLeaderBoard_WhenStatusCodeNotOk_ReturnsNull()
        {
            var year = DateTime.Now.Year;
            var path = $"{year}/leaderboard/private/view/148156.json";
            var content = File.ReadAllText("leaderboard-148156.json");
            var client = CreateClient(path, content, HttpStatusCode.NotFound);
            var result = await client.GetLeaderBoardAsync(DateTime.Now.Year, 148156);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetLeaderBoard_WhenReturnedContentIsHtml_ReturnsNull()
        {
            var year = DateTime.Now.Year;
            var path = $"{year}/leaderboard/private/view/148156.json";
            var content = "<html>";
            var client = CreateClient(path, content, HttpStatusCode.OK);
            var result = await client.GetLeaderBoardAsync(DateTime.Now.Year, 148156);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetMemberId_ExtractsMemberId()
        {
            var path = $"/settings";
            var content = File.ReadAllText("settings.html");
            var client = CreateClient(path, content, HttpStatusCode.OK);
            var result = await client.GetMemberId();
            Assert.Equal(148156, result);
        }

        [Fact]
        public async Task GetMemberAsync()
        {
            var wrapper = Substitute.For<IHttpClientWrapper>();
            var logger = Substitute.For<ILogger<AoCClient>>();
            var year = DateTime.Now.Year;

            wrapper.GetAsync($"/settings").Returns(Task.FromResult((HttpStatusCode.OK, File.ReadAllText("settings.html"))));
            wrapper.GetAsync($"{year}/leaderboard/private/view/148156.json").Returns(Task.FromResult((HttpStatusCode.OK, File.ReadAllText("leaderboard-148156.json"))));

            var client = new AoCClient(wrapper, logger);
            var result = await client.GetMemberAsync(year);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task PostAnswerAsync()
        {
            var wrapper = Substitute.For<IHttpClientWrapper>();
            var logger = Substitute.For<ILogger<AoCClient>>();

            wrapper.PostAsync(Arg.Any<string>(), Arg.Any<FormUrlEncodedContent>()).Returns(Task.FromResult((HttpStatusCode.OK, "<article>CONTENT</article>")));

            var client = new AoCClient(wrapper, logger);
            var result = await client.PostAnswerAsync(2017, 5, 1, "ANSWER");

            Assert.Equal(HttpStatusCode.OK, result.status);
            Assert.Equal("CONTENT", result.content);
        }

        [Fact]
        public async Task GetPuzzle_Answered()
        {
            var path = $"2015/day/1";
            var content = File.ReadAllText("puzzle-answered-both-parts.html");
            var client = CreateClient(path, content, HttpStatusCode.OK);
            var result = await client.GetPuzzleAsync(new(2015, 1));
            Assert.NotNull(result);
            Assert.Equal(2015, result.Year);
            Assert.Equal(1, result.Day);
            Assert.Equal(Status.Completed, result.Status);
            Assert.Equal("232", result.Answer.part1);
            Assert.Equal("1783", result.Answer.part2);
        }

        [Fact]
        public async Task GetPuzzle_Unanswered()
        {
            var path = $"2019/day/9";
            var content = File.ReadAllText("puzzle-unanswered.html");
            var client = CreateClient(path, content, HttpStatusCode.OK);
            var result = await client.GetPuzzleAsync(new(2019, 9));
            Assert.NotNull(result);
            Assert.Equal(2019, result.Year);
            Assert.Equal(9, result.Day);
            Assert.Equal(Status.Unlocked, result.Status);
            Assert.Equal("", result.Answer.part1);
            Assert.Equal("", result.Answer.part2);
        }
    }
}