using Net.Code.AdventOfCode.Toolkit.Core;

namespace Net.Code.AdventOfCode.Toolkit.Web;

using NodaTime;

using System.Text.Json;

record LeaderboardJson(string content)
{
    public LeaderBoard GetLeaderBoard()
    {
        var jobject = JsonDocument.Parse(content).RootElement;

        var (ownerid, members, year) = jobject.EnumerateObject()
            .Aggregate(
                (ownerid: -1, members: Enumerable.Empty<PersonalStats>(), year: -1),
                (m, p) => p.Name switch
                {
                    "owner_id" => (GetInt32(p.Value), m.members, m.year),
                    "members" => (m.ownerid, GetMembers(p.Value), m.year),
                    "event" => (m.ownerid, m.members, GetInt32(p.Value)),
                    _ => m
                }
            );

        return new LeaderBoard(ownerid, year, members.ToDictionary(m => m.Id));

        int GetInt32(JsonElement value)
        {
            return value.ValueKind switch
            {
                JsonValueKind.Number => value.GetInt32(),
                JsonValueKind.String => int.Parse(value.GetString()!),
                _ => throw new InvalidOperationException("expected string or number")
            };
        }

        IEnumerable<PersonalStats> GetMembers(JsonElement element)
        {
            foreach (var item in element.EnumerateObject())
            {
                var member = item.Value;
                var result = new PersonalStats(0, string.Empty, 0, 0, 0, null, new Dictionary<int, DailyStars>());
                foreach (var property in member.EnumerateObject())
                {
                    result = property.Name switch
                    {
                        "name" => result with { Name = property.Value.GetString()! },
                        "id" => result with { Id = GetInt32(property.Value) },
                        "stars" when property.Value.ValueKind is JsonValueKind.Number => result with { TotalStars = property.Value.GetInt32() },
                        "local_score" when property.Value.ValueKind is JsonValueKind.Number => result with { LocalScore = property.Value.GetInt32() },
                        "global_score" when property.Value.ValueKind is JsonValueKind.Number => result with { GlobalScore = property.Value.GetInt32() },
                        "last_star_ts" when property.Value.ValueKind is JsonValueKind.Number => result with { LastStarTimeStamp = Instant.FromUnixTimeSeconds(property.Value.GetInt32()) },
                        "completion_day_level" => result with { Stars = GetCompletions(property).ToDictionary(x => x.Day) },
                        _ => result
                    };
                }
                yield return result;
            }
        }

        IEnumerable<DailyStars> GetCompletions(JsonProperty property)
        {
            foreach (var compl in property.Value.EnumerateObject())
            {
                var day = int.Parse(compl.Name);
                var ds = new DailyStars(day, null, null);

                foreach (var star in compl.Value.EnumerateObject())
                {
                    var instant = Instant.FromUnixTimeSeconds(star.Value.EnumerateObject().First().Value.GetInt32());
                    ds = int.Parse(star.Name) switch
                    {
                        1 => ds with { FirstStar = instant },
                        2 => ds with { SecondStar = instant },
                        _ => ds,
                    };
                }
                yield return ds;
            }

        }
    }
}
