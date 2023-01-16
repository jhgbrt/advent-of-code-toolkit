
using Net.Code.AdventOfCode.Toolkit.Core;

namespace Net.Code.AdventOfCode.Toolkit.Logic;

class MemberManager : IMemberManager
{
    private readonly IAoCClient client;

    public MemberManager(IAoCClient client)
    {
        this.client = client;
    }
    public async IAsyncEnumerable<(int year, MemberStats stats)> GetMemberStats(IEnumerable<int> years)
    {
        foreach (var y in years)
        {
            var m = await client.GetMemberAsync(y);
            if (m == null) continue;
            yield return (y, new MemberStats(m.Name, m.TotalStars, m.LocalScore));
        }
    }
}
