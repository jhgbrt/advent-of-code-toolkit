namespace Net.Code.AdventOfCode.Toolkit.Core;

using HtmlAgilityPack;

record PostAnswerResult(string html)
{
    public string GetResponse()
    {
        var document = new HtmlDocument();
        document.LoadHtml(html);
        var articles = document.DocumentNode.SelectNodes("//article").ToArray();
        return articles.First().InnerText;
    }
}
