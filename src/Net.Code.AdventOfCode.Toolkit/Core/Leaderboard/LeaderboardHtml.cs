﻿namespace Net.Code.AdventOfCode.Toolkit.Core;

using HtmlAgilityPack;

using System.Text.RegularExpressions;

record LeaderboardHtml(string html)
{
    public IEnumerable<(int id, string description)> GetLeaderboards()
    {
        var document = new HtmlDocument();
        document.LoadHtml(html);

        var link = new Regex(@"/\d+/leaderboard/private/view/(?<id>\d+)");

        var id =
            from a in document.DocumentNode.SelectNodes("//a")
            where a.InnerText == "[View]"
            let href = a.Attributes["href"].Value
            let match = link.Match(href)
            where match.Success
            let description = a.ParentNode.Name == "div" ? a.ParentNode.InnerText.Trim() : "Your own private leaderboard"
            select (int.Parse(match.Groups["id"].Value), description)
            ;

        return id;
    }
}
