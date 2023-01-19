namespace Net.Code.AdventOfCode.Toolkit.Core;

using HtmlAgilityPack;

using System.Text.RegularExpressions;

record SettingsHtml(string html)
{
    public int GetMemberId()
    {
        var document = new HtmlDocument();
        document.LoadHtml(html);

        var text = (from node in document.DocumentNode.SelectNodes("//span")
                    where node.InnerText.Contains("anonymous user #")
                    select node.InnerText).Single();

        return int.Parse(Regex.Match(text, @"#(?<id>\d+)\)").Groups["id"].Value);
    }
}
