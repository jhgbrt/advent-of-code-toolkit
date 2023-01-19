using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Net.Code.AdventOfCode.Toolkit.Data;

public class TimeSpanConverter : ValueConverter<TimeSpan, long>
{
    public TimeSpanConverter()
        : base(
            v => v.Ticks,
            v => TimeSpan.FromTicks(v))
    {
    }
}
