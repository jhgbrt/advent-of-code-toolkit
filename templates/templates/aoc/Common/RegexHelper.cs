using System.Globalization;

namespace AdventOfCode;

internal static class RegexHelper
{
    public static T? As<T>(this Regex regex, string s, IFormatProvider? provider = null) where T: struct
    {
        var match = regex.Match(s);
        if (!match.Success) return default;

        var constructor = typeof(T).GetConstructors().Single();
        var parameters = constructor.GetParameters();

        var j = from p in parameters
                join m in match.Groups.OfType<Group>() on p.Name equals m.Name
                select Convert.ChangeType(m.Value, p.ParameterType, provider ?? CultureInfo.InvariantCulture);

        return (T)constructor.Invoke(j.ToArray());

    }

    public static int GetInt32(this Match m, string name) => int.Parse(m.Groups[name].Value);

}

record struct MyRecord1(string s1, string s2);
record struct MyRecord2(string s1, int i1);
record struct MyRecord3(string s1, decimal d1);
record struct MyRecord4(DateTime d1);

public class Tests
{
    ITestOutputHelper _output;

    public Tests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void CanParseRecordWithStringProperties()
    {
        var r = new Regex(@"--(?<s1>\w+)---(?<s2>\w+)");
        var result = r.As<MyRecord1>("--abc---def");
        Assert.True(result.HasValue);
        Assert.Equal(new MyRecord1("abc", "def"), result);
    }
    [Fact]
    public void CanParseRecordWithIntProperties()
    {
        var r = new Regex(@"--(?<s1>\w+)---(?<i1>\d+)--");
        var result = r.As<MyRecord2>("--abc---12--");
        Assert.True(result.HasValue);
        Assert.Equal(new MyRecord2("abc", 12), result);
    }

    [Fact]
    public void CanParseRecordWithDecimalProperties()
    {
        var r = new Regex(@"--(?<s1>\w+)---(?<d1>[\.,\d]+)--");
        var result = r.As<MyRecord3>("--abc---12,23--", CultureInfo.GetCultureInfoByIetfLanguageTag("nl"));
        _output.WriteLine(result.ToString());
        Assert.True(result.HasValue);
        Assert.Equal(new MyRecord3("abc", 12.23m), result);
    }

    [Fact]
    public void CanParseRecordWithDecimalPropertiesAndCulture()
    {
        var r = new Regex(@"--(?<s1>\w+)---(?<d1>[\.,\d]+)--");
        var result = r.As<MyRecord3>("--abc---12.23--", CultureInfo.InvariantCulture);
        _output.WriteLine(result.ToString());
        Assert.True(result.HasValue);
        Assert.Equal(new MyRecord3("abc", 12.23m), result);
    }
    [Fact]
    public void CanParseRecordWithDateProperty()
    {
        var r = new Regex(@"--(?<d1>.+)--");
        var result = r.As<MyRecord4>("--2021-12-31--");
        _output.WriteLine(result.ToString());
        Assert.True(result.HasValue);
        Assert.Equal(new MyRecord4(new DateTime(2021, 12, 31)), result);
    }

    [Fact]
    public void CanParseRecordWithDatePropertyAndSpecificCulture()
    {
        var r = new Regex(@"--(?<d1>.+)--");
        var result = r.As<MyRecord4>("--31/12/2021--", CultureInfo.GetCultureInfoByIetfLanguageTag("nl"));
        _output.WriteLine(result.ToString());
        Assert.True(result.HasValue);
        Assert.Equal(new MyRecord4(new DateTime(2021,12,31)), result);
    }
}
