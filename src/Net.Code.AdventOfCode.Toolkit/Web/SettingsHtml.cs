namespace Net.Code.AdventOfCode.Toolkit.Core;

using HtmlAgilityPack;

using System.Text.RegularExpressions;

partial record SettingsHtml(string html)
{
    public int GetMemberId()
    {
        var document = new HtmlDocument();
        document.LoadHtml(html);

        var text = (from node in document.DocumentNode.SelectNodes("//span")
                    where node.InnerText.Contains("anonymous user #")
                    select node.InnerText).Single();

        return int.Parse(IdRegex().Match(text).Groups["id"].Value);
    }

    [GeneratedRegex("#(?<id>\\d+)\\)")]
    private static partial Regex IdRegex();
}
