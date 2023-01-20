using Net.Code.AdventOfCode.Toolkit.Core;

namespace Net.Code.AdventOfCode.Toolkit.Web;

using HtmlAgilityPack;

record PuzzleHtml(PuzzleKey key, string html, string input)
{
    public Puzzle GetPuzzle()
    {
        var document = new HtmlDocument();
        document.LoadHtml(html);

        var answers = (
            from node in document.DocumentNode.SelectNodes("//p")
            where node.InnerText.StartsWith("Your puzzle answer was")
            select node.SelectSingleNode("code").InnerText
            ).ToArray();

        var answer = answers.Length switch
        {
            2 => new Answer(answers[0], answers[1]),
            1 => new Answer(answers[0], string.Empty),
            0 => Answer.Empty,
            _ => throw new Exception($"expected 0, 1 or 2 answers, not {answers.Length}")
        };

        return Puzzle.Create(key, input, answer);
    }
}
