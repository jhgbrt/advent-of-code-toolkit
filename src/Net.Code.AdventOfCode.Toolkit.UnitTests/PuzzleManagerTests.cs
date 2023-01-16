using Net.Code.AdventOfCode.Toolkit.Core;
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
        public async Task Sync_Calls_GetPuzzleWithoutCache()
        {
            var client = Substitute.For<IAoCClient>();
            var cache = Substitute.For<ICache>();
            var m = new PuzzleManager(client, cache);

            await m.Sync(2021, 1);

            await client.Received().GetPuzzleAsync(2021, 1, false);
        }

        [Fact]
        public async Task GetPuzzleResult_Calls_GetPuzzleWithCache()
        {
            var client = Substitute.For<IAoCClient>();
            var cache = Substitute.For<ICache>();
            var m = new PuzzleManager(client, cache);

            await m.GetPuzzleResult(2021, 1);

            await client.Received().GetPuzzleAsync(2021, 1, true);
        }

        [Theory]
        [InlineData(Status.Locked, false, 0)]
        [InlineData(Status.Unlocked, true, 1)]
        [InlineData(Status.AnsweredPart1, true, 2)]
        [InlineData(Status.Completed, false, 0)]
        public async Task PreparePost_WhenCalled(Status status, bool expectedResult, int expectedPart)
        {
            var client = Substitute.For<IAoCClient>();
            var cache = Substitute.For<ICache>();
            var m = new PuzzleManager(client, cache);

            var puzzle = new Puzzle(2021, 1, "", "", "", Answer.Empty, status);
            client.GetPuzzleAsync(2021, 1).Returns(Task.FromResult(puzzle));

            var result = await m.PreparePost(2021, 1);

            Assert.Equal((result.status, result.part), (status: expectedResult, part: expectedPart));
        }
        [Theory]
        [InlineData(HttpStatusCode.OK, "That's the right answer! asdfadf", true)]
        [InlineData(HttpStatusCode.OK, "That's NOT the right answer! asdfadf", false)]
        [InlineData(HttpStatusCode.Unauthorized, "whatever", false)]
        public async Task Post_WhenCalled(HttpStatusCode statusCode, string content, bool expectedSuccess)
        {
            var client = Substitute.For<IAoCClient>();
            var cache = Substitute.For<ICache>();
            var m = new PuzzleManager(client, cache);

            client.PostAnswerAsync(2021, 1, 1, "answer").Returns(Task.FromResult((statusCode, content)));
#pragma warning disable CS8620
            client.GetMemberAsync(2021, false).Returns(
                Task.FromResult(
                    new Member(1, "", 50, 123, 123, NodaTime.SystemClock.Instance.GetCurrentInstant(), new Dictionary<int, DailyStars>())
                ));

            var result = await m.Post(2021, 1, 1, "answer");

            Assert.Equal((success: expectedSuccess, status: statusCode), (result.success, result.status));
        }
    }
}

