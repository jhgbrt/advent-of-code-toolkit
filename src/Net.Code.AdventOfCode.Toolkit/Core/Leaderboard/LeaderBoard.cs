namespace Net.Code.AdventOfCode.Toolkit.Core;

record LeaderBoard(int OwnerId, int Year, IReadOnlyDictionary<int, Member> Members);
