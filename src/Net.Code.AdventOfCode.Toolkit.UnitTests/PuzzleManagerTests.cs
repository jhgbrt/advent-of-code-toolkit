using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Logic;

using NSubstitute;

using System.Net;

namespace Net.Code.AdventOfCode.Toolkit.UnitTests
{
    public class PuzzleManagerTests
    {
        AoCLogic logic = new AoCLogic(TestClock.Create(2021, 12, 1, 8, 0, 0));
        [Fact]
        public async Task GetPuzzle_WhenPuzzleIsAvailable_DoesNotThrow()
        {
            var client = Substitute.For<IAoCClient>();
            var cache = Substitute.For<IAoCDbContext>();
            var m = new PuzzleManager(client, cache, logic);

            PuzzleKey key = new(2021, 1);
            cache.GetPuzzle(key).Returns(Puzzle.Create(key, "", Answer.Empty));

            var puzzle = await m.GetPuzzle(key);
        }
        [Fact]
        public async Task GetPuzzle_WhenPuzzleIsNotAvailable_DoesNotThrow()
        {
            var client = Substitute.For<IAoCClient>();
            var cache = Substitute.For<IAoCDbContext>();
            var m = new PuzzleManager(client, cache, logic);

            PuzzleKey key = new(2021, 1);

            cache.GetPuzzle(key).Returns(default(Puzzle));

            await Assert.ThrowsAsync<Exception>(() => m.GetPuzzle(key));
        }

        [Theory]
        [InlineData(HttpStatusCode.OK, "That's the right answer! asdfadf", Status.Unlocked, 1, true)]
        [InlineData(HttpStatusCode.OK, "That's NOT the right answer! asdfadf", Status.Unlocked, 1, false)]
        [InlineData(HttpStatusCode.Unauthorized, "whatever", Status.Unlocked, 1, false)]
        [InlineData(HttpStatusCode.OK, "That's the right answer! asdfadf", Status.AnsweredPart1, 2, true)]
        [InlineData(HttpStatusCode.OK, "That's NOT the right answer! asdfadf", Status.AnsweredPart1, 2, false)]
        [InlineData(HttpStatusCode.Unauthorized, "whatever", Status.AnsweredPart1, 2, false)]
        public async Task Post_WhenCalled(HttpStatusCode statusCode, string content, Status currentStatus, int part, bool expectedSuccess)
        {
            var client = Substitute.For<IAoCClient>();
            var cache = Substitute.For<IAoCDbContext>();

            PuzzleKey key = new(2021, 1);
            var puzzle = Puzzle.Create(key, "input", Answer.Empty);
            puzzle.Status = currentStatus;
            puzzle.Answer = part == 1 ? new("answer1", "") : new("answer1", "answer2");

            cache.GetPuzzle(key).Returns(puzzle);

            var m = new PuzzleManager(client, cache, logic);

            client.PostAnswerAsync(2021, 1, part, $"answer{part}").Returns((statusCode, content));
#pragma warning disable CS8620
            client.GetPersonalStatsAsync(2021).Returns(
                Task.FromResult(
                    new PersonalStats(1, "", 50, 123, 123, NodaTime.SystemClock.Instance.GetCurrentInstant(), new Dictionary<int, DailyStars>())
                ));

            var result = await m.PostAnswer(new(2021, 1), $"answer{part}");

            Assert.Equal(expectedSuccess, result.success);
        }
    }
}

