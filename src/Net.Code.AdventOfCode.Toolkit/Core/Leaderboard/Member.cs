namespace Net.Code.AdventOfCode.Toolkit.Core;

using NodaTime;

record Member(int Id, string Name, int TotalStars, int LocalScore, int GlobalScore, Instant? LastStarTimeStamp, IReadOnlyDictionary<int, DailyStars> Stars);
