﻿using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Web;

namespace Net.Code.AdventOfCode.Toolkit.UnitTests
{
    public class DeserializationTests
    {

        [Fact]
        public void Puzzle_Answered_BothParts()
        {
            var content = System.IO.File.ReadAllText("puzzle-answered-both-parts.html");
            var result = new PuzzleHtml(new(2015, 1), content, "input").GetPuzzle();

            Assert.NotNull(result);
            Assert.Equal(2015, result.Year);
            Assert.Equal(1, result.Day);
            Assert.Equal(Status.Completed, result.Status);
            Assert.Equal("232", result.Answer.part1);
            Assert.Equal("1783", result.Answer.part2);
        }

        [Fact]
        public void Puzzle_Unanswered()
        {
            var content = System.IO.File.ReadAllText("puzzle-unanswered.html");
            var result = new PuzzleHtml(new(2019, 9), content, "input").GetPuzzle();

            Assert.NotNull(result);
            Assert.Equal(2019, result.Year);
            Assert.Equal(9, result.Day);
            Assert.Equal(Status.Unlocked, result.Status);
            Assert.Equal("", result.Answer.part1);
            Assert.Equal("", result.Answer.part2);
        }

        [Fact]
        public void LeaderboardHtmlTest()
        {
            var content = System.IO.File.ReadAllText("leaderboard.html");
            var result = new LeaderboardHtml(content).GetLeaderboards().ToList();
            Assert.Equal(29328, result.First().id);
            Assert.Equal(148156, result.Last().id);
        }

        [Fact]
        public void LeaderboardJson()
        {
            var year = DateTime.Now.Year;
            var content = System.IO.File.ReadAllText("leaderboard-148156.json");
            var result = new LeaderboardJson(year, content).GetLeaderBoard();
            Assert.NotNull(result);
            Assert.Equal("user1", result!.Members[1].Name);
            Assert.Equal(1, result.Members[1].Id);
            Assert.Equal(17, result.Members[2].TotalStars);
        }

        [Fact]
        public void SettingsHtml()
        {
            var content = System.IO.File.ReadAllText("settings.html");
            var result = new SettingsHtml(content).GetMemberId();
            Assert.Equal(148156, result);
        }
    }
}