
using Net.Code.AdventOfCode.Toolkit.Core;

using NodaTime;

using NSubstitute;

namespace Net.Code.AdventOfCode.Toolkit.UnitTests
{
    public class AoCLogicTests
    {
        AoCLogic AoCLogic = new AoCLogic();

        [Theory]
        [InlineData(2014, 1, 1, 0, 0, 0, 2014, 1)]
        [InlineData(2014, 12, 26, 0, 0, 0, 2014, 3)]
        [InlineData(2015, 1, 1, 0, 0, 0, 2015, 1)]
        [InlineData(2015, 11, 30, 23, 59, 59, 2015, 1)]
        [InlineData(2015, 12, 1, 0, 0, 0, 2015, 2)]
        [InlineData(2015, 12, 2, 0, 0, 0, 2015, 3)]
        [InlineData(2015, 12, 24, 0, 0, 0, 2015, 25)]
        [InlineData(2121, 12, 24, 0, 0, 0, 2121, 25)]
        public void WhenNowIsBeforeDayOfPuzzle_PuzzleIsInvalid(int year, int month, int day, int hour, int min, int sec, int pyear, int pday)
        {
            SetClock(year, month, day, hour, min, sec);
            Assert.False(AoCLogic.IsValidAndUnlocked(pyear, pday));
        }

        [Theory]
        [InlineData(2015, 12, 1, 0, 0, 0, 2015, 1)]
        [InlineData(2015, 12, 2, 0, 0, 0, 2015, 1)]
        [InlineData(2015, 12, 2, 0, 0, 0, 2015, 2)]
        [InlineData(2015, 12, 25, 0, 0, 0, 2015, 25)]
        [InlineData(2121, 12, 25, 0, 0, 0, 2121, 25)]
        [InlineData(2121, 12, 25, 0, 0, 0, 2121, 24)]
        public void WhenNowIsAfterDayOfPuzzle_PuzzleIsInvalid(int year, int month, int day, int hour, int min, int sec, int pyear, int pday)
        {
            SetClock(year, month, day, hour, min, sec);
            Assert.True(AoCLogic.IsValidAndUnlocked(pyear, pday));
        }

        [Fact]
        public void Years_ReturnsAllYearsFrom2015()
        {
            SetClock(2017, 1, 1, 0, 0, 0);
            Assert.Equal(new[] { 2015, 2016, 2017 }, AoCLogic.Years().ToArray());
        }

        [Fact]
        public void Puzzles_DuringAdvent_NoYearNoDay_ReturnsCurrentPuzzle()
        {
            SetClock(2017, 12, 23, 0, 0, 0);
            Assert.Equal(new[] { (2017, 23) }, AoCLogic.Puzzles(null, null).ToArray());
        }
        [Fact]
        public void Puzzles_OutsideAdvent_NoYearNoDay_ReturnsAllPastPuzzles()
        {
            SetClock(2018, 1, 26, 0, 0, 0);
            Assert.Equal(75, AoCLogic.Puzzles(null, null).Count());
            Assert.Equal((2015, 1), AoCLogic.Puzzles(null, null).First());
            Assert.Equal((2016, 1), AoCLogic.Puzzles(null, null).Skip(25).First());
            Assert.Equal((2017, 25), AoCLogic.Puzzles(null, null).Skip(25).Last());
        }
        [Fact]
        public void Puzzles_OutsideAdventInDecember_YearNoDay_ReturnsPuzzlesForYear()
        {
            SetClock(2017, 12, 26, 0, 0, 0);
            var result = AoCLogic.Puzzles(2017, null).ToArray();
            Assert.Equal(25, result.Count());
            Assert.Equal((2017, 1), result.First());
            Assert.Equal((2017, 25), result.Last());
        }
        [Fact]
        public void Puzzles_DuringAdvent_YearWithoutDay_ReturnsAllDaysForCurrentYear()
        {
            SetClock(2017, 12, 17, 0, 0, 0);
            var result = AoCLogic.Puzzles(2017, null).ToArray();
            Assert.Equal(17, result.Count());
            Assert.Equal((2017, 1), result.First());
            Assert.Equal((2017, 17), result.Last());
        }

        [Fact]
        public void Puzzles_DuringAdvent_NoYearWithDay_ReturnsSingleDay()
        {
            SetClock(2017, 12, 17, 0, 0, 0);
            Assert.Equal((2017,1), AoCLogic.Puzzles(null, 1).Single());
        }

        [Fact]
        public void Puzzles_OutsideAdvent_NoYearWithDay_Throws()
        {
            SetClock(2017, 12, 26, 0, 0, 0);
            Assert.Throws<ArgumentException>(() => AoCLogic.Puzzles(null, 1).ToList());
        }

        [Fact]
        public void Days_BeforeDecember_ReturnsNothing()
        {
            SetClock(2017, 11, 30, 0, 0, 0);
            Assert.Equal(Array.Empty<int>(), AoCLogic.Days(2017).ToArray());
        }

        [Fact]
        public void Days_DuringAdvent_ReturnsDaysBefore()
        {
            SetClock(2017, 12, 15, 0, 0, 0);
            Assert.Equal(Enumerable.Range(1, 15).ToArray(), AoCLogic.Days(2017).ToArray());
        }
        [Fact]
        public void Days_AfterAdvent_ReturnsAllDays()
        {
            SetClock(2017, 12, 26, 0, 0, 0);
            Assert.Equal(Enumerable.Range(1, 25).ToArray(), AoCLogic.Days(2017).ToArray());
        }

        [Fact]
        public void IsToday_OnCurrentDay_IsTrue()
        {
            SetClock(2017, 12, 5, 0, 0, 0);
            Assert.True(AoCLogic.IsToday(2017, 5));
        }
        [Fact]
        public void IsToday_OnOtherDay_IsFalse()
        {
            SetClock(2017, 12, 5, 0, 0, 0);
            Assert.False(AoCLogic.IsToday(2017, 3));
        }
        void SetClock(int year, int month, int day, int hour, int min, int sec)
        {
            var localdate = new LocalDateTime(year, month, day, hour, min, sec);
            var instant = localdate.InZoneLeniently(DateTimeZoneProviders.Tzdb["EST"]).ToInstant();
            var clock = Substitute.For<IClock>();
            clock.GetCurrentInstant().Returns(instant);
            AoCLogic = new AoCLogic(clock);
        }
    }
}