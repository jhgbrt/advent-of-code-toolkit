using Net.Code.AdventOfCode.Toolkit.Core;

using Net.Code.AdventOfCode.Toolkit.Data;
using Net.Code.AdventOfCode.Toolkit.Logic;

using NSubstitute;

using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using Xunit;

namespace Net.Code.AdventOfCode.Toolkit.UnitTests
{
    public class PuzzleManagerTests
    {
        [Fact]
        public async Task GetPuzzle_Calls_GetPuzzleAsync()
        {
            var client = Substitute.For<IAoCClient>();
            var cache = Substitute.For<IAoCDbContext>();
            var m = new PuzzleManager(client, cache);

            PuzzleKey key = new(2021, 1);
            var puzzle = await m.GetPuzzle(key);

            await client.Received().GetPuzzleAsync(key);
        }


        [Theory]
        [InlineData(Status.Locked, false, 0)]
        [InlineData(Status.Unlocked, true, 1)]
        [InlineData(Status.AnsweredPart1, true, 2)]
        [InlineData(Status.Completed, false, 0)]
        public async Task PreparePost_WhenCalled(Status status, bool expectedResult, int expectedPart)
        {
            var client = Substitute.For<IAoCClient>();
            var cache = Substitute.For<IAoCDbContext>();
            var m = new PuzzleManager(client, cache);

            PuzzleKey key = new(2021, 1);
            var puzzle = new Puzzle(key, "input", Answer.Empty, status);
            client.GetPuzzleAsync(key).Returns(Task.FromResult(puzzle));

            var result = await m.PreparePost(new(2021, 1));

            Assert.Equal((result.status, result.part), (status: expectedResult, part: expectedPart));
        }
        [Theory]
        [InlineData(HttpStatusCode.OK, "That's the right answer! asdfadf", true)]
        [InlineData(HttpStatusCode.OK, "That's NOT the right answer! asdfadf", false)]
        [InlineData(HttpStatusCode.Unauthorized, "whatever", false)]
        public async Task Post_WhenCalled(HttpStatusCode statusCode, string content, bool expectedSuccess)
        {
            var client = Substitute.For<IAoCClient>();
            var cache = Substitute.For<IAoCDbContext>();

            PuzzleKey key = new(2021, 1);
            var answered = new Puzzle(key, "input", new Answer("answer", string.Empty), Status.AnsweredPart1);
            cache.GetPuzzle(key).Returns(answered);
            client.GetPuzzleAsync(key).Returns(answered);
            var m = new PuzzleManager(client, cache);

            client.PostAnswerAsync(2021, 1, 1, "answer").Returns(Task.FromResult((statusCode, content)));
#pragma warning disable CS8620
            client.GetPersonalStatsAsync(2021).Returns(
                Task.FromResult(
                    new PersonalStats(1, "", 50, 123, 123, NodaTime.SystemClock.Instance.GetCurrentInstant(), new Dictionary<int, DailyStars>())
                ));

            var result = await m.PostAnswer(new(2021, 1), new(1, "answer"));

            Assert.Equal(expectedSuccess, result.success);
        }
    }
}

